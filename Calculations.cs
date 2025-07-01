using CsvHelper.Configuration.Attributes;

namespace chess_calculator
{
    class Calculations
    {
        static float CalculateExpectedScore(int ratingA, int ratingB)
        {
            int ratingDifferential = ratingB - ratingA;
            int ratingAdvantage = 480; // Using modification sugested by Jeff Sonas in 2011; Original value is 400
            float denominator = 1 + MathF.Pow(10, ratingDifferential / ratingAdvantage);
            float expectedScore = 1 / denominator;
            return expectedScore;
        }
        static int GetKFactor(Player player, DateOnly dateOfMatch)
        {
            if (player.InternationalMaster || player.Rating >= 2400 && player.GamesPlayed >= 30)
            {
                return 10;
            }
            else if (!IsEighteenOrOlder(player, dateOfMatch) && player.Rating < 2300)
            {
                return 40;
            }
            else if (player.GamesPlayed < 30)
            {
                return 40;
            }
            else return 20;
        }
        static bool IsEighteenOrOlder(Player player, DateOnly dateOfMatch)
        {
            DateOnly matchDate = dateOfMatch;
            DateOnly eighteenYearsAgo = matchDate.Year > 18 ? matchDate.AddYears(-18) : matchDate;
            return player.DateOfBirth >= eighteenYearsAgo;
        }
        public static int CalculatePlayerRatingChange(Player player, Player opponent, int games, float score, DateOnly dateOfMatch)
        {
            int kFactor = GetKFactor(player, dateOfMatch);
            float expectedScore = CalculateExpectedScore(player.Rating, opponent.Rating); // multiply Expected Score by the number of games played
            float actualScore = score / games;
            float scoreChange = kFactor * (actualScore - expectedScore); // K * (Actual Score - Expected Score)
            int newScore = (int)Math.Round(scoreChange) * games;
            int newRating = player.Rating + newScore;
            if (scoreChange > 0)
            {
                Console.WriteLine($"{player.Name}'s rating of {player.Rating} increased by {newScore} and is now {newRating}.");
            }
            else if (scoreChange < 0)
            {
                Console.WriteLine($"{player.Name}'s rating of {player.Rating} decreased by {Math.Abs(newScore)} and is now {newRating}.");
            }
            else
            {
                Console.WriteLine($"{player.Name}'s rating of {player.Rating} did not change.");
            }
            return newRating;
        }
    }
}