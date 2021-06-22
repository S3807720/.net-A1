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
    class CustomerManager
	{
		private readonly string _connectionString;

		public List<Customer> customers { get; }

		public CustomerManager()
        {
			_connectionString = Utilities.connectionString;

			using var connection = new SqlConnection(_connectionString);
			using var command = connection.CreateCommand();
			command.CommandText = "select * from Customer";

			var accountManager = new AccountManager();


			customers = command.GetDataTable().Select().Select(X => new Customer
			{
				customerId = X.Field<int>("CustomerID"),
				name = X.Field<string>("Name"),
				address = X.Field<string>("Address"),
				city = X.Field<string>("City"),
				postCode = X.Field<string>("PostCode"),
				accounts = accountManager.getAccounts(X.Field<int>("CustomerID"))
			}).ToList();

		}

		public void InsertCustomer(Customer customer)
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Open();

			using var command = connection.CreateCommand();
			command.CommandText =
				"insert into Customer (CustomerID, Name, Address, City, PostCode) values (@customerId, @name, " +
				"@address, @city, @postCode)";
			command.Parameters.AddWithValue("customerId", customer.customerId);
			command.Parameters.AddWithValue("name", customer.name);
			command.Parameters.AddWithValue("address", (customer.address == null) ? DBNull.Value : customer.address);
			command.Parameters.AddWithValue("city", (customer.city == null) ? DBNull.Value : customer.city);
			command.Parameters.AddWithValue("postCode", (customer.postCode == null) ? DBNull.Value : customer.postCode);

			command.ExecuteNonQuery();

		}
	}

}
