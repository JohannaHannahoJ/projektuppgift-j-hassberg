namespace Digibok.Models;

/*
 * Representerar en enskild rad i en verifikation.
*/

public class VoucherRow
{
    public int Id { get; set; }
    public int VoucherId { get; set; }
    public Voucher Voucher { get; set; } = null!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public DateTime CreatedAt { get; set; }
}