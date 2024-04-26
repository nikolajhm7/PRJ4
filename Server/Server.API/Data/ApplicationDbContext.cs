using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace Server.API.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    private const string DbName = "PartyPlayPalaceDB";
    private const string ConnectionString = $"Data Source=localhost;Initial Catalog={DbName};User ID=SA;Password=abc123AB;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

    public DbSet<User> Users { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<UserGame> UserGames { get; set; }
    public DbSet<Friendship> Friendships { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseSqlServer(ConnectionString);
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Friendship>()
            .HasKey(fs => new { fs.User1Id, fs.User2Id });

        modelBuilder.Entity<Friendship>()
            .HasOne(fs => fs.User1)
            .WithMany(u => u.Inviters)
            .HasForeignKey(fs => fs.User1Id)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Friendship>()
            .HasOne(fs => fs.User2)
            .WithMany(u => u.Invitees)
            .HasForeignKey(fs => fs.User2Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure primary key for IdentityUserLogin<string>
        modelBuilder.Entity<IdentityUserLogin<string>>()
            .HasKey(l => new { l.LoginProvider, l.ProviderKey });

        // Configure primary key for IdentityUserLogin<string>
        modelBuilder.Entity<IdentityUserRole<string>>()
            .HasKey(l => new { l.UserId, l.RoleId });

        // Configure primary key for IdentityUserLogin<string>
        modelBuilder.Entity<IdentityUserToken<string>>()
            .HasKey(l => new { l.UserId, l.LoginProvider, l.Name });


        //Seeding data for user and game
        //creating users
        var appUser1 = new User
        {
            Id = "59fbd0c8-0e0b-4cba-980e-f196b905a249",
            Email = "frank@gmail.com",
            NormalizedEmail = "FRANK@GMAIL.COM",
            EmailConfirmed = true,
            UserName = "Frank",
            NormalizedUserName = "FRANK"
        };
        var appUser2 = new User
        {
            Id = "3de1a4b2-2b03-4b9d-b04d-d02cbef1f447",
            Email = "Peter@gmail.com",
            NormalizedEmail = "PETER@GMAIL.COM",
            EmailConfirmed = true,
            UserName = "Peter",
            NormalizedUserName = "PETER"
        };
        var appUser3 = new User
        {
            Id = "1c7e97d3-a982-4a1b-8d8e-b6b9d7e32c0f",
            Email = "Hans@gmail.com",
            NormalizedEmail = "HANS@GMAIL.COM",
            EmailConfirmed = true,
            UserName = "Hans",
            NormalizedUserName = "HANS"
        };

        //set user passwords
        PasswordHasher<User> ph1 = new PasswordHasher<User>();
        appUser1.PasswordHash = ph1.HashPassword(appUser1, "FrankFrank1!");

        PasswordHasher<User> ph2 = new PasswordHasher<User>();
        appUser2.PasswordHash = ph2.HashPassword(appUser2, "PeterPeter1!");

        PasswordHasher<User> ph3 = new PasswordHasher<User>();
        appUser3.PasswordHash = ph3.HashPassword(appUser3, "HansHans1!");

        //seed users
        modelBuilder.Entity<User>().HasData(appUser1);
        modelBuilder.Entity<User>().HasData(appUser2);
        modelBuilder.Entity<User>().HasData(appUser3);


        // seeding all the games
        // creating games
        var game1 = new Game()
        {
            GameId = 1,
            MaxPlayers = 10,
            Name = "Hangman"
        };
        var game2 = new Game()
        {
            GameId = 2,
            MaxPlayers = 2,
            Name = "TicTacToe"
        };
        var game3 = new Game()
        {
            GameId = 3,
            MaxPlayers = 2,
            Name = "Rock, Paper, Scissors"
        };

        modelBuilder.Entity<Game>().HasData(game1);
        modelBuilder.Entity<Game>().HasData(game2);
        modelBuilder.Entity<Game>().HasData(game3);
        
        // Define friendship data with user ids

        modelBuilder.Entity<Friendship>().HasData(
            new Friendship { User1Id = "59fbd0c8-0e0b-4cba-980e-f196b905a249", User2Id = "3de1a4b2-2b03-4b9d-b04d-d02cbef1f447", Status = "Accepted", date = DateTime.UtcNow }
            );

        // seeding games
        modelBuilder.Entity<UserGame>().HasData(
            // Define user game data with user and game ids
            new UserGame { UserGameId = 1, UserId = "59fbd0c8-0e0b-4cba-980e-f196b905a249", GameId = 1 },
            new UserGame { UserGameId = 2, UserId = "59fbd0c8-0e0b-4cba-980e-f196b905a249", GameId = 2 },
            new UserGame { UserGameId = 3, UserId = "59fbd0c8-0e0b-4cba-980e-f196b905a249", GameId = 3 },
            new UserGame { UserGameId = 4, UserId = "3de1a4b2-2b03-4b9d-b04d-d02cbef1f447", GameId = 1 },
            new UserGame { UserGameId = 5, UserId = "1c7e97d3-a982-4a1b-8d8e-b6b9d7e32c0f", GameId = 1 }
        );
    }
    public async Task DeleteOldLogEntriesAsync()
    {
        var sql = "DELETE FROM LogEvents WHERE TimeStamp < DATEADD(day, -30, GETUTCDATE())";
        await Database.ExecuteSqlRawAsync(sql);
    }

}