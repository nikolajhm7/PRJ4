namespace Server.API.Models;

public class UserGame
{
    public int UserGameId { get; set; }
    public string UserId { get; set; }
    public int GameId { get; set; }
    public virtual User User { get; set; }
    public virtual Game Game { get; set; }
}