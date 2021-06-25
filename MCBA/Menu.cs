using MCBA;
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
using System.Text;

public class Menu
{
	static Customer loggedIn = null;

	public static void updateLogin()
    {
		int id = loggedIn.customerId;
		var cs = new CustomerManager();
		foreach(Customer cust in cs.customers)
        {
			if (cust.customerId == id)
            {
				loggedIn = cust;
            }
        }
    }
	public Menu()
	{
		addCustomerDataToDatabase();
		addLoginDataToDatabase();
		login();
	}

	private async void login()
    {
		bool loginCheck = false;
		while (loginCheck == false)
		{
			Console.WriteLine("Enter Login ID: ");
			String loginId = Console.ReadLine();
			Console.WriteLine("Enter Password: ");
			String password = Utilities.HideCharacter();
			var logMgr = new LoginManager();
			var message = "";
			var id = logMgr.checkLogin(loginId, password, ref message);
			if (id == -1)
			{
				Console.WriteLine(message);
			}
			else
			{
				Console.Clear();
				loginCheck = true;
				setLogin(id);
			}
		}
		DisplayMenu();
	}

	private void setLogin(int id)
    {
		var customers = new CustomerManager().customers;
		foreach(Customer customer in customers)
        {
			if (id == customer.customerId)
            {
				loggedIn = customer;
				Console.WriteLine($"Welcome {customer.name}.\n");
            }
        }
    }

	private void DisplayMenu()
    {
		int choice = 0;
		string menu = $"[1] Deposit\r\n" +
			$"[2] Withdraw\r\n" +
			$"[3] Transfer\r\n" +
			$"[4] My Statement\r\n" +
			$"[5] Logout\r\n" +
			$"[6] Exit\r\n" +
			$"\r\nEnter an option: ";
		bool menuCheck = false;
		while (menuCheck == false)
        {
			Console.WriteLine(menu);
			var input = Console.ReadLine();
			try
			{
				choice = Convert.ToInt32(input);
				if (choice == 6)
                {
					Console.WriteLine("Exiting the application. Thanks for playing!");
					Environment.Exit(1);
				}
				// bit clunky, but a temp? workaround to throw the error msg anyway
				else if (choice > 6 || 1 > choice)
                {
					throw new FormatException();
                } else
                {
					GoToMenu(choice);
				}
			}
			catch (FormatException)
			{
				Console.WriteLine($"{input} is not a menu option.");
			}
			
		}
				
		
	}

	private void GoToMenu(int choice)
    {
		var a = new AccountManager();
		switch (choice)
        {
			case 1:
				a.selectAccount(loggedIn, "Please enter the account number of the account to deposit to.(0 to exit): ",
					TransactionTypes.Deposit);
				break;
			case 2:
				a.selectAccount(loggedIn, "Please enter the account number of the account to withdraw from.(0 to exit): ",
					TransactionTypes.Withdraw);
				break;
			case 3:
				a.selectAccount(loggedIn, "Please enter the account number of the account you wish to transfer from.(0 to exit): ",
					TransactionTypes.TransferOut);
				break;
			case 4:
				new ViewStatements(loggedIn);
				break;
			case 5:
				loggedIn = null;
				Console.Clear();
				login();
				break;
        }
		return;
    }

	private void addCustomerDataToDatabase()
    {
		var customerManager = new CustomerManager();
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
		var accountManager = new AccountManager();
		var transactionsManager = new TransactionsManager();
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
					transaction.destinationAccountNumber = transaction.destinationAccountNumber;
					transaction.transactionType = "D";
					transactionsManager.InsertTransaction(transaction);
                }
            }
		}
	}

	private void addLoginDataToDatabase()
    {
		var loginManager = new LoginManager();
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
			loginManager.InsertLogin(login);
        }
	}

}
