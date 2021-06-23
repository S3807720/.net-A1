using MCBA.Models;
using MiscellaneousUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;


namespace MCBA.Managers
{
    class AccountManager
    {
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
        public void selectAccount(Customer cust, String message, int transactionType)
        {
            List<Account> acc = cust.accounts;
            bool check = false;
            while (check == false)
            {
                Console.WriteLine(message);
                foreach (Account ac in acc)
                {
                    Console.WriteLine(ac.ToString().Replace("@", Environment.NewLine));
                    Console.WriteLine();
                }

                var input = Console.ReadLine();
                try
                {
                    var choice = Convert.ToInt32(input);
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
                            if (transactionType == 0)
                            {
                                depositToAccount(ac);
                            }
                            else if (transactionType == 1)
                            {
                                //withd
                            }
                            else
                            {
                                //xfer
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
        private void depositToAccount(Account account)
        {
            var money = getMoney($"Enter an amount to deposit to Account #{account.accountNumber} (0 to exit).");
            if (money == 0)
            {
                Console.WriteLine("Deposit cancelled.");
                return;
            }
            Console.WriteLine("Enter a comment for your deposit(blank to skip): ");
            var comment = Console.ReadLine();
            addTransaction(account, new Transaction("D", account.accountNumber, money, comment));
        }

        private void addTransaction(Account account, Transaction transaction)
        {
            account.addTransaction(transaction);
            using var connection = new SqlConnection(Utilities.connectionString);
            try { 
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"INSERT INTO [Transaction](TransactionType, AccountNumber, Amount, Comment,TransactionTimeUtc) " +
                       $"VALUES(@TransactionType, @AccountNumber, @Amount, @Comment, @TransactionTimeUtc)";
                command.Parameters.AddWithValue("@TransactionType", transaction.transactionType);
                command.Parameters.AddWithValue("@AccountNumber", transaction.accountNumber);
                command.Parameters.AddWithValue("@Amount", transaction.amount);
                command.Parameters.AddWithValue("@Comment", transaction.comment);
                command.Parameters.AddWithValue("@TransactionTimeUtc", transaction.transactionTimeUtc);
                if (transaction.transactionType == "T" || transaction.transactionType == "S")
                {
                    command.Parameters.AddWithValue("@DestinationAccountNumber", transaction.destinationAccountNumber);
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
                    throw new Exception("Transaction could not be completed.");
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
