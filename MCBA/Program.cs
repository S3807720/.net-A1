using MiscellaneousUtilities;
using System;
using System.Data.SqlClient;

namespace MCBA
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var conn = new SqlConnection(Utilities.connectionString))
            {
                if (conn.IsAvailable())
                {
                    new Menu();
                } else
                {
                    Console.WriteLine("Cannot connect to the database. Exiting application.");
                    Environment.Exit(1);
                }
            }
        }
    }
}
