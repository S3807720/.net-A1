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

		public AccountManager(string connectionString)
        {
			_connectionString = connectionString;
		}

		public List<Account> getAccounts()
        {
			using var connection = new SqlConnection(_connectionString);
			using var command = connection.CreateCommand();
			command.CommandText = "select * from Account";
			var transactionsManager = new TransactionsManager(_connectionString);

			return command.GetDataTable().Select().Select(X => new Account
			{
				accountNumber = X.Field<int>("AccountNumber"),
				accountType = X.Field<char>("AccountType"),
				customerId = X.Field<int>("CustomerID"),
				transactions = transactionsManager.getTransactions(),
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
