using System;
using System.Collections.Generic;

namespace MCBA.Models
{
    class Account
    {
        public int accountNumber { get; set; }
        public string accountType { get; set; }
        public int customerId { get; set; }
        public decimal balance{ get; set; }
        public List<Transaction> transactions { get; set; }

        public void SetBalance()
        {
            balance = 0;
            foreach (Transaction transacts in transactions)
            {
                if (transacts.destinationAccountNumber is null && 
                    !(transacts.transactionType is "S" or "W") )
                {
                    balance += transacts.amount;
                } else
                {
                    balance -= transacts.amount;
                }
                
            }
       }
        //check fee
        public bool TransactionFeeOrNot()
        {
            int counter = 0;
            foreach(Transaction trans in transactions)
            {
                if (trans.destinationAccountNumber is not null || trans.transactionType == "W")
                {
                    counter++;
                }
                if (counter >= 4)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddTransaction(Transaction transact)
        {
            transactions.Add(transact);
            SetBalance();
        }
        public override string ToString()
        {
            var type = accountType == "S" ? "Savings" : "Checking";
            return $"Account Number: {accountNumber}@Account Type: {type}@Balance: ${balance:0.00}@";
        }
    }
}
