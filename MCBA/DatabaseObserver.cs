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
        public void UpdateAccount(Account acc)
        {
            acc.setBalance();
            using var connection = new SqlConnection(Utilities.connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $"UPDATE Account SET Balance = @balance WHERE AccountNumber = @AccountID";
            command.Parameters.AddWithValue("@balance", acc.balance);
            command.Parameters.AddWithValue("@AccountID", acc.accountNumber);
            command.ExecuteNonQuery();
			Menu.UpdateLogin();
		}

        public void AddTransaction(Transaction transaction)
        {
			using var connection = new SqlConnection(Utilities.connectionString);
			connection.Open();

			using var command = connection.CreateCommand();
			command.CommandText =
				"insert into [Transaction] (TransactionType, AccountNumber, DestinationAccountNumber," +
				"Amount, Comment, TransactionTimeUtc) values (@transactionType, @accountNumber, " +
				"@destinationAccountNumber, @amount, @comment, @transactionTimeUtc)";
			command.Parameters.AddWithValue("transactionType", transaction.transactionType);
			command.Parameters.AddWithValue("accountNumber", transaction.accountNumber);
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

			command.ExecuteNonQuery();
		}

    }
}
