using System;
using SpringHeroBank.controller;
using SpringHeroBank.entity;

namespace SpringHeroBank.view
{
    public class MainView
    {
        public static Account loggedInAccount;


        public static void GenerateMenu()
        {
            AccountController controller = new AccountController();
            while (true)
            {
                Console.WriteLine("---------WELCOME TO SPRING HERO BANK---------");
                Console.WriteLine("1. Register for free.");
                Console.WriteLine("2. Login.");
                Console.WriteLine("3. Exit.");
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("Please enter your choice (1|2|3): ");
                var choice = GetNumber();
                switch (choice)
                {
                    case 1:
                        controller.Register();
                        break;
                    case 2:
                        controller.DoLogin();
                        break;
                    case 3:
                        Console.WriteLine("See you later.");
                        Environment.Exit(1);
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static int GetNumber()
        {
            var choice = 0;
            while (true)
            {
                try
                {
                    var strChoice = Console.ReadLine();
                    choice = Int32.Parse(strChoice);
                    break;
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please enter a number.");
                }
            }
            return choice;
        }
    }
}