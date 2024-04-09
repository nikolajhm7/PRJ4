using Microsoft.EntityFrameworkCore;
using Server.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Server.API.Data.Models;
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
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(ConnectionString);
    
    public async Task DeleteOldLogEntriesAsync()
    {
        var sql = "DELETE FROM LogEvents WHERE TimeStamp < DATEADD(day, -30, GETUTCDATE())";
        await Database.ExecuteSqlRawAsync(sql);
    }

}