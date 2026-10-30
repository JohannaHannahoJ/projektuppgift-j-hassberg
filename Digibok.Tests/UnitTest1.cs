using Digibok.Data;
using Digibok.Models;
using Digibok.Services;
using Microsoft.EntityFrameworkCore;

namespace Digibok.Tests;

/*
 * Tester av befintliga metoder i Digibok
 * Kollar att programmets funktioner fungerar som förväntat
 */

public class UnitTest1
{
    [Fact]
    public async Task IsVoucherBalancedAsync_ReturnsTrue()
    {
        // Skapa en testdatabas
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new AppDbContext(options);

        // Skapa ett testverifikat
        var voucher = new Voucher
        {
            Date = new DateOnly(2026, 10, 30),
            Description = "Testverifikat",
            FiscalYear = 2026,
            VoucherSeries = 'A',
            IsPosted = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();

        // Lägg till två bokföringsrader
        db.VoucherRows.AddRange(
            new VoucherRow
            {
                VoucherId = voucher.Id,
                Voucher = voucher,
                AccountId = 1,
                Debit = 500,
                Credit = 0,
                CreatedAt = DateTime.UtcNow
            },
            new VoucherRow
            {
                VoucherId = voucher.Id,
                Voucher = voucher,
                AccountId = 2,
                Debit = 0,
                Credit = 500,
                CreatedAt = DateTime.UtcNow
            }
        );

        await db.SaveChangesAsync();

        // Använd metoden i VoucherService
        var service = new VoucherService(db);
        bool result = await service.IsVoucherBalancedAsync(voucher.Id);

        // Kontrollera om resultatet är sant
        Assert.True(result);
    }

    [Fact]
    public async Task IsVoucherBalancedAsync_ReturnsFalse()
    {
        // Skapa en testdatabas
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new AppDbContext(options);

        // Skapa ett testverifikat
        var voucher = new Voucher
        {
            Date = new DateOnly(2026, 10, 30),
            Description = "Obalanserat testverifikat",
            FiscalYear = 2026,
            VoucherSeries = 'A',
            IsPosted = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();

        // Lägg till två bokföringsrader med olika belopp
        db.VoucherRows.AddRange(
            new VoucherRow
            {
                VoucherId = voucher.Id,
                Voucher = voucher,
                AccountId = 1,
                Debit = 500,
                Credit = 0,
                CreatedAt = DateTime.UtcNow
            },
            new VoucherRow
            {
                VoucherId = voucher.Id,
                Voucher = voucher,
                AccountId = 2,
                Debit = 0,
                Credit = 300,
                CreatedAt = DateTime.UtcNow
            }
        );

        await db.SaveChangesAsync();

        // Testa metoden
        var service = new VoucherService(db);
        bool result = await service.IsVoucherBalancedAsync(voucher.Id);

        // Kontrollera att resultatet är false
        Assert.False(result);
    }

    [Fact]
    public async Task IsVoucherBalancedAsync_ReturnsFalseIfOneRow()
    {
        // Skapa en testdatabas
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new AppDbContext(options);

        // Skapa ett testverifikat
        var voucher = new Voucher
        {
            Date = new DateOnly(2026, 10, 30),
            Description = "Test med en bokföringsrad",
            FiscalYear = 2026,
            VoucherSeries = 'A',
            IsPosted = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();

        // Lägg till en bokföringsrad
        db.VoucherRows.Add(new VoucherRow
        {
            VoucherId = voucher.Id,
            Voucher = voucher,
            AccountId = 1,
            Debit = 500,
            Credit = 0,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Testa metoden
        var service = new VoucherService(db);
        bool result = await service.IsVoucherBalancedAsync(voucher.Id);

        // Kontrollera att resultatet är false
        Assert.False(result);
    }
}