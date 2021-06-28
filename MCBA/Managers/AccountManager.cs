using MCBA.Models;
using MiscellaneousUtilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace MCBA.Managers
{
    class AccountManager
    {
        private const decimal WITHDRAWAL_FEE = 0.10M;
        private const decimal TRANSFER_FEE = 0.20M;
        private const decimal MINIMUM_CHECKINGS_FUNDS = 200;
        private readonly string _connectionString;
        private HashSet<DatabaseObserver> _observers = new HashSet<DatabaseObserver>();

        public AccountManager()
        {
            _connectionString = Utilities.connectionString; 
        }

        public List<Account> GetAccounts(int customerID)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "select * from Account where CustomerID = @custId";
            command.Parameters.AddWithValue("@custId", customerID);
            var transactionsManager = new TransactionsManager();

            return command.GetDataTable().Select().Select(X => new Account
            {
                accountNumber = X.Field<int>("AccountNumber"),
                accountType = X.Field<string>("AccountType"),
                customerId = X.Field<int>("CustomerID"),
                transactions = transactionsManager.GetTransactions(X.Field<int>("AccountNumber")),
                balance = X.Field<decimal>("Balance")
            }).ToList();
        }
        public async Task<int> InsertAccount(Account account)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText =
                "insert into Account (AccountNumber, AccountType, CustomerID, Balance) values (@accountNumber, @accountType, @customerId, @balance)";
            command.Parameters.AddWithValue("accountNumber", account.accountNumber);
            command.Parameters.AddWithValue("accountType", account.accountType);
            command.Parameters.AddWithValue("customerId", account.customerId);
            command.Parameters.AddWithValue("balance", account.balance);

            await command.ExecuteNonQueryAsync();
            return 1;
        }



        public void SelectAccount(Customer cust, String message, TransactionTypes transactionType)
        {
            List<Account> acc = cust.accounts;
            bool check = false;
            while (check == false)
            {
                Console.WriteLine(message);

                foreach (Account ac in acc)
                {
                    ac.SetBalance();
                    Console.WriteLine(ac.ToString().Replace("@", Environment.NewLine));
                }


                var input = Console.ReadLine();
                try
                {
                    var choice = Convert.ToInt32(input);
                    Console.WriteLine();
                    bool found = false;
                    if (choice.Equals(0))
                    {
                        Console.WriteLine("Returning to menu..\n");
                        check = true;
                        break;
                    }
                    foreach (Account ac in acc)
                    {
                        if (ac.accountNumber.Equals(choice))
                        {
                            if (transactionType == TransactionTypes.Deposit)
                            {
                                DepositToAccount(ac);
                            }
                            else if (transactionType == TransactionTypes.Withdraw)
                            {
                                WithdrawToAccount(ac);
                            }
                            else
                            {
                                TransferToAccount(ac);
                            }
                            found = true;
                            check = true;
                        }
                    }
                    //no account, throw exception to print error 
                    if (found == false)
                    {
                        throw new FormatException();
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Account #{input} does not exist.");
                }
            }
        }

        private bool CheckAccountBalance(Account account, decimal money)
        {
            if (money > account.balance || (account.accountType == "C" && MINIMUM_CHECKINGS_FUNDS > account.balance - money))
            {
                Console.WriteLine("You do not have enough money in your account to withdraw that much.");
                return false;
            } 
            else if (money == 0)
            {
                Console.WriteLine("Deposit cancelled.");
                return false;
            }
            else
            {
                return true;
            }
        }

        private Account GetDestinationAccount()
        {
            Console.WriteLine("Enter the account number of the account to transfer to(0 to exit): ");
            bool menuCheck = false;
            bool found = false;
            while (menuCheck == false)
            {
                try
                {
                    var input = Console.ReadLine();
                    var accountNum = Convert.ToInt32(input);
                    if (accountNum == 0)
                    {
                        menuCheck = true;
                    }
                    Console.WriteLine();
                    //loop through each customer and account to find a match..
                    var customers = new CustomerManager();
                    foreach (Customer customer in customers.customers)
                    {
                        foreach (Account account in customer.accounts)
                        {
                            if (account.accountNumber == accountNum)
                            {
                                customer.accounts.Remove(account);
                                menuCheck = true;
                                found = true;
                                return account;
                            }
                        }
                    }
                    //no account, throw exception to print error 
                    if (found == false)
                    {
                        throw new Exception("That account does not exist.");
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Account numbers can only be numeric, no letters or special characters!");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return null;
        }
        //transfer function, similar to withdraw, but with a destination account & more error checks
        private void TransferToAccount(Account account)
        {
            if (account.TransactionFeeOrNot())
            {
                Console.WriteLine("You will be charged a service fee of $0.20.");
            }
            Account destAcc = GetDestinationAccount();
            if (destAcc.accountNumber == account.accountNumber)
            {
                Console.WriteLine("You cannot transfer to the same account.");
                return;
            }
            if (destAcc == null)
            {
                Console.WriteLine("Returning to menu..");
                return;
            }
            var money = GetMoney($"Enter an amount to transfer from Account #{account.accountNumber} to account #{destAcc.accountNumber}(0 to exit).");
            if (!CheckAccountBalance(account, account.TransactionFeeOrNot() ? money+TRANSFER_FEE : money))
            {
                return;
            }
            var comment = GetComment("Enter a comment for your withdrawal(blank to skip): ");

            //sender
            AddTransaction(account, new Transaction("T", account.accountNumber, destAcc.accountNumber, money, comment));
            //receiver
            AddTransaction(destAcc, new Transaction("T", destAcc.accountNumber, money, comment));
            if (account.TransactionFeeOrNot())
            {
                AddTransaction(account, new Transaction("S", account.accountNumber, TRANSFER_FEE, "Transfer fee of $0.20."));
            }
            Console.WriteLine("Transaction successfully processed.");

        }
        //withdraw function, fee if past the threshold of 4 freebies
        private void WithdrawToAccount(Account account)
        {
            if (account.TransactionFeeOrNot())
            {
                Console.WriteLine("You will be charged a service fee of $0.10.");
            }
            var money = GetMoney($"Enter an amount to withdraw from Account #{account.accountNumber} (0 to exit).");
            if (!CheckAccountBalance(account, account.TransactionFeeOrNot() ? money + WITHDRAWAL_FEE : money))
            {
                return;
            }
            var comment = GetComment("Enter a comment for your withdrawal(blank to skip): ");


            AddTransaction(account, new Transaction("W", account.accountNumber, money, comment));
            if (account.TransactionFeeOrNot())
            {
                AddTransaction(account, new Transaction("S", account.accountNumber, WITHDRAWAL_FEE, "Withdrawal fee of $0.10."));
            }
            Console.WriteLine("Transaction successfully processed.");
        }
        //deposit function
        private void DepositToAccount(Account account)
        {
            var money = GetMoney($"Enter an amount to deposit to Account #{account.accountNumber} (0 to exit).");
            if (money == 0)
            {
                Console.WriteLine("Deposit cancelled.");
                return;
            }
            var comment = GetComment("Enter a comment for your deposit(blank to skip): ");
            AddTransaction(account, new Transaction("D", account.accountNumber, money, comment));
            Console.WriteLine("Transaction successfully processed.");
        }
        //add to account & activate observer to update db
        private void AddTransaction(Account account, Transaction transaction)
        {
            account.AddTransaction(transaction);
            Notify(account, transaction);
        }
        //grab input for transaction amount
        private decimal GetMoney(string message)
        {
            Console.WriteLine(message);
            bool check = false;
            while (check == false)
            {
                var input = Console.ReadLine();
                try
                {
                    var money = Convert.ToDecimal(input);
                    if (money == 0)
                    {
                        check = true;
                    }
                    else if (money < 0)
                    {
                        throw new FormatException();
                    }
                    else
                    {
                        return money;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Please enter a positive number.");
                }
            }
            return 0;
        }
        //grab comment input..
        private string GetComment(string message)
        {
            Console.WriteLine(message);
            var comment = Console.ReadLine();
            Console.WriteLine();
            return comment;
        }

        //observer stuff
        public void Register(DatabaseObserver observer)
        {
            _observers.Add(observer);
        }
        public void UnRegister(DatabaseObserver observer)
        {
            _observers.Remove(observer);
        }
        public async void Notify(Account account, Transaction transaction)
        {
            foreach (DatabaseObserver o in _observers)
            {
                await o.AddTransaction(transaction);
                await o.UpdateAccount(account);
            }
        }

    }
}
