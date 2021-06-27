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
    class TransactionsManager
    {
		private readonly string _connectionString;

		public TransactionsManager()
        {
			_connectionString = Utilities.connectionString; ;
		}


		public List<Transaction> getTransactions(int accNum)
        {
			using var connection = new SqlConnection(_connectionString);
			using var command = connection.CreateCommand();
			command.CommandText = "select * from [Transaction] where AccountNumber = @accNum";
			command.Parameters.AddWithValue("@accNum", accNum);
			return command.GetDataTable().Select().Select(X => new Transaction
			{
				transactionId = X.Field<int>("TransactionID"),
				transactionType = X.Field<string>("TransactionType"),
				destinationAccountNumber = X.Field<int?>("DestinationAccountNumber").Equals(DBNull.Value) ? null : X.Field<int?>("DestinationAccountNumber"),
				accountNumber = accNum,
				amount = X.Field<decimal>("Amount"),
				comment = X.Field<string>("Comment"),
				transactionTimeUtc = X.Field<DateTime>("TransactionTimeUtc")
			}).ToList();
		}
		
	}

}
