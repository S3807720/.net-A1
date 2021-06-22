using MCBA.Managers;
using MCBA.Models;
using MiscellaneousUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBA
{
    class ViewStatements
    {
        public ViewStatements(Account account)
        {
            var transactions = account.transactions;
            var start = 0; 
            var end = 3;
            var menuChoice = false;
            Console.WriteLine($"Account #{account.accountNumber} has a balance of ${account.balance}.");
            while (menuChoice == false)
            {
                for (int i = start; transactions.Count > end? end >= i : transactions.Count > i; i++)
                {
                    //print each transaction for the account
                    Console.WriteLine(transactions[i]?.ToString().Replace("@", Environment.NewLine));
                    Console.WriteLine();
                }

                //show menu options based on transactions displayed
                var menu = "";
                if (transactions.Count > end)
                {
                    menu += "Q. Next page";
                }
                if (start > 3)
                {
                    menu += "\nW. Previous page";
                }
                menu += "\nE. Back.";
                
                //if transaction count is lower than the maximum page amount, change end to that
                if (end > transactions.Count) end = transactions.Count;
                Console.WriteLine($"{start+1}-{end} of {transactions.Count}\n{menu}");
                var input = Console.ReadLine().ToUpper();
                //enable page functions based on amount of transactions and what page the user is on
                if (transactions.Count > end)
                {
                    if (input == "Q")
                    {
                        start += 4;
                        end += 4;
                    }
                    else if (input == "W" && start > 3)
                    {
                        start -= 4;
                        end -= 4;
                    }
                }                
                if (input == "E")
                {
                    return;
                } else
                {
                    Console.WriteLine($"{input} is not a menu option.");
                }
            }

        }
    }
}
