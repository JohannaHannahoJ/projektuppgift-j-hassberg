namespace Digibok.Models;

/*
 * Representerar ett enskilt bokföringskonto.
 * Exempelvis 1910 - Bankkonto
 */

public class Account
{
    public int Id { get; set; }
    public int AccountNumber { get; set; }
    public string Name { get; set; } = "";
    public int AccountClassId { get; set; }
    public AccountClass AccountClass { get; set; } = null!;
    public bool IsCustom { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}