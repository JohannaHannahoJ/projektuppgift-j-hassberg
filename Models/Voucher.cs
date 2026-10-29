namespace Digibok.Models;

/*
 * Representerar en verifikation i bokföringen.
*/

public class Voucher
{
    public int Id { get; set; }
    public int FiscalYear { get; set; }
    public char VoucherSeries { get; set; }
    public int? VoucherNumber { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; } = "";
    public bool IsPosted { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<VoucherRow> VoucherRows { get; set; } = [];
    public List<VoucherAttachment> Attachments { get; set; } = [];
}