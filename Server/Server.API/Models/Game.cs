using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Server.API.Models
{
    public class Game
    {
        public int GameId { get; set; }
        public string Name { get; set;}
    }
}
