namespace Digibok.Models;

/*
 * Representerar en kontoklass som grupperar bokföringskonton.
 * Exempelvis 1 - Tillgångar.
*/

public class AccountClass
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public List<Account> Accounts { get; set; } = [];
}