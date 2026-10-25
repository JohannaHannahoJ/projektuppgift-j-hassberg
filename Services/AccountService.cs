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

    // Hämtar alla konton från databasen och returnerar dem som en lista.
    public async Task<List<Account>> GetAllAccountsAsync()
    {
        return await _db.Accounts.ToListAsync();
    }
}   