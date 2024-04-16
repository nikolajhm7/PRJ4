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

    public DbSet<User> User => Set<User>();
    public DbSet<Game> Games { get; set; }
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
    }
    public async Task DeleteOldLogEntriesAsync()
    {
        var sql = "DELETE FROM LogEvents WHERE TimeStamp < DATEADD(day, -30, GETUTCDATE())";
        await Database.ExecuteSqlRawAsync(sql);
    }

}