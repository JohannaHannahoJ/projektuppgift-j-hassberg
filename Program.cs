using Digibok.Services;
using Digibok.Data;

Console.WriteLine("Välkommen till Digibok - din digitala bokföring!");

// Importera kontoplanen om den inte redan finns i databasen
var importer = new AccountImportService();
await importer.ImportIfNeededAsync();


// Hämta och visa alla konton från databasen
using var db = new AppDbContext();
var accountService = new AccountService(db);
var accounts = await accountService.GetAllAccountsAsync();
foreach (var account in accounts)
{
    Console.WriteLine($"Konto: {account.AccountNumber} - {account.Name}");
}
