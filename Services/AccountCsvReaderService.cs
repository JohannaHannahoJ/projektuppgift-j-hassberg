
using Microsoft.VisualBasic.FileIO;
using Digibok.Models;

namespace Digibok.Services;

/*
 * Ansvarar för att läsa in och förbereda data från bas-kontoplan-k1.csv.
*/

public class AccountCsvReaderService
{
    // Läser in kontoplanen från CSV-filen och returnerar en lista med AccountCsvRow-objekt.
    public List<AccountCsvRow> ReadAccounts(string filePath)
    {
        var accounts = new List<AccountCsvRow>();

        // Öppna CSV-filen och hantera semikolon som avgränsare
        using var parser = new TextFieldParser(filePath);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(";");

        // Hoppa över rubrikraden.
        parser.ReadFields();

        // Läs in varje rad i CSV-filen
        while (!parser.EndOfData)
        {
            string[]? fields = parser.ReadFields();

            if (fields == null || fields.Length < 4)
                continue;

            if (!int.TryParse(fields[2].Trim(), out int accountNumber))
                throw new FormatException(
                    $"Ogiltigt kontonummer: {fields[2]}");

            // Skapa ett objekt med rensade värden från CSV-raden
            accounts.Add(new AccountCsvRow
            {
                // rensa bort eventuella extra mellanslag
                ClassCode = fields[0].Trim(),
                ClassName = fields[1].Trim(),
                AccountNumber = accountNumber,
                AccountName = fields[3].Trim()
            });
        }

        return accounts;
    }
}
