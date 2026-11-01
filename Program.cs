using Digibok.Services;
using Digibok.Data;

Console.WriteLine("Välkommen till Digibok - din digitala bokföring!");

// Importera kontoplanen om den inte redan finns i databasen
var importer = new AccountImportService();
await importer.ImportIfNeededAsync();

// Skapa en koppling till databasen
using var db = new AppDbContext();

// Skapa tjänsten som hanterar verifikat
var _voucherService = new VoucherService(db);
// Skapa tjänsten som hanterar konton
var accountService = new AccountService(db);
// Skapa tjänsten som hanterar rader till och bokföring av verifikaten
var voucherConsoleService = new VoucherConsoleService(_voucherService, accountService);

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
            var voucher = await _voucherService.CreateVoucherAsync(date, description, series);

            Console.WriteLine($"Verifikat: {voucher.VoucherSeries}{voucher.VoucherNumber}, " + $"datum: {voucher.Date}, beskrivning: {voucher.Description}");
            await voucherConsoleService.AddVoucherRowsAsync(voucher.Id);
            await voucherConsoleService.AskToPostVoucherAsync(voucher.Id);
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
                        var vouchers = await _voucherService.GetAllVouchersAsync();

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
                        var postedVouchers = (await _voucherService.GetAllVouchersAsync()).Where(v => v.IsPosted).ToList();

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
                        var draftVouchers = (await _voucherService.GetAllVouchersAsync()).Where(v => !v.IsPosted).ToList();

                        if (draftVouchers.Count == 0)
                        {
                            Console.WriteLine("Det finns inga utkast att visa.");
                        }
                        else
                        {
                            Console.WriteLine("===== UTKAST =====");

                            foreach (var item in draftVouchers)
                            {
                                Console.WriteLine(
                                    $"ID: {item.Id}, Verifikat: {item.VoucherSeries}, " +
                                    $"datum: {item.Date}, beskrivning: {item.Description}");
                            }

                            Console.Write("Ange ID på utkastet du vill öppna (0 för tillbaka): ");

                            if (int.TryParse(Console.ReadLine(), out int draftId) && draftId != 0)
                            {
                                var draft = await _voucherService.GetVoucherByIdAsync(draftId);

                                if (draft == null || draft.IsPosted)
                                {
                                    Console.WriteLine("Utkastet kunde inte hittas.");
                                }
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("===== ÖPPNAT UTKAST =====");
                                    Console.WriteLine($"Datum: {draft.Date}");
                                    Console.WriteLine($"Beskrivning: {draft.Description}");

                                    var rows = draft.VoucherRows;

                                    foreach (var row in rows)
                                    {
                                        Console.WriteLine(
                                            $"{row.Account.AccountNumber} - {row.Account.Name} " +
                                            $"Debet: {row.Debit:N2} Kredit: {row.Credit:N2}");
                                    }

                                    // Meny för det öppnade utkastet.
                                    Console.WriteLine();
                                    Console.WriteLine("===== HANTERA UTKAST =====");
                                    Console.WriteLine("1. Lägg till bokföringsrader");
                                    Console.WriteLine("2. Radera utkast");
                                    Console.WriteLine("3. Tillbaka");
                                    Console.Write("Välj ett alternativ: ");

                                    string? draftChoice = Console.ReadLine();

                                    switch (draftChoice)
                                    {
                                        case "1":
                                            // Lägger till bokföringsrader och frågar om verifikatet ska bokföras.
                                            await voucherConsoleService.AddVoucherRowsAsync(draft.Id);
                                            await voucherConsoleService.AskToPostVoucherAsync(draft.Id);
                                            break;
                                            
                                        case "2":
                                            string? deleteChoice;

                                            do
                                            {
                                                Console.Write("Är du säker på att du vill radera utkastet? (j/n): ");
                                                deleteChoice = Console.ReadLine()?.Trim().ToLower();

                                                if (deleteChoice != "j" && deleteChoice != "n")
                                                {
                                                    Console.WriteLine("Ogiltigt svar. Ange j eller n.");
                                                }

                                            } while (deleteChoice != "j" && deleteChoice != "n");

                                            if (deleteChoice == "j")
                                            {
                                                await _voucherService.DeleteDraftAsync(draft.Id);
                                                Console.WriteLine("Utkastet har raderats.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Utkastet har inte raderats.");
                                            }
                                            break;

                                        case "3":
                                            break;

                                        default:
                                            Console.WriteLine("Ogiltigt val.");
                                            break;
                                    }
                                }
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
            // Hämtar och visar endast aktiva konton.
            var activeAccounts = await accountService.GetActiveAccountsAsync();

            if (activeAccounts.Count == 0)
            {
                Console.WriteLine("Det finns inga konton i din kontoplan.");
            }
            else
            {
                Console.WriteLine("===== KONTOPLAN =====");

                foreach (var account in activeAccounts)
                {
                    Console.WriteLine($"{account.AccountNumber} - {account.Name}");
                }
            }

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