using CsvHelper.Configuration;

namespace chess_calculator
{
    public class Player
    {
        public int Id { get; set; } = -1;
        public string Name { get; set; } = "";
        public DateOnly DateOfBirth { get; set; } = default;
        public int Rating { get; set; } = 1000;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public int GamesPlayed { get; set; } = 0;
        public bool InternationalMaster { get; set; } = false;

        public Player() { }
        public Player(string name, DateOnly dateOfBirth = default, int rating = 1000)
        {
            Name = name;
            DateOfBirth = dateOfBirth;
            Rating = rating;
        }
    }
    public sealed class PlayerMap : ClassMap<Player>
    {
        public PlayerMap()
        {
            Map(m => m.Id).Name("Id");
            Map(m => m.Name).Name("Name");
            Map(m => m.DateOfBirth).Name("DateOfBirth");
            Map(m => m.Rating).Name("Rating");
            Map(m => m.Wins).Name("Wins");
            Map(m => m.Losses).Name("Losses");
            Map(m => m.Draws).Name("Draws");
            Map(m => m.GamesPlayed).Name("GamesPlayed");
            Map(m => m.InternationalMaster).Name("InternationalMaster");
        }
    }
}
