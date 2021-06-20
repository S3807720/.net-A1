﻿using MCBA.Models;
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
    class TransactionsManager
    {
		private readonly string _connectionString;

		public TransactionsManager(string connectionString)
        {
			_connectionString = connectionString;
		}


		public List<Transaction> getTransactions()
        {
			using var connection = new SqlConnection(_connectionString);
			using var command = connection.CreateCommand();
			command.CommandText = "select * from Account";

			return command.GetDataTable().Select().Select(X => new Transaction
			{
				transactionId = X.Field<int>("TransactionID"),
				amount = X.Field<decimal>("Amount"),
				comment = X.Field<string>("Comment"),
				transactionTimeUtc = X.Field<DateTime>("TransactionTimeUtc")
			}).ToList();
		}
		public void InsertTransaction(Transaction transaction)
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Open();

			using var command = connection.CreateCommand();
			command.CommandText =
				"insert into [Transaction] (TransactionType, AccountNumber, DestinationAccountNumber," +
				"Amount, Comment, TransactionTimeUtc) values (@transactionType, @accountNumber, " +
				"@destinationAccountNumber, @amount, @comment, @transactionTimeUtc)";
			//command.Parameters.AddWithValue("transactionId", transaction.transactionId);
			command.Parameters.AddWithValue("transactionType", transaction.transactionType);
			command.Parameters.AddWithValue("accountNumber", transaction.accountNumber);
			command.Parameters.AddWithValue("destinationAccountNumber", transaction.destinationAccountNumber);
			command.Parameters.AddWithValue("amount", transaction.amount);
			command.Parameters.AddWithValue("comment", (transaction.comment == null) ? DBNull.Value : transaction.comment);
			command.Parameters.AddWithValue("transactionTimeUtc", transaction.transactionTimeUtc);

			command.ExecuteNonQuery();

		}
	}

}
