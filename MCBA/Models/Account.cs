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

        public void setBalance()
        {
            balance = 0;
            foreach (Transaction transacts in transactions)
            {
                balance += transacts.amount;
            }
        }
        public void addTransaction(Transaction transact)
        {
            transactions.Add(transact);
            setBalance();
        }
        public override string ToString()
        {
            return $"Account Number: {accountNumber}@AccountType: {accountType}@Balance: {balance}@";
        }
    }
}
