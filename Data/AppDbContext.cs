using Digibok.Models;
using Microsoft.EntityFrameworkCore;

namespace Digibok.Data;
/*
 * Klass för att hantera databasen med Entity Framework Core.
 * Beskriver hur modellerna mappas till databastabeller.
*/
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountClass> AccountClasses => Set<AccountClass>();
    // Använder SQLite som databas. Om ingen konfiguration har angetts, 
    // används en lokal fil med namnet "digibok.db".
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=digibok.db");
        }
    }
}
