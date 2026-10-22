using Digibok.Services;

Console.WriteLine("Välkommen till Digibok -din digitala bokföring!");

// Importera kontoplanen om den inte redan finns i databasen
var importer = new AccountImportService();
await importer.ImportIfNeededAsync();

