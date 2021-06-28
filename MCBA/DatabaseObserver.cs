using MCBA.Models;
using MiscellaneousUtilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBA
{
    class DatabaseObserver
    {
		string connectionString = Utilities.connectionString;
		//update account balance
		public async Task<int> UpdateAccount(Account acc)
        {
            acc.SetBalance();
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = $"UPDATE Account SET Balance = @balance WHERE AccountNumber = @AccountID";
            command.Parameters.AddWithValue("@balance", acc.balance);
            command.Parameters.AddWithValue("@AccountID", acc.accountNumber);
            await command.ExecuteNonQueryAsync();
			Menu.UpdateLogin();
			return 1;
		}
		//add transaction to db
		public async Task<int> AddTransaction(Transaction transaction)
        {
			using var connection = new SqlConnection(connectionString);
			await connection.OpenAsync();

			using var command = connection.CreateCommand();
			command.CommandText =
				"insert into [Transaction] (TransactionType, AccountNumber, DestinationAccountNumber," +
				"Amount, Comment, TransactionTimeUtc) values (@transactionType, @accountNumber, " +
				"@destinationAccountNumber, @amount, @comment, @transactionTimeUtc)";
			command.Parameters.AddWithValue("transactionType", transaction.transactionType);
			command.Parameters.AddWithValue("accountNumber", transaction.accountNumber);
			//either include dbnull or actual destination if exists
			if (transaction.destinationAccountNumber != null)
			{
				command.Parameters.AddWithValue("@DestinationAccountNumber", transaction.destinationAccountNumber);
			}
			else
			{
				command.Parameters.AddWithValue("@DestinationAccountNumber", DBNull.Value);
			}
			command.Parameters.AddWithValue("amount", transaction.amount);
			command.Parameters.AddWithValue("comment", (transaction.comment == null) ? DBNull.Value : transaction.comment);
			command.Parameters.AddWithValue("transactionTimeUtc", transaction.transactionTimeUtc);

			await command.ExecuteNonQueryAsync();
			return 1;
		}


    }
}
