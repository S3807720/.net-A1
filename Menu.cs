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
		Console.WriteLine($"[1] Deposit
            [2] Withdraw
            [3] Transfer
            [4] My Statement
            [5] Logout
            [6] Exit
            %nEnter an option: ");
		var choice = Console.ReadLine();
	}
}
