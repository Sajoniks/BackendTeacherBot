using Microsoft.EntityFrameworkCore;

namespace LearnBotServer.Model;

public class UserContext : DbContext
{
    public UserContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=botdb.db");
    }

    public DbSet<User> Users { get; set; }
}