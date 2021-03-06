using MiscellaneousUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBA.Models
{
    public class Transaction
    {
        public int transactionId { get; set; }
        public string transactionType { get; set; }
        public int accountNumber { get; set; }
        public int? destinationAccountNumber { get; set; }
        public decimal amount { get; set; }
        public string comment { get; set;  }
        public DateTime transactionTimeUtc { get; set; }

        //transfer constructor
        public Transaction(string type, int accNum, int? destinationAcc, decimal amnt, string com)
        {
            transactionType = type;
            accountNumber = accNum;
            destinationAccountNumber = destinationAcc;
            amount = amnt;
            comment = com;
            transactionTimeUtc = DateTime.UtcNow;
        }
        public Transaction() {
            transactionTimeUtc = DateTime.UtcNow;
            destinationAccountNumber = null;
            transactionType = "D";
        }
        //deposit constructor
        public Transaction(string type, int accNum, decimal amnt, string com)
        {
            transactionType = type;
            accountNumber = accNum;
            amount = amnt;
            comment = com;
            destinationAccountNumber = null;
            transactionTimeUtc = DateTime.UtcNow;
        }
        //return string based on transact type w/ local time conversion
        public override string ToString()
        {
            string type = "";
            switch(transactionType)
            {
                case "D": type = "Deposit"; break;
                case "S": type = "Service Fee"; break;
                case "T": type = "Transfer"; break;
                case "W": type = "Withdraw"; break;

            }
            if (!(destinationAccountNumber is null))
            {
                return $"Transaction Type: {type} @Account Number: {accountNumber} "
               + $"@Destination Account Number: {destinationAccountNumber} " +
                $"@Amount: {amount:0.00}@Comment: {comment} @Transaction Time: {transactionTimeUtc.ToLocalTime()}@";
            } 
            else
            {
                return $"Transaction Type: {type} @Account Number: {accountNumber} "
               + $"@Amount: {amount:0.00}@Comment: {comment} @Transaction Time: {transactionTimeUtc.ToLocalTime()}@";
            }
        }
    }  
}
