using Digibok.Services;
using Digibok.Data;

Console.WriteLine("Välkommen till Digibok - din digitala bokföring!");

// Importera kontoplanen om den inte redan finns i databasen
var importer = new AccountImportService();
await importer.ImportIfNeededAsync();

// Skapa en koppling till databasen
using var db = new AppDbContext();

// Skapa tjänsten som hanterar verifikat
var voucherService = new VoucherService(db);

// Visa menyn tills användaren väljer att avsluta.
bool running = true;

while (running)
{
    Console.WriteLine();
    Console.WriteLine("===== DIGIBOK =====");
    Console.WriteLine("1. Skapa verifikat");
    Console.WriteLine("2. Visa verifikat");
    Console.WriteLine("3. Visa kontoplan");
    Console.WriteLine("4. Avsluta");
    Console.Write("Välj ett alternativ: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            Console.WriteLine("Skapa verifikat");
            break;

        case "2":
            Console.WriteLine("Visa verifikat");
            break;

        case "3":
            Console.WriteLine("Visa kontoplan");
            break;

        case "4":
            running = false;
            Console.WriteLine("Avslutar Digibok");
            break;

        default:
            Console.WriteLine("Ogiltigt val. Försök igen.");
            break;
    }
}

/*
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
/* var accountService = new AccountService(db);
var accounts = await accountService.GetAllAccountsAsync();
foreach (var account in accounts)
{
    Console.WriteLine($"Konto: {account.AccountNumber} - {account.Name}");
}
*/