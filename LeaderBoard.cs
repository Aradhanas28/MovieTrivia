namespace MovieTrivia.Models;

public class Leaderboard
{
    public int Id { get; set; }
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public DateTime DatePosted { get; set; } = DateTime.Now; // default value
}