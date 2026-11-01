using Digibok.Models;

namespace Digibok.Services;

public class VoucherConsoleService
{
    private readonly VoucherService _voucherService;
    private readonly AccountService _accountService;

    public VoucherConsoleService(
        VoucherService voucherService,
        AccountService accountService)
    {
        _voucherService = voucherService;
        _accountService = accountService;
    }

    public async Task AddVoucherRowsAsync(int voucherId)
    {
        // Hämta verifikatet efter Id och kollar att det finns som utkast
        var voucher = await _voucherService.GetVoucherByIdAsync(voucherId);

        if (voucher == null || voucher.IsPosted)
        {
            Console.WriteLine("Verifikatet kunde inte hittas eller är redan bokfört.");
            return;
        }

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
                var account = await _accountService.GetAccountByNumberAsync(accountNumber);

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

                        var accountClasses = await _accountService.GetAllAccountClassesAsync();

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
                            account = await _accountService.CreateAccountAsync(
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
                        await _voucherService.AddVoucherRowAsync(
                            voucher.Id,
                            account.Id,
                            debit,
                            credit);

                        Console.WriteLine("Bokföringsraden har sparats!");

                        // Hämtar alla rader som hittills har lagts till.
                        var rows = await _voucherService.GetVoucherRowsAsync(voucher.Id);

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
            bool isBalanced = await _voucherService.IsVoucherBalancedAsync(voucher.Id);

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
    }

    public async Task AskToPostVoucherAsync(int voucherId)
    {
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
                await _voucherService.PostVoucherAsync(voucherId);
                var postedVoucher = await _voucherService.GetVoucherByIdAsync(voucherId);

                Console.WriteLine($"Verifikatet {postedVoucher?.VoucherSeries}{postedVoucher?.VoucherNumber} har bokförts!");
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
    }
}
