namespace Digibok.Models;

    /*
     * Klass för ett enkilt bokföringskonto i databasen.
     * Exempelvis 1910 - Bankkonto, 3010 - Försäljning varor inom Sverige.
     */

public class Account
{
    public int Id { get; set; }
    public int AccountNumber { get; set; }
    public string Name { get; set; } = "";
    public int AccountClassId { get; set; }
    public string? Description { get; set; }
    public AccountClass? AccountClass { get; set; }
}