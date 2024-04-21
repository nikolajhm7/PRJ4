namespace Server.API.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public bool IsActive => !IsExpired;
    public bool IsExpired => DateTime.UtcNow >= Expires;
}