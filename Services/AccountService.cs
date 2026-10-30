using Microsoft.EntityFrameworkCore;
using Digibok.Models;
using Digibok.Data;

namespace Digibok.Services;

/*
* Ansvarar för att hämta och visa konton.
*/

public class AccountService
{
    // Skapa en privat instans av AppDbContext för att interagera med databasen
    private readonly AppDbContext _db;

    // Spara objektet _db som tar parametern db så att andra metoder kan använda det.
    public AccountService(AppDbContext db)
    {
        _db = db;
    }

    // Hämtar alla konton från databasen.
    public async Task<List<Account>> GetAllAccountsAsync()
    {
        return await _db.Accounts.ToListAsync();
    }

    // Hämtar aktiva konton från databasen
    public async Task<List<Account>> GetActiveAccountsAsync()
    {
        return await _db.Accounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync();
    }

    // Söker efter ett konto med ett visst kontonummer
    public async Task<Account?> GetAccountByNumberAsync(int accountNumber)
    {
        return await _db.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    // Hämtar alla kontoklasser från databasen.
    public async Task<List<AccountClass>> GetAllAccountClassesAsync()
    {
        return await _db.AccountClasses
            .OrderBy(c => c.Code)
            .ToListAsync();
    }


    // Skapar och sparar ett eget konto i databasen.
    public async Task<Account> CreateAccountAsync(
        int accountNumber,
        string name,
        int accountClassId)
    {
        var account = new Account
        {
            AccountNumber = accountNumber,
            Name = name,
            AccountClassId = accountClassId,
            IsCustom = true,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        return account;
    }
}