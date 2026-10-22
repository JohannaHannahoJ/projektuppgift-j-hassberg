namespace Digibok.Models;

/*
 * Representerar bilagor som är kopplad till en verifikation.
*/

public class VoucherAttachment
{
    public int Id { get; set; }
    public int VoucherId { get; set; }
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string ContentType { get; set; } = "";
    public Voucher Voucher { get; set; } = null!;

}