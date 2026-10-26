using Digibok.Services;
using Digibok.Data;

Console.WriteLine("Välkommen till Digibok - din digitala bokföring!");

// Importera kontoplanen om den inte redan finns i databasen
var importer = new AccountImportService();
await importer.ImportIfNeededAsync();


// Skapa en koppling till databasen
using var db = new AppDbContext();

var voucherService = new VoucherService(db);

// Skapa ett testverifikat
var voucher = await voucherService.CreateVoucherAsync(
    new DateOnly(2026, 10, 26),
    "Test av verifikat",
    'A');

Console.WriteLine(
    $"Skapade verifikat: {voucher.VoucherSeries}{voucher.VoucherNumber}, " +
    $"datum: {voucher.Date}, beskrivning: {voucher.Description}");

// Hämta och visa alla konton från databasen
/*var accountService = new AccountService(db);
var accounts = await accountService.GetAllAccountsAsync();
foreach (var account in accounts)
{
    Console.WriteLine($"Konto: {account.AccountNumber} - {account.Name}");
}
*/