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
		Console.WriteLine($"[1] Deposit\r\n" +
            $"[2] Withdraw\r\n" +
            $"[3] Transfer\r\n" +
            $"[4] My Statement\r\n" +
            $"[5] Logout\r\n" +
            $"[6] Exit\r\n" +
            $"\r\nEnter an option: ");
		var choice = Console.ReadLine();
	}
}
