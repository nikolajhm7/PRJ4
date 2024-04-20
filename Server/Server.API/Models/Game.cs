using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.API.Models;

public class Game
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GameId { get; set; }
    public string Name { get; set;}
    public virtual List<UserGame> UserGames { get; set; } = new List<UserGame>();
}

