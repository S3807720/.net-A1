using MCBA.Models;
using MiscellaneousUtilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


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
                balance = X.Field<decimal>("Balance"),
                transactions = transactionsManager.getTransactions(X.Field<int>("AccountNumber"))
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
        public void selectAccount(Customer cust, String message, TransactionTypes transactionType)
        {
            List<Account> acc = cust.accounts;
            bool check = false;
            while (check == false)
            {
                Console.WriteLine(message);
                foreach (Account ac in acc)
                {
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
                                menuCheck = true;
                                return account;
                            }
                        }
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Please enter a valid whole number.");
                }
            }
              
            return null;
        }

        private void transferToAccount(Account account)
        {
            if (account.transactionFeeOrNot())
            {
                Console.WriteLine("You will be charged a service fee of $0.20. ");
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
            //sender
            addTransaction(account, new Transaction("T", account.accountNumber, destAcc.accountNumber, money, comment));
            //receiver
            addTransaction(destAcc, new Transaction("T", destAcc.accountNumber, money, comment));
            if (account.transactionFeeOrNot())
            {
                addTransaction(account, new Transaction("S", account.accountNumber, TRANSFER_FEE, "Transfer fee of $0.20."));
            }
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
            addTransaction(account, new Transaction("W", account.accountNumber, money, comment));
            if (account.transactionFeeOrNot())
            {
                addTransaction(account, new Transaction("S", account.accountNumber, WITHDRAWAL_FEE, "Withdrawal fee of $0.10."));
            }
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
            addTransaction(account, new Transaction("D", account.accountNumber, money, comment));
        }


        private void addTransaction(Account account, Transaction transaction)
        {
            account.addTransaction(transaction);
            using var connection = new SqlConnection(Utilities.connectionString);
            try { 
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"INSERT INTO [Transaction](TransactionType, AccountNumber, DestinationAccountNumber, Amount, Comment,TransactionTimeUtc) " +
                       $"VALUES(@TransactionType, @AccountNumber, @DestinationAccountNumber, @Amount, @Comment, @TransactionTimeUtc)";
                command.Parameters.AddWithValue("@TransactionType", transaction.transactionType);
                command.Parameters.AddWithValue("@AccountNumber", transaction.accountNumber);
                command.Parameters.AddWithValue("@Amount", transaction.amount);
                command.Parameters.AddWithValue("@Comment", transaction.comment);
                command.Parameters.AddWithValue("@TransactionTimeUtc", transaction.transactionTimeUtc);
                
                if (transaction.transactionType == "T" && transaction.destinationAccountNumber != 0)
                {
                    command.Parameters.AddWithValue("@DestinationAccountNumber", transaction.destinationAccountNumber);
                } else
                {
                    command.Parameters.AddWithValue("@DestinationAccountNumber", DBNull.Value);
                }
                int success = command.ExecuteNonQuery();
                if (success > 0)
                {
                    Console.WriteLine(transaction.ToString().Replace("@", Environment.NewLine));
                    var updateAcc = connection.CreateCommand();
                    command.CommandText = $"UPDATE Account SET Balance = @balance WHERE AccountNumber = @AccountID";
                    command.Parameters.AddWithValue("@balance", account.balance);
                    command.Parameters.AddWithValue("@AccountID", account.accountNumber);
                    command.ExecuteNonQuery();
                }
                else
                {
                    throw new Exception("Transaction could not be processed.");
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
