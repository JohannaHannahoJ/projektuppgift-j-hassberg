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
        var voucher = new Voucher
        {
            Date = date,
            Description = description,
            FiscalYear = date.Year,
            VoucherSeries = voucherSeries,
            VoucherNumber = null,
            IsPosted = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Vouchers.Add(voucher);
        await _db.SaveChangesAsync();

        return voucher;
    }

    // Lägg till en bokföringsrad på ett befintligt verifikat.
    public async Task<VoucherRow> AddVoucherRowAsync(
        int voucherId,
        int accountId,
        decimal debit,
        decimal credit)
    {
        var row = new VoucherRow
        {
            VoucherId = voucherId,
            AccountId = accountId,
            Debit = debit,
            Credit = credit,
            CreatedAt = DateTime.UtcNow
        };

        _db.VoucherRows.Add(row);
        await _db.SaveChangesAsync();

        return row;
    }

    // Kontrollerar om ett verifikat balanserar.
    public async Task<bool> IsVoucherBalancedAsync(int voucherId)
    {
        // Hämta alla bokföringsrader som hör till verifikatet.
        var rows = await _db.VoucherRows
            .Where(row => row.VoucherId == voucherId)
            .ToListAsync();

        // Summera alla debetbelopp.
        var totalDebit = rows.Sum(row => row.Debit);

        // Summera alla kreditbelopp.
        var totalCredit = rows.Sum(row => row.Credit);

        // Verifikatet balanserar om summorna är lika.
        return totalDebit == totalCredit;
    }

    // Hämta verifikaten från databasen
    public async Task<List<Voucher>> GetAllVouchersAsync()
    {
        return await _db.Vouchers
            .OrderBy(v => v.Date) // Äldsta datum först
            .ThenBy(v => v.VoucherSeries) // Serie i bokstavsordning
            .ThenBy(v => v.VoucherNumber) // Nummer i stigande ordning
            .ToListAsync();
    }
}
