using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Server.API.Models;

public class Game
{
    public int GameId { get; set; }
    public string Name { get; set;}
    public virtual List<User> UserGames { get; set; } = new List<User>();
}

