namespace Digibok.Models;

/*
 * Klass för kontoklasshantering i databasen. Kontoklasser används för att gruppera konton i olika kategorier,
 * Exempelvis 12 - Maskiner och inventarier.
*/

public class AccountClass
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}