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

            DateOnly date;
            bool validDate = false;

            // Fortsätter att fråga efter datum tills giltig inmatning
            do
            {
                Console.Write("Bokföringsdatum (ÅÅÅÅ-MM-DD): ");
                string? dateInput = Console.ReadLine()?.Trim();

                if (DateOnly.TryParseExact(dateInput, "yyyy-MM-dd", out date))
                {
                    validDate = true;
                }
                else
                {
                    Console.WriteLine("Ogiltigt datum. Använd formatet åååå-mm-dd.");
                }

            } while (!validDate);

            char series = 'A';
            bool validSeries = false;

            // Fortsätter att fråga efter verifikatserie tills giltig inmatning
            do
            {
                Console.Write("Verifikatserie (A-Z): ");
                string? seriesInput = Console.ReadLine()?.Trim();

                if (seriesInput != null && seriesInput.Trim().Length == 1 && char.IsAsciiLetter(seriesInput[0]))
                {
                    series = char.ToUpperInvariant(seriesInput[0]);
                    validSeries = true;
                }
                else
                {
                    Console.WriteLine("Ange en bokstav mellan A och Z.");
                }

            } while (!validSeries);

            // Be om en beskrivning.
            Console.Write("Beskrivning: ");
            string? description = (Console.ReadLine() ?? "").Trim();

            // Skapa och spara verifikatet
            var voucher = await voucherService.CreateVoucherAsync(date, description, series);

            Console.WriteLine($"Verifikat: {voucher.VoucherSeries}{voucher.VoucherNumber}, " + $"datum: {voucher.Date}, beskrivning: {voucher.Description}");

            break;

        case "2":
            // Undermeny för att visa verifikat
            bool showVoucherMenu = true;

            while (showVoucherMenu)
            {
                Console.WriteLine();
                Console.WriteLine("===== VISA VERIFIKAT =====");
                Console.WriteLine("1. Visa alla");
                Console.WriteLine("2. Visa bokförda");
                Console.WriteLine("3. Visa utkast");
                Console.WriteLine("4. Tillbaka");
                Console.Write("Välj ett alternativ: ");

                string? voucherChoice = Console.ReadLine();

                switch (voucherChoice)
                {
                    case "1":
                        // Hämtar och visar alla verifikat.
                        var vouchers = await voucherService.GetAllVouchersAsync();

                        if (vouchers.Count == 0)
                        {
                            Console.WriteLine("Det finns inga verifikat att visa.");
                        }
                        else
                        {
                            foreach (var item in vouchers)
                            {
                                Console.WriteLine(
                                    $"Verifikat: {item.VoucherSeries}{item.VoucherNumber}, " +
                                    $"datum: {item.Date}, beskrivning: {item.Description}");
                            }
                        }

                        break;

                    case "2":
                        // Hämtar verifikat och filtrerar fram de bokförda verifikaten
                        var postedVouchers = (await voucherService.GetAllVouchersAsync()).Where(v => v.IsPosted).ToList();

                        if (postedVouchers.Count == 0)
                        {
                            Console.WriteLine("Det finns inga bokförda verifikat.");
                        }
                        else
                        {
                            foreach (var item in postedVouchers)
                            {
                                Console.WriteLine(
                                    $"Verifikat: {item.VoucherSeries}{item.VoucherNumber}, " +
                                    $"datum: {item.Date}, beskrivning: {item.Description}");
                            }
                        }

                        break;

                    case "3":
                        // Hämtar verifikat och filtrerar fram utkasten
                        var draftVouchers = (await voucherService.GetAllVouchersAsync()).Where(v => !v.IsPosted).ToList();

                        if (draftVouchers.Count == 0)
                        {
                            Console.WriteLine("Det finns inga utkast att visa.");
                        }
                        else
                        {
                            foreach (var item in draftVouchers)
                            {
                                Console.WriteLine(
                                    $"Verifikat: {item.VoucherSeries}{item.VoucherNumber}, " +
                                    $"datum: {item.Date}, beskrivning: {item.Description}");
                            }
                        }

                        break;

                    case "4":
                    // tillbaka till huvudmeny
                        showVoucherMenu = false;
                        break;

                    default:
                        Console.WriteLine("Ogiltigt val. Försök igen.");
                        break;
                }
            }

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