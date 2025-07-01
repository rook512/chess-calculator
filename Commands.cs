using System.Security.Cryptography.X509Certificates;

namespace chess_calculator
{
    public class Commands
    {
        static public bool AddPlayer( out Player player, string? name = null)
        {
            if (name is null)
            {
                if (!AddPlayerName(out string resolvedName))
                {
                    player = default!;
                    return false;
                }
                name = resolvedName;
            }
            DateOnly dateOfBirth = AddDate();
            int rating = AddPlayerRating();
            if (!AddPlayerConfirm(name, dateOfBirth, rating))
            {
                bool continueLoop = true;
                while (continueLoop)
                {
                    Console.WriteLine($"\nWhat needs to be changed?\n1 - Name ({name})\n2 - Date of Birth ({dateOfBirth})\n3 - Rating ({rating})\n0 - I changed my mind; I don't want to add a player.\nProceed - Add a new player with the current information");
                    var entry = Console.ReadLine()?.Trim().ToLower() ?? "";
                    switch (entry)
                    {
                        case "1":
                            if (!AddPlayerName(out string newName))
                            {
                                player = default!;
                                return false;
                            }
                            name = newName;
                            break;
                        case "2":
                            dateOfBirth = AddDate();
                            break;
                        case "3":
                            rating = AddPlayerRating();
                            break;
                        case "0":
                            Console.WriteLine("\nAre you sure you want to abandon adding this player? Y/N");
                            if (Confirm())
                            {
                                player = default!;
                                return false;
                            }
                            break;
                        case "proceed":
                            continueLoop = false;
                            break;
                        default:
                            Console.WriteLine("I didn't quite catch that.");
                            break;
                    }
                }
            }
            DataAccessLayer.AddNewPlayer(name, dateOfBirth, out player, rating);
            return true;
        }
        static public bool AddPlayerName(out string name)
        {
            Console.WriteLine("Please enter the name of the player you would like to add:");
            var inputName = Console.ReadLine() ?? "";
            if (inputName == "")
            {
                Console.WriteLine("Unable to parse name\n");
                return AddPlayerName(out name);
            }
            else
            {
                Console.WriteLine($"Is {inputName} correct? Y/N");
                if (!Confirm())
                {
                    return AddPlayerName(out name);
                }
                var players = DataAccessLayer.GetPlayers();
                if (players is not null)
                {
                    var matches = players.Where(player => player.Name == inputName);
                    if (matches.Any())
                    {
                        Console.WriteLine($"\nFound {matches.Count()} existing player(s) named {inputName}. Did you want to add a new player? Y/N");
                        if (!Confirm())
                        {
                            name = "";
                            return false;
                        }
                    }
                }
                name = inputName;
                return true;
            }
        }
        static public DateOnly AddDate(string? text = "\nPlease enter the date of birth as 'MM/DD/YYYY' or press enter to skip.\nNOTE: not including a date will not properly account for players under the age of 18.")
        {

            Console.WriteLine(text);
            var inputDate = Console.ReadLine()?.Trim() ?? "";
            if (inputDate == "")
            {
                return default;
            }
            else
            {
                string[] parsedDate = inputDate.Split('/');
                if (parsedDate.Length != 3 || parsedDate[0].Length > 2 || parsedDate[1].Length > 2 || parsedDate[2].Length != 4)
                {
                    Console.WriteLine("Invalid format; please enter dates as 'MM/DD/YYYY'");
                    return AddDate();
                }
                else if (int.TryParse(parsedDate[0], out int month) && int.TryParse(parsedDate[1], out int day) && int.TryParse(parsedDate[2], out int year))
                {
                    if (month >= 1 && month <= 12 && day >= 1 && day <= 31 && year >= 1)
                    {
                        return new DateOnly(year, month, day);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid Date; Month {month}, Day {day}, Year {year}");
                        return AddDate();
                    }
                }
                else
                {
                    Console.WriteLine("Invalid format; please enter dates as 'MM/DD/YYYY'");
                    return AddDate();
                }

            }
        }
        static public int AddPlayerRating()
        {
            Console.WriteLine("\nPlease enter the starting rating for the player if other than the default 1000:");
            var entry = Console.ReadLine()?.Trim() ?? "";
            if (entry == "") return 1000;
            else if (int.TryParse(entry, out int rating)) return rating;
            else
            {
                Console.WriteLine("Invalid input");
                return AddPlayerRating();
            }
        }
        static public bool AddPlayerConfirm(string name, DateOnly dateOfBirth, int rating)
        {
            Console.WriteLine($"\nBefore proceeding, please verify these details:\nName: {name}\nDate Of Birth: {dateOfBirth}\nRating: {rating}");
            Console.WriteLine("\nAre these details correct? Y/N");
            return Confirm();
        }
        static public DateOnly ConvertStringToDate(string dateString)
        {
            try
            {
                string[] splitString = dateString.Split("/");
                int.TryParse(splitString[0], out int month);
                int.TryParse(splitString[1], out int day);
                int.TryParse(splitString[2], out int year);
                return new DateOnly(year, month, day);
            }
            catch
            {
                throw new Exception($"Unable to parse {dateString}");
            }

        }
        static public bool Confirm()
        {
            var input = Console.ReadLine()?.Trim().ToLower() ?? "";
            if (input == "y") return true;
            else return false;
        }
        public static void AddNewGame()
        {
            Console.WriteLine("\nGot it, adding a new match.");
            DateOnly matchDate = AddDate("\nPlease enter the date of the match in MM/DD/YYYY format or press Enter to skip.\nNOTE: Matches without a date will use today's date and may not appropriately account for players under 18.");
            if (!PlayerLookup("\nPlease enter the name of the White player:", out Player white)) return;
            if (!PlayerLookup("\nPlease enter the name of the Black player:", out Player black, white.Id)) return;
            List<float> matches = MatchEntry("\nPlease enter the match result from White's perspective. e.g. 1 for White win, 0 for White loss, and .5 for a draw.\nMultiple matches between the same player can be entered by separating them with a comma. e.g. 1, 0, .5, 1");
            MatchConfirm(matchDate, white, black, matches);
        }
        public static bool PlayerLookup(string text, out Player player, int? id = null)
        {
            Console.WriteLine(text);
            var entry = Console.ReadLine() ?? "";
            var records = DataAccessLayer.GetPlayers();
            var results = records.Where(player => player.Name == entry).ToList();
            if (results.Count == 0)
            {
                Console.WriteLine($"\nNo player found with the name {entry}.");
                return RetrySearch(entry, text, out player);
            }
            else if (results.Count > 1)
            {
                return PlayerDisambiguation(results, entry, out player, id);
            }
            else if (results[0].Id == id)
            {
                Console.WriteLine($"{entry} is already included in this match as the White player.");
                return RetrySearch(entry, text, out player, id);
            }
            else
            {
                player = results[0];
                return true;
            }
        }
        public static bool RetrySearch(string name, string text, out Player player, int? id = null)
        {
            Console.WriteLine($"Please ensure that the name is properly spelled and includes first and last name if entered when adding the player.");
            Console.WriteLine($"\nWould you like to search again or add {name} as a new player?\n1 - Search for a different name.\n2 - Add {name} as a new player.\n0 - Abandon adding this match.");
            var entry = Console.ReadLine()?.Trim() ?? "";
            switch (entry)
            {
                case "1":
                    return PlayerLookup(text, out player, id);
                case "2":
                    return AddPlayer(out player, name);
                case "0":
                    Console.WriteLine("Abanadoned adding match.");
                    player = default!;
                    return false;
                default:
                    Console.WriteLine("I didn't quite catch that.");
                    return RetrySearch(name, text, out player);
            }
        }
        public static bool PlayerDisambiguation(List<Player> players, string name, out Player player, int? id = null)
        {
            Console.WriteLine($"\nI found {players.Count} players with the name {name}. Let's try to narrow it down");
            var count = 1;
            foreach (Player p in players)
            {
                Console.WriteLine($"{count} - Name: {p.Name}, ID: {p.Id}, Date of Birth: {p.DateOfBirth}, Games Played: {p.GamesPlayed}");
                count++;
            }
            Console.WriteLine($"Add - I don't see the player in the list. I would like to add a new player named {name}");
            Console.WriteLine("0 - I changed my mind; I don't want to add this match");
            var entry = Console.ReadLine()?.Trim().ToLower() ?? "";
            if (int.TryParse(entry, out int number) && number - 1 >= 0 && number <= players.Count)
            {
                player = players[number - 1];
                if (id != null && player.Id == id)
                {
                    Console.WriteLine($"\n{player.Name} is already included in this match as the White player.");
                    return PlayerDisambiguation(players, name, out player, id);
                }
                Console.WriteLine($"\nName: {player.Name}, ID: {player.Id}, Date of Birth: {player.DateOfBirth}, GamePlayed: {player.GamesPlayed}");
                Console.WriteLine("Use this player? Y/N");
                if (Confirm()) return true;
                else { return PlayerDisambiguation(players, name, out player, id); }
            }
            else if (entry == "0")
            {
                Console.WriteLine("Are you sure you want to abandon the match? Y/N");
                if (Confirm())
                {
                    player = default!;
                    return false;
                }
                else return PlayerDisambiguation(players, name, out player, id);
            }
            else if (entry == "add")
            {
                AddPlayer(out player, name);
                return true;
            }
            else
            {
                Console.WriteLine("I didn't quite catch that.");
                return PlayerDisambiguation(players, name, out player, id);
            }
        }
        public static List<float> MatchEntry(string text)
        {
            Console.WriteLine(text);
            var entry = Console.ReadLine() ?? "";
            var matchArray = entry.Split(",");
            var formatted = matchArray.Select(n => n.Trim());
            List<float> result = [];
            foreach (string match in formatted)
            {
                if (!float.TryParse(match, out float parsed))
                {
                    Console.WriteLine($"Could not parse '{match}' in {entry}. Please format multiple games as a number separated by a comma. e.g. '0,.5,1,1'");
                    return MatchEntry(text);
                }
                else if (parsed > 1.0f)
                {
                    Console.WriteLine($"{parsed} is not a valid match result. Please use '1' for a win, '.5' for a draw, and '0' for a loss");
                    return MatchEntry(text);
                }
                else
                {
                    result.Add(parsed);
                }
            }
            return result;
        }
        public static void MatchConfirm(DateOnly matchDate, Player white, Player black, List<float> result)
        {
            Console.WriteLine("\nThe information provided for this match is currently as follows:");
            Console.WriteLine($"Match Date: {matchDate}");
            Console.WriteLine($"White Player: {white.Name}");
            Console.WriteLine($"Black Player: {black.Name}");
            Console.WriteLine($"Match result for white: {string.Join(", ", result)}");
            Console.WriteLine("\nDoes this look correct? Y/N");
            if (Confirm())
            {
                UpdateStatsForMatch(matchDate, white, black, result);
            }
            else UpdateMatch(matchDate, white, black, result);
        }
        public static void UpdateStatsForMatch(DateOnly matchDate, Player white, Player black, List<float> matches)
        {
            List<Match> preparedMatches = PreparedMatches(matchDate, white, black, matches);

            DataAccessLayer.AddNewMatch(preparedMatches);
            DataAccessLayer.UpdatePlayerStats(white, black, matches, matchDate);
        }
        public static List<Match> PreparedMatches(DateOnly matchDate, Player white, Player black, List<float> matches)
        {
            List<Match> result = [];
            foreach (float match in matches)
            {
                result.Add(new Match()
                {
                    DateOfMatch = matchDate,
                    White = white.Name,
                    Black = black.Name,
                    Result = match == 1.0f ? "White" : match == 0f ? "Black" : "Draw"
                });
            }
            return result;
        }
        public static void UpdateMatch(DateOnly matchDate, Player white, Player black, List<float> result)
        {
            Console.WriteLine($"What would you like to change?\n");
            Console.WriteLine($"1 - Match Date ({matchDate})");
            Console.WriteLine($"2 - White Player ({white.Name})");
            Console.WriteLine($"3 - Black Player ({black.Name})");
            Console.WriteLine($"4 - Match results for white ({string.Join(", ", result)})");
            Console.WriteLine("0 - I changed my mind; I don't want to add this match");
            Console.WriteLine("Proceed - This match looks good, I'm ready to add it.");
            var entry = Console.ReadLine()?.Trim().ToLower() ?? "";
            switch (entry)
            {
                case "1":
                    DateOnly newDate = AddDate("\nPlease enter the date of the match in MM/DD/YYYY format or press Enter to skip.\nNOTE: Matches without a date will not appropriately account for players under the age of 18.");
                    UpdateMatch(newDate, white, black, result);
                    break;
                case "2":
                    if (!PlayerLookup("\nPlease enter the name of the White player:", out Player newWhite)) return;
                    UpdateMatch(matchDate, newWhite, black, result);
                    break;
                case "3":
                    if (!PlayerLookup("\nPlease enter the name of the Black player:", out Player newBlack)) return;
                    UpdateMatch(matchDate, white, newBlack, result);
                    break;
                case "4":
                    List<float> newResult = MatchEntry("\nPlease enter the match result from White's perspective. e.g. 1 for White win, 0 for White loss, and .5 for a draw.\nMultiple matches between the same player can be entered by separating them with a comma. e.g. 1, 0, .5, 1");
                    UpdateMatch(matchDate, white, black, newResult);
                    break;
                case "0":
                    Console.WriteLine("\nAre you sure you want to abandon adding this match? Y/N");
                    if (Confirm()) return;
                    else UpdateMatch(matchDate, white, black, result);
                    break;
                case "proceed":
                    UpdateStatsForMatch(matchDate, white, black, result);
                    break;
                default:
                    Console.WriteLine("\nI didn't quite catch that.\n");
                    UpdateMatch(matchDate, white, black, result);
                    break;
            }
        }
    }
}