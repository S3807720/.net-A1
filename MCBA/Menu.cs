using System;

public class Menu
{
	public Menu()
	{
		Console.WriteLine("Enter Login ID: ");
		String loginId = Console.ReadLine();
		Console.WriteLine("Enter Password: ");
		String password = Console.ReadLine();
		if (loginId != loginId)
        {
			Console.WriteLine("Invalid ID!");
        } else if (password != password)
        {
			Console.WriteLine("Invalid password!");
        } else
        {
			DisplayMenu();
        }

	}

	private void DisplayMenu()
    {
		int choice = 0;
		Console.WriteLine($"[1] Deposit\r\n" +
            $"[2] Withdraw\r\n" +
            $"[3] Transfer\r\n" +
            $"[4] My Statement\r\n" +
            $"[5] Logout\r\n" +
            $"[6] Exit\r\n" +
            $"\r\nEnter an option: ");
		while (choice > 6 || choice < 1)
        {
			var input = Console.ReadLine();
			try
			{
				choice = Convert.ToInt32(input);
				// bit clunky, but a temp? workaround to throw the error msg anyway
				if (choice > 6)
                {
					throw new FormatException();
                }
			}
			catch (FormatException)
			{
				Console.WriteLine($"{input} is not a menu option.");
			}
			
		}
				
		GoToMenu(choice);
	}
	private void GoToMenu(int choice)
    {
		if (choice == 6)
        {
			Console.WriteLine("Exiting the application. Thanks for playing!");
			Environment.Exit(1);
        }
    }
}
