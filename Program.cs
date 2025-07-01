using System.Collections;
using chess_calculator;

bool isExit = false;
Console.WriteLine("Welcome to Chess Tracker v0.5");
while (isExit == false)
{
    Console.WriteLine("\nPlease select an option:\n1 - Add a match\n2 - Add a new player\n3 - View player stats -- Coming Soon!\n4 - Update player information -- Coming Soon!\n9 - Exit");
    var userInput = Console.ReadLine()?.Trim() ?? "";
    switch (userInput)
    {
        case "1":
            Commands.AddNewGame();
            break;
        case "2":
            Console.WriteLine("\nAdding a new player:");
            Commands.AddPlayer(out _);
            break;
        case "3":
            Console.WriteLine("Coming Soon");
            break;
        case "4":
            Console.WriteLine("Coming Soon");
            break;
        case "9":
            Console.WriteLine("Are you sure you want to quit? Y/N");
            var confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
            if (confirm == "y")
            {
                Console.WriteLine("Have a great day!");
                isExit = true;
            }
            break;
        default:
            Console.WriteLine("I didn't quite catch that");
            break;
    }
}
