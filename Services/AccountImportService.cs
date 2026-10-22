using Microsoft.EntityFrameworkCore;
using Digibok.Data;

namespace Digibok.Services;

/*
* Ansvarar för att hantera importen av kontoplanen från CSV-filen till databasen.
*/


public class AccountImportService
{
    public async Task ImportIfNeededAsync()
    {
        // Skapa en instans av AppDbContext för att interagera med databasen
        using var db = new AppDbContext();

        if (await db.Accounts.AnyAsync())
        {
            Console.WriteLine("Kontoplanen finns redan i databasen.");
        }
        else
        {
            Console.WriteLine(
                "Hämtar bas-kontoplan.");

            // Sökväg till CSV-fil med kontoplan
            string filePath = Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "bas-kontoplan-k1.csv"
            );

            // Avbryt om CSV-filen inte hittas
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Hittade inte bas-kontoplan.");
                return;
            }

           // Läs in och förbered kontoplanens rader från CSV-filen
            var csvReader = new AccountCsvReaderService();
            var accounts = csvReader.ReadAccounts(filePath);

            Console.WriteLine($"Totalt antal inlästa konton: {accounts.Count}");

        }
    }
}