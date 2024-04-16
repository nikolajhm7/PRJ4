using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Server.API.Hubs;

namespace Server.API.Models;

public class User : IdentityUser
{
    public int coins { get; set; }

    // List of users who initiated the "friendship"
    public virtual List<Friendship> Inviters { get; set; }
    // List of users who where invited to the "friendship"
    public virtual List<Friendship> Invitees { get; set; }

}