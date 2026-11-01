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
// Skapa tjänsten som hanterar konton
var accountService = new AccountService(db);

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

            // Frågar användaren om en bokföringsrad ska läggas till. Kontrollerar att svaret är j eller n
            string? answer;

            do
            {
                Console.Write("Vill du lägga till en bokföringsrad? (j/n): ");
                answer = Console.ReadLine()?.Trim().ToLower();

                if (answer != "j" && answer != "n")
                {
                    Console.WriteLine("Ogiltigt svar. Ange j eller n.");
                }

            } while (answer != "j" && answer != "n");

            if (answer == "j")
            {
                string? addAnotherRow = "j";

                do
                {
                    // Ber användaren ange ett kontonummer.
                    int accountNumber;
                    bool validAccountNumber = false;

                    do
                    {
                        Console.Write("Ange kontonummer: ");
                        string? input = Console.ReadLine()?.Trim();

                        if (int.TryParse(input, out accountNumber) && accountNumber >= 1000 && accountNumber <= 8999)
                        {
                            validAccountNumber = true;
                        }
                        else
                        {
                            Console.WriteLine("Ange ett fyrsiffrigt kontonummer mellan 1000 och 8999.");
                        }

                    } while (!validAccountNumber);

                    // Söker efter kontot i databasen.
                    var account = await accountService.GetAccountByNumberAsync(accountNumber);

                    bool addRow = true;

                    if (account == null)
                    {
                        Console.WriteLine("Kontot finns inte i kontoplanen.");

                        string? createAccount;


                        do
                        {
                            Console.Write("Vill du skapa ett eget konto? (j/n): ");
                            createAccount = Console.ReadLine()?.Trim().ToLower();

                            if (createAccount != "j" && createAccount != "n")
                            {
                                Console.WriteLine("Ogiltigt svar. Ange j eller n.");
                            }

                        } while (createAccount != "j" && createAccount != "n");

                        if (createAccount == "j")
                        {
                            string? accountName;

                            do
                            {
                                Console.Write("Ange kontonamn: ");
                                accountName = Console.ReadLine()?.Trim();

                                if (string.IsNullOrWhiteSpace(accountName))
                                {
                                    Console.WriteLine("Kontonamnet får inte vara tomt.");
                                }

                            } while (string.IsNullOrWhiteSpace(accountName));

                            // Hämtar kontoklassen automatiskt från första siffran.
                            string classCode = accountNumber.ToString()[0].ToString();

                            var accountClasses = await accountService.GetAllAccountClassesAsync();

                            var accountClass = accountClasses
                                .FirstOrDefault(c => c.Code == classCode);

                            if (accountClass == null)
                            {
                                Console.WriteLine("Kunde inte hitta kontoklassen i databasen.");
                                addRow = false;
                            }
                            else
                            {
                                // Skapar kontot med rätt kontoklass.
                                account = await accountService.CreateAccountAsync(
                                    accountNumber,
                                    accountName,
                                    accountClass.Id);

                                Console.WriteLine($"Kontot har skapats: {account.AccountNumber} - {account.Name}");
                            }
                        }
                        else
                        {
                            addRow = false;
                            Console.WriteLine("Ingen bokföringsrad läggs till.");
                        }
                    }

                    // Om kontot finns eller har skapats
                    if (account != null && addRow)
                    {
                        Console.WriteLine($"Använder konto: {account.AccountNumber} - {account.Name}");

                        // Välj debet eller kredit.
                        string? side;

                        do
                        {
                            Console.Write("Vill du bokföra på debet eller kredit? (d/k): ");
                            side = Console.ReadLine()?.Trim().ToLower();

                            if (side != "d" && side != "k")
                            {
                                Console.WriteLine("Ogiltigt svar. Ange d eller k.");
                            }

                        } while (side != "d" && side != "k");

                        decimal amount = 0;
                        bool validAmount = false;

                        do
                        {
                            Console.Write("Ange belopp: ");
                            string? amountInput = Console.ReadLine();

                            if (decimal.TryParse(amountInput, out amount) && amount > 0)
                            {
                                validAmount = true;
                            }
                            else
                            {
                                Console.WriteLine("Ogiltigt belopp. Ange ett tal som är större än noll.");
                            }

                        } while (!validAmount);

                        decimal debit = 0;
                        decimal credit = 0;

                        if (side == "d")
                        {
                            debit = amount;
                        }
                        else
                        {
                            credit = amount;
                        }

                        try
                        {
                            await voucherService.AddVoucherRowAsync(
                                voucher.Id,
                                account.Id,
                                debit,
                                credit);

                            Console.WriteLine("Bokföringsraden har sparats!");

                            // Hämtar alla rader som hittills har lagts till.
                            var rows = await voucherService.GetVoucherRowsAsync(voucher.Id);

                            // Visar verifikatet.
                            Console.WriteLine();
                            Console.WriteLine("===== VERIFIKAT =====");
                            Console.WriteLine($"Datum: {voucher.Date}");
                            Console.WriteLine($"Beskrivning: {voucher.Description}");
                            Console.WriteLine();

                            Console.WriteLine(
                                $"{"Konto",-8} {"Kontonamn",-38} {"Debet",14} {"Kredit",14}");

                            Console.WriteLine(new string('-', 78));

                            foreach (var row in rows)
                            {
                                string name = row.Account.Name;

                                if (name.Length > 38)
                                {
                                    name = name[..35] + "...";
                                }

                                Console.WriteLine(
                                    $"{row.Account.AccountNumber,-8} " +
                                    $"{name,-38} " +
                                    $"{row.Debit,14:N2} " +
                                    $"{row.Credit,14:N2}");
                            }

                            decimal totalDebit = rows.Sum(row => row.Debit);
                            decimal totalCredit = rows.Sum(row => row.Credit);

                            Console.WriteLine(new string('-', 78));

                            Console.WriteLine(
                                $"{"Summa",-47} {totalDebit,14:N2} {totalCredit,14:N2}");

                            Console.WriteLine();

                            if (totalDebit == totalCredit && rows.Count >= 2)
                            {
                                Console.WriteLine("Status: Verifikatet balanserar!");
                            }
                            else
                            {
                                Console.WriteLine("Status: Verifikatet balanserar inte ännu.");
                            }

                            Console.WriteLine();
                        }
                        catch (ArgumentException ex)
                        {
                            Console.WriteLine($"Fel: {ex.Message}");
                        }
                    }

                    // Frågar om användaren vill lägga till rad
                    do
                    {
                        Console.Write("Vill du lägga till en ny bokföringsrad? (j/n): ");
                        addAnotherRow = Console.ReadLine()?.Trim().ToLower();

                        if (addAnotherRow != "j" && addAnotherRow != "n")
                        {
                            Console.WriteLine("Ogiltigt svar. Ange j eller n.");
                        }

                    } while (addAnotherRow != "j" && addAnotherRow != "n");

                } while (addAnotherRow == "j");

                // Kontrollerar om verifikatet balanserar.
                bool isBalanced = await voucherService.IsVoucherBalancedAsync(voucher.Id);

                if (isBalanced)
                {
                    Console.WriteLine("Verifikatet är balanserat och kan gå vidare till bokföring.");
                }
                else
                {
                    Console.WriteLine("OBS! Verifikatet balanserar inte.");
                    Console.WriteLine("Det sparas som ett utkast tills det har kompletterats.");
                }
            }

            // Frågar om användaren vill bokföra verifikatet.
            string? postChoice;

            do
            {
                Console.Write("Vill du bokföra verifikatet? (j/n): ");
                postChoice = Console.ReadLine()?.Trim().ToLower();

                if (postChoice != "j" && postChoice != "n")
                {
                    Console.WriteLine("Ogiltigt svar. Ange j eller n.");
                }

            } while (postChoice != "j" && postChoice != "n");

            if (postChoice == "j")
            {
                try
                {
                    await voucherService.PostVoucherAsync(voucher.Id);

                    Console.WriteLine(
                        $"Verifikatet {voucher.VoucherSeries}{voucher.VoucherNumber} har bokförts!");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Verifikatet kunde inte bokföras: {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Fel: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Verifikatet sparas som utkast.");
            }

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
                                var draft = await voucherService.GetVoucherByIdAsync(draftId);

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
                                    Console.WriteLine("1. Radera utkast");
                                    Console.WriteLine("2. Tillbaka");
                                    Console.Write("Välj ett alternativ: ");

                                    string? draftChoice = Console.ReadLine();

                                    switch (draftChoice)
                                    {
                                        case "1":
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
                                                await voucherService.DeleteDraftAsync(draft.Id);
                                                Console.WriteLine("Utkastet har raderats.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Utkastet har inte raderats.");
                                            }
                                            break;

                                        case "2":
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