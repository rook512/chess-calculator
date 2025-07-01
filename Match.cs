public class Match
{
    public int MatchId { get; set; } = -1;
    public DateOnly DateOfMatch { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string White { get; set; } = "";
    public string Black { get; set; } = "";
    public string Result { get; set; } = "Draw";

    public Match() { }
}