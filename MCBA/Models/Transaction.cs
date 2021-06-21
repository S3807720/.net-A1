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
        public int destinationAccountNumber { get; set; }
        public decimal amount { get; set; }
        public string comment { get; set;  }
        public DateTime transactionTimeUtc { get; set; }

        public Transaction() {
            transactionTimeUtc = DateTime.UtcNow;
            transactionType = "D";
        }
    }  
}
