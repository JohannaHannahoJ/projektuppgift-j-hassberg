using Microsoft.EntityFrameworkCore;
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

// Hämta ett konto som finns i databasen
var account = await db.Accounts
    .OrderBy(a => a.AccountNumber)
    .FirstAsync();

// Lägg till en debetrad på testverifikatet
var row = await voucherService.AddVoucherRowAsync(
    voucher.Id,
    account.Id,
    100m,
    0m);

Console.WriteLine(
    $"Bokföringsrad skapad: konto {account.AccountNumber}, " +
    $"debet {row.Debit}, kredit {row.Credit}");

// Hämta ett andra konto
var secondAccount = await db.Accounts
    .OrderBy(a => a.AccountNumber)
    .Skip(1)
    .FirstAsync();

// Lägg till en kreditrad på samma verifikat
var creditRow = await voucherService.AddVoucherRowAsync(
    voucher.Id,
    secondAccount.Id,
    0m,
    100m);

Console.WriteLine(
    $"Kreditrad skapad: konto {secondAccount.AccountNumber}, " +
    $"debet {creditRow.Debit}, kredit {creditRow.Credit}");
    
// Kontrollera om verifikatet balanserar.
var isBalanced = await voucherService.IsVoucherBalancedAsync(voucher.Id);

if (isBalanced)
{
    Console.WriteLine("Verifikatet balanserar");
}
else
{
    Console.WriteLine("Verifikatet balanserar inte");
}

// Hämta och visa alla konton från databasen
/*var accountService = new AccountService(db);
var accounts = await accountService.GetAllAccountsAsync();
foreach (var account in accounts)
{
    Console.WriteLine($"Konto: {account.AccountNumber} - {account.Name}");
}
*/