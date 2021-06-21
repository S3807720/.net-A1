using MCBA.Managers;
using MCBA.Models;
using MiscellaneousUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;

public class Menu
{
	const string connectionString = "server=rmit.australiaeast.cloudapp.azure.com;" +
		"uid=s3807720_a1;pwd=abc123";

	public Menu()
	{
		addCustomerDataToDatabase();
		addLoginDataToDatabase();
		Console.WriteLine("Enter Login ID: ");
		String loginId = Console.ReadLine();
		Console.WriteLine("Enter Password: ");
		String password = Console.ReadLine();
		if (loginId != loginId)
        {
			Console.WriteLine("Invalid ID!");
        } else if (password != password)
        {
			Console.WriteLine("Invalid password!");
        } else
        {
			DisplayMenu();
        }

	}

	private void DisplayMenu()
    {
		int choice = 0;
		Console.WriteLine($"[1] Deposit\r\n" +
            $"[2] Withdraw\r\n" +
            $"[3] Transfer\r\n" +
            $"[4] My Statement\r\n" +
            $"[5] Logout\r\n" +
            $"[6] Exit\r\n" +
            $"\r\nEnter an option: ");
		while (choice > 6 || choice < 1)
        {
			var input = Console.ReadLine();
			try
			{
				choice = Convert.ToInt32(input);
				if (choice == 1)
                {
					DataTable table = DisconnectedAccess("Customer");
					foreach (var x in table.Select())
					{
						Console.WriteLine($"{x["Name"]}\n{x["CustomerID"]}\n{x["Address"]},{x["City"]}, {x["PostCode"]}");
					}
					table = DisconnectedAccess("[Transaction]");
					foreach (var x in table.Select())
					{
						Console.WriteLine($"{x["TransactionID"]}\n{x["TransactionType"]}\n" +
							$"{x["AccountNumber"]},{x["Amount"]}, {x["Comment"]}");
					}
				}
				// bit clunky, but a temp? workaround to throw the error msg anyway
				if (choice > 6)
                {
					throw new FormatException();
                }
			}
			catch (FormatException)
			{
				Console.WriteLine($"{input} is not a menu option.");
			}
			
		}
				
		GoToMenu(choice);
	}
	private DataTable DisconnectedAccess(string commandText)
    {
		using var connection = new SqlConnection(connectionString);
		connection.Open();
		var command = connection.CreateCommand();
		command.CommandText = $"select * from {commandText}";

		var table = new DataTable();
		new SqlDataAdapter(command).Fill(table);

		
		return table;
	}
	private void GoToMenu(int choice)
    {
		if (choice == 6)
        {
			Console.WriteLine("Exiting the application. Thanks for playing!");
			Environment.Exit(1);
        }
    }

	private void addCustomerDataToDatabase()
    {
		var customerManager = new CustomerManager(connectionString);
		if (customerManager.customers.Any())
        {
			return;
        }

		const string custDetailsAddr = "https://coreteaching01.csit.rmit.edu.au/~e87149/wdt/services/customers/";

		using var client = new HttpClient();
		var json = client.GetStringAsync(custDetailsAddr).Result;

		var customers = JsonConvert.DeserializeObject<List<Customer>>(json, new JsonSerializerSettings
		{
			// See here for DateTime format string documentation:
			// https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings

			DateFormatString = "dd/MM/yyyy hh:mm:ss tt"
		});
		var accountManager = new AccountManager(connectionString);
		var transactionsManager = new TransactionsManager(connectionString);
		foreach (var customer in customers)
		{
			customerManager.InsertCustomer(customer);

			foreach (var account in customer.accounts)
            {
				account.setBalance();
				account.customerId = customer.customerId;
				accountManager.InsertAccount(account);
				foreach(var transaction in account.transactions)
                {
					transaction.accountNumber = account.accountNumber;
					transaction.destinationAccountNumber = account.accountNumber;
					transaction.transactionType = "D";
					transaction.transactionId = Utilities.transactionIdCount++;
					transactionsManager.InsertTransaction(transaction);
                }
            }
		}
	}

	private void addLoginDataToDatabase()
    {
		var loginManager = new LoginManager(connectionString);
		if (loginManager.logins.Any())
        {
			return;
        }

		const string loginAddr = "https://coreteaching01.csit.rmit.edu.au/~e87149/wdt/services/logins/";		

		using var client = new HttpClient();
		var json = client.GetStringAsync(loginAddr).Result;

		var logins = JsonConvert.DeserializeObject<List<Login>>(json, new JsonSerializerSettings
		{
			// See here for DateTime format string documentation:
			// https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
			DateFormatString = "dd/MM/yyyy hh:mm:ss tt"
		});
		foreach(var login in logins)
        {
			Console.WriteLine(login.loginId + "\n" + login.customerId + "\n"+ login.passwordHash+"\n");
			loginManager.InsertLogin(login);
        }
	}
}
