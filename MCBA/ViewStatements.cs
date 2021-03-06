using MCBA.Models;
using System;

namespace MCBA
{
    class ViewStatements
    {
        public ViewStatements(Customer customer)
        {
            bool check = false;
            while (check == false)
            {
                Console.WriteLine("Please enter the account number of the account to view (0 to exit): ");
                foreach (Account acc in customer.accounts)
                {
                    Console.WriteLine(acc.ToString().Replace("@", Environment.NewLine));
                    Console.WriteLine();
                }

                var input = Console.ReadLine();
                try
                {
                    var choice = Convert.ToInt32(input);
                    bool found = false;
                    if (choice.Equals(0))
                    {
                        Console.WriteLine("Returning to menu..\n");
                        check = true;
                        break;
                    }
                    foreach (Account acc in customer.accounts)
                    {
                        if (acc.accountNumber.Equals(choice))
                        {
                            ViewStatement(acc);
                            found = true;
                        }
                    }
                    //no account, throw exception to print error 
                    if (found == false)
                    {
                        throw new FormatException();
                    }
                } catch(FormatException)
                {
                    Console.WriteLine($"Account #{input} does not exist.");
                } 
            }
            
            
        }

        private void ViewStatement(Account account)
        {
            var transactions = account.transactions;
            var start = 0;
            var end = 3;
            var menuChoice = false;
            Console.WriteLine($"Account #{account.accountNumber} has a balance of ${account.balance}.");
            while (menuChoice == false)
            {
                Console.Clear();
                for (int i = start; transactions.Count > end ? end >= i : transactions.Count > i; i++)
                {
                    //print each transaction for the account
                    Console.WriteLine();
                    Console.WriteLine(transactions[i]?.ToString().Replace("@", Environment.NewLine));
                }

                //show menu options based on transactions displayed
                var menu = "";
                if (transactions.Count > end+1)
                {
                    menu += "Q. Next page";
                }
                if (start > 3)
                {
                    menu += "\nW. Previous page";
                }
                menu += "\nE. Back.";

                //if transaction count is lower than the maximum page amount, change end to that
                end = end >= transactions.Count ? end = transactions.Count-1 : end;
                Console.WriteLine($"{start + 1}-{end+1} of {transactions.Count}\n{menu}");
                var input = Console.ReadLine().ToUpper();
                //enable page functions based on amount of transactions and what page the user is on
                if (transactions.Count > 3)
                {
                    //if start is 1, end will be 4 etc...
                    if (input == "Q" && end + 1 != transactions.Count)
                    {
                        start += 4;
                        end = start+3;
                    }
                    else if (input == "W" && start > 3)
                    {
                        start -= 4;
                        end = start+3;
                    }
                } if (input == "E")
                {
                    menuChoice = true;
                    break;
                } else
                {
                    Console.WriteLine($"{input} is not a menu option.");
                }
            }
        }
    }
}
