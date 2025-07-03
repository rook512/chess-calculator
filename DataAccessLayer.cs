using System.Globalization;
using System.Runtime;
using CsvHelper;
using CsvHelper.Configuration;

namespace chess_calculator
{
    public class DataAccessLayer
    {
        static public List<Player> GetPlayers()
        {
            CreateFilesIfNeeded("players.csv");
            using var reader = new StreamReader("data\\players.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<PlayerMap>();
            var records = csv.GetRecords<Player>().ToList();
            return records;
        }
        static public void UpdatePlayers(IEnumerable<Player> players)
        {
            using var writer = new StreamWriter("data\\players.csv", false);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(players);
        }

        static public void AddNewPlayer(string name, DateOnly dateOfBirth, out Player player, int rating = 1000)
        {

            var records = GetPlayers();
            if (records.Count == 0)
            {
                player = new(name, dateOfBirth, rating)
                {
                    Id = 1
                };
                if (player.Rating >= 2400) player.InternationalMaster = true;
                IEnumerable<Player> playerUpdate = [player];
                UpdatePlayers(playerUpdate);
            }
            else
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                };
                IEnumerable<int> Ids = records.Select(player => player.Id);
                int newId = Ids.Max() + 1;
                player = new(name, dateOfBirth, rating) { Id = newId };
                if (player.Rating >= 2400) player.InternationalMaster = true;
                using var stream = File.Open("data\\players.csv", FileMode.Append);
                using var writer = new StreamWriter(stream);
                using var csv = new CsvWriter(writer, config);
                csv.WriteRecord(player);
                csv.NextRecord();
            }
        }
        static public List<Match> GetMatches()
        {
            CreateFilesIfNeeded("matches.csv");
            using var reader = new StreamReader("data\\matches.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            //csv.Context.RegisterClassMap<PlayerMap>();
            var records = csv.GetRecords<Match>().ToList();
            return records;
        }
        static public void AddNewMatch(List<Match> matches)
        {
            foreach (Match match in matches)
            {
                var records = GetMatches();
                if (records.Count == 0)
                {
                    match.MatchId = 1;
                    IEnumerable<Match> matchUpdate = [match];
                    UpdateMatches(matchUpdate);
                }
                else
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = false,
                    };
                    IEnumerable<int> Ids = records.Select(match => match.MatchId);
                    match.MatchId = Ids.Max() + 1;

                    using var stream = File.Open("data\\matches.csv", FileMode.Append);
                    using var writer = new StreamWriter(stream);
                    using var csv = new CsvWriter(writer, config);
                    csv.WriteRecord(match);
                    csv.NextRecord();
                }
            }
        }
        static public void UpdateMatches(IEnumerable<Match> matches)
        {
            using var writer = new StreamWriter("data\\matches.csv", false);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(matches);
        }
        static public void UpdatePlayerStats(Player white, Player black, List<float> matches, DateOnly dateOfMatch)
        {
            float whiteScore = matches.Sum();
            float blackScore = matches.Count - whiteScore;
            int whiteRating = Calculations.CalculatePlayerRatingChange(white, black, matches.Count, whiteScore, dateOfMatch);
            int blackRating = Calculations.CalculatePlayerRatingChange(black, white, matches.Count, blackScore, dateOfMatch);
            List<Player> players = GetPlayers();

            Player? whiteTarget = players.FirstOrDefault(p => p.Id == white.Id);
            if (whiteTarget != null)
            {
                whiteTarget.GamesPlayed += matches.Count;
                foreach (float game in matches)
                {
                    if (game == 1f) whiteTarget.Wins++;
                    else if (game == 0f) whiteTarget.Losses++;
                    else whiteTarget.Draws++;
                }
                whiteTarget.Rating = whiteRating;
            }
            else throw new($"Could not find player with ID {white.Id}");
            Player? blackTarget = players.FirstOrDefault(p => p.Id == black.Id);
            if (blackTarget != null)
            {
                blackTarget.GamesPlayed += matches.Count;
                foreach (float game in matches)
                {
                    if (game == 0f) blackTarget.Wins++;
                    else if (game == 1f) blackTarget.Losses++;
                    else blackTarget.Draws++;
                }
                blackTarget.Rating = blackRating;
            }
            else throw new($"Could not find player with ID {black.Id}");
            if (whiteTarget.Rating >= 2400) whiteTarget.InternationalMaster = true;
            if (blackTarget.Rating >= 2400) blackTarget.InternationalMaster = true;
            UpdatePlayers(players);
        }
        static public void CreateFilesIfNeeded(string fileName)
        {
            string folderPath = "data";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);
            if (!File.Exists(filePath)) using (File.Create(filePath)) { }
            ;
        }
    }
}