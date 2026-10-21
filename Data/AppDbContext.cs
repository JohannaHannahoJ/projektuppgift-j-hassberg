using Digibok.Models;
using Microsoft.EntityFrameworkCore;

namespace Digibok.Data;
/*
 * klass för att hantera databasen med Entity Framework Core.
 * Beskriver hur modellerna mappas till databastabeller.
*/
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountClass> AccountClasses => Set<AccountClass>();
}
