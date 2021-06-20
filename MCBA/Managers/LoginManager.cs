using MiscellaneousUtilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;


namespace MCBA.Managers
{
    class LoginManager
    {
		private readonly string _connectionString;

		List<Login> logins { get; }

		public LoginManager(string connectionString)
        {
			_connectionString = connectionString;

			using var connection = new SqlConnection(_connectionString);
			using var command = connection.CreateCommand();
			command.CommandText = "select * from Login";

			logins = command.GetDataTable().Select().Select(X => new Login
			{
				loginId = X.Field<string>("LoginID"),
				customerId = X.Field<int>("CustomerID"),
				passwordHash = X.Field<string>("PasswordHash")
			}).ToList();

		}

		public void InsertLogin(Login login)
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Open();

			using var command = connection.CreateCommand();
			command.CommandText =
				"insert into Login (LoginID, CustomerID, PasswordHash) values (@loginId, @customerId, @password)";
			command.Parameters.AddWithValue("loginId", login.loginId);
			command.Parameters.AddWithValue("customerId", login.customerId);
			command.Parameters.AddWithValue("password", login.passwordHash);

			command.ExecuteNonQuery();

		}
	}

}
