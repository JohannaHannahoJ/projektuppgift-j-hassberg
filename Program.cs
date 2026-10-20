using Microsoft.VisualBasic.FileIO;

Console.WriteLine("Välkommen till Digibok -din digitala bokföring!");

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

// Öppna CSV-filen och hantera semikolon som avgränsare
using var parser = new TextFieldParser(filePath);
parser.TextFieldType = FieldType.Delimited;
parser.SetDelimiters(";");

// Hoppa över rubrikraden i CSV-filen
parser.ReadFields();

int accountCount = 0;

// Läs varje rad i CSV-filen och skriv ut kontonummer, kontonamn och kontoklass
while (!parser.EndOfData)
{
    string[]? fields = parser.ReadFields();

    if (fields == null || fields.Length < 4)
        continue;

    // rensa bort eventuella extra mellanslag
    string classCode = fields[0].Trim();
    string className = fields[1].Trim();
    string accountNumber = fields[2].Trim();
    string accountName = fields[3].Trim();

    accountCount++;

    // Visa endast de första 10 kontona
    if (accountCount <= 10)
    {
        Console.WriteLine(
            $"{accountNumber} - {accountName} " +
            $"(Klass {classCode}: {className})"
        );
    }
}

Console.WriteLine($"Totalt antal inlästa konton: {accountCount}");
