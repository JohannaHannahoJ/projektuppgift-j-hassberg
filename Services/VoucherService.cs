using Microsoft.EntityFrameworkCore;
using Digibok.Data;
using Digibok.Models;

namespace Digibok.Services;

/*
* Ansvarar för att hantera bokföring av verifikat.
*/
public class VoucherService
{
    private readonly AppDbContext _db;

    public VoucherService(AppDbContext db)
    {
        _db = db;
    }

    // Skapar ett nytt verifikat och sparar det i databasen.
    public async Task<Voucher> CreateVoucherAsync(
        DateOnly date,
        string description,
        char voucherSeries)
    {
        // Hitta det högsta verifikationsnumret för vald serie och år
        var lastVoucherNumber = await _db.Vouchers
            .Where(v => v.FiscalYear == date.Year && v.VoucherSeries == voucherSeries)
            .Select(v => (int?)v.VoucherNumber)
            .MaxAsync();

        // Nästa nummer blir 1 om serien saknar verifikat,
        // annars det högsta numret plus 1.
        var nextVoucherNumber = (lastVoucherNumber ?? 0) + 1;

        var voucher = new Voucher
        {
            Date = date,
            Description = description,
            FiscalYear = date.Year,
            VoucherSeries = voucherSeries,
            VoucherNumber = nextVoucherNumber,
            IsPosted = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Vouchers.Add(voucher);
        await _db.SaveChangesAsync();

        return voucher;
    }
}
