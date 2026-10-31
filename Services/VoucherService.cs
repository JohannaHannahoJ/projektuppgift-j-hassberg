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
        // En rad får inte ha belopp på både debet och kredit.
        if (debit > 0 && credit > 0)
        {
            throw new ArgumentException("En rad kan inte ha både debet och kredit.");
        }

        // Debet och kredit får inte vara negativa.
        if (debit < 0 || credit < 0)
        {
            throw new ArgumentException("Belopp kan inte vara negativa. Ange noll eller mer.");
        }

        // En bokföringsrad måste ha ett belopp.
        if (debit == 0 && credit == 0)
        {
            throw new ArgumentException("Ange ett belopp på antingen debet eller kredit.");
        }

        // Kontrollera att verifikatet finns
        var voucherExists = await _db.Vouchers
            .AnyAsync(v => v.Id == voucherId);

        if (!voucherExists)
        {
            throw new ArgumentException("Verifikatet finns inte.");
        }

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

        // Kolla att verifikatet har minst två bokföringsrader
        if (rows.Count < 2)
        {
            return false;
        }

        // Summera alla debetbelopp
        var totalDebit = rows.Sum(row => row.Debit);

        // Summera alla kreditbelopp
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

    // Hämtar alla bokföringsrader för ett verifikat.
    public async Task<List<VoucherRow>> GetVoucherRowsAsync(int voucherId)
    {
        return await _db.VoucherRows
            .Include(row => row.Account)
            .Where(row => row.VoucherId == voucherId)
            .ToListAsync();
    }

    // Bokför verifiakt om det balanserar
    public async Task PostVoucherAsync(int voucherId)
    {
        // Hämtar verifikatet och dess bokföringsrader samt konton.
        var voucher = await _db.Vouchers
            .Include(v => v.VoucherRows)
            .ThenInclude(row => row.Account)
            .FirstOrDefaultAsync(v => v.Id == voucherId);

        if (voucher == null)
        {
            throw new ArgumentException("Verifikatet finns inte.");
        }

        if (voucher.IsPosted)
        {
            throw new InvalidOperationException("Verifikatet är redan bokfört.");
        }

        // Kontrollerar att verifikatet balanserar.
        if (voucher.VoucherRows.Count < 2)
        {
            throw new InvalidOperationException(
                "Verifikatet måste ha minst två bokföringsrader.");
        }

        decimal totalDebit = voucher.VoucherRows.Sum(row => row.Debit);
        decimal totalCredit = voucher.VoucherRows.Sum(row => row.Credit);

        if (totalDebit != totalCredit)
        {
            throw new InvalidOperationException(
                "Verifikatet balanserar inte och kan därför inte bokföras.");
        }

        // Hämtar senaste verifikatnumret för samma år och serie.
        int lastNumber = await _db.Vouchers
            .Where(v => v.FiscalYear == voucher.FiscalYear
                        && v.VoucherSeries == voucher.VoucherSeries
                        && v.IsPosted
                        && v.VoucherNumber.HasValue)
            .MaxAsync(v => (int?)v.VoucherNumber) ?? 0;

        // Tilldelar nästa nummer.
        voucher.VoucherNumber = lastNumber + 1;

        // Markerar verifikatet som bokfört.
        voucher.IsPosted = true;

        // Aktiverar alla konton som används i verifikatet.
        foreach (var row in voucher.VoucherRows)
        {
            row.Account.IsActive = true;
        }

        await _db.SaveChangesAsync();
    }
}
