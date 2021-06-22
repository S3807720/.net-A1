using MCBA.Models;
using MiscellaneousUtilities;
using Newtonsoft.Json;
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
	}

}
