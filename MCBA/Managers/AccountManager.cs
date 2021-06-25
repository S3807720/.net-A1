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

        public AccountManager()
        {
            _connectionString = Utilities.connectionString; ;
        }

        public List<Account> getAccounts(int customerID)
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
                transactions = transactionsManager.getTransactions(X.Field<int>("AccountNumber")),
                balance = X.Field<decimal>("Balance")
            }).ToList();
        }
        public void InsertAccount(Account account)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                "insert into Account (AccountNumber, AccountType, CustomerID, Balance) values (@accountNumber, @accountType, @customerId, @balance)";
            command.Parameters.AddWithValue("accountNumber", account.accountNumber);
            command.Parameters.AddWithValue("accountType", account.accountType);
            command.Parameters.AddWithValue("customerId", account.customerId);
            command.Parameters.AddWithValue("balance", account.balance);
            
            command.ExecuteNonQuery();

        }

        public void updateAccountBalance(Account account)
        {
            account.setBalance();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $"UPDATE Account SET Balance = @balance WHERE AccountNumber = @AccountID";
            command.Parameters.AddWithValue("@balance", account.balance);
            command.Parameters.AddWithValue("@AccountID", account.accountNumber);
            command.ExecuteNonQuery();

        }

        public void selectAccount(Customer cust, String message, TransactionTypes transactionType)
        {
            List<Account> acc = cust.accounts;
            bool check = false;
            while (check == false)
            {
                Console.WriteLine(message);
                
                foreach (Account ac in acc)
                {
                    ac.setBalance();
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
                                depositToAccount(ac);
                            }
                            else if (transactionType == TransactionTypes.Withdraw)
                            {
                                withdrawToAccount(ac);
                            }
                            else
                            {
                                transferToAccount(ac);
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
       
        private bool checkAccountBalance(Account account, decimal money)
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
            } else
            {
                return true;
            }
        }
        
        private Account getDestinationAccount()
        {
            Console.WriteLine("Enter the account number of the account to transfer to(0 to exit): ");
            var input = Console.ReadLine();
            bool menuCheck = false;
            bool found = false;
            while (menuCheck == false)
            {
                try
                {
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
                    Console.WriteLine("Please enter a valid whole number.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
              
            return null;
        }

        private void transferToAccount(Account account)
        {
            if (account.transactionFeeOrNot())
            {
                Console.WriteLine("You will be charged a service fee of $0.20.");
            }
            Account destAcc = getDestinationAccount();
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
            var money = getMoney($"Enter an amount to transfer from Account #{account.accountNumber} to account #{destAcc.accountNumber}(0 to exit).");
            if (!checkAccountBalance(account, money))
            {
                return;
            }
            var comment = getComment("Enter a comment for your withdrawal(blank to skip): ");
            var tasks = new Task[]
            {
                //sender
                addTransaction(account, new Transaction("T", account.accountNumber, destAcc.accountNumber, money, comment)),
                //receiver
                addTransaction(destAcc, new Transaction("T", destAcc.accountNumber, money, comment)),
                account.transactionFeeOrNot()? addTransaction(account, new Transaction("S", account.accountNumber, TRANSFER_FEE, "Transfer fee of $0.20.")) : null
            };

           // if (account.transactionFeeOrNot())
           // {
           //     addTransaction(account, new Transaction("S", account.accountNumber, TRANSFER_FEE, "Transfer fee of $0.20."));
           // }
            Console.WriteLine("Transaction successfully processed.");
            
        }

        private void withdrawToAccount(Account account)
        {
            if (account.transactionFeeOrNot())
            {
                Console.WriteLine("You will be charged a service fee of $0.10.");
            }
            var money = getMoney($"Enter an amount to withdraw from Account #{account.accountNumber} (0 to exit).");
            if (!checkAccountBalance(account, money))
            {
                return;
            }
            var comment = getComment("Enter a comment for your withdrawal(blank to skip): ");

            var tasks = new Task[]
            {
                addTransaction(account, new Transaction("W", account.accountNumber, money, comment)),
                account.transactionFeeOrNot()? addTransaction(account, new Transaction("S", account.accountNumber, WITHDRAWAL_FEE, "Withdrawal fee of $0.10.")) : null
            };

         //   addTransaction(account, new Transaction("W", account.accountNumber, money, comment));
       //     if (account.transactionFeeOrNot())
       //     {
             //   addTransaction(account, new Transaction("S", account.accountNumber, WITHDRAWAL_FEE, "Withdrawal fee of $0.10."));
         //   }
            Console.WriteLine("Transaction successfully processed.");
        }

        private void depositToAccount(Account account)
        {
            var money = getMoney($"Enter an amount to deposit to Account #{account.accountNumber} (0 to exit).");
            if (money == 0)
            {
                Console.WriteLine("Deposit cancelled.");
                return;
            }
            var comment = getComment("Enter a comment for your deposit(blank to skip): ");
            _ = addTransaction(account, new Transaction("D", account.accountNumber, money, comment));
            Console.WriteLine("Transaction successfully processed.");
        }


        private async Task addTransaction(Account account, Transaction transaction)
        {
            var ts = new TransactionsManager();
            ts.InsertTransaction(transaction);
            account.setBalance();
            updateAccountBalance(account);
            Menu.updateLogin();
            Console.WriteLine(transaction);
        }

        private decimal getMoney(string message)
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

        private string getComment(string message)
        {
            Console.WriteLine(message);
            var comment = Console.ReadLine();
            Console.WriteLine();
            return comment;
        }

    }
}
