using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class ZatcaInvoiceRepositoryTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IRepository<ZatcaInvoice, Guid> _invoiceRepository;
    private readonly IRepository<ZatcaInvoiceLine, Guid> _invoiceLineRepository;

    public ZatcaInvoiceRepositoryTests()
    {
        _invoiceRepository = GetRequiredService<IRepository<ZatcaInvoice, Guid>>();
        _invoiceLineRepository = GetRequiredService<IRepository<ZatcaInvoiceLine, Guid>>();
    }

    [Fact]
    public async Task Should_Filter_By_Status()
    {
        var draftInvoices = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _invoiceRepository.GetQueryableAsync();
            return await queryable
                .Where(i => i.Status == ZatcaInvoiceStatus.Draft)
                .ToListAsync();
        });

        draftInvoices.ShouldNotBeEmpty();
        draftInvoices.ShouldContain(i => i.Id == NexusCoreTestData.InvoiceDraftId);
        draftInvoices.ShouldAllBe(i => i.Status == ZatcaInvoiceStatus.Draft);
    }

    [Fact]
    public async Task Should_Filter_By_Date_Range()
    {
        var invoices = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _invoiceRepository.GetQueryableAsync();
            return await queryable
                .Where(i =>
                    i.IssueDate >= new DateTime(2024, 6, 1) &&
                    i.IssueDate <= new DateTime(2024, 6, 30))
                .ToListAsync();
        });

        invoices.ShouldNotBeEmpty();
        invoices.ShouldContain(i => i.Id == NexusCoreTestData.InvoiceDraftId);
    }

    [Fact]
    public async Task Should_Filter_By_Seller()
    {
        var invoices = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _invoiceRepository.GetQueryableAsync();
            return await queryable
                .Where(i => i.SellerId == NexusCoreTestData.SellerId)
                .ToListAsync();
        });

        invoices.Count.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Should_Support_Paging_And_Sorting()
    {
        var invoices = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _invoiceRepository.GetQueryableAsync();
            return await queryable
                .OrderByDescending(i => i.IssueDate)
                .Skip(0)
                .Take(2)
                .ToListAsync();
        });

        invoices.Count.ShouldBe(2);
        invoices[0].IssueDate.ShouldBeGreaterThanOrEqualTo(invoices[1].IssueDate);
    }

    [Fact]
    public async Task Should_Query_Invoice_Lines_By_InvoiceId()
    {
        var lines = await WithUnitOfWorkAsync(async () =>
            await _invoiceLineRepository.GetListAsync(
                l => l.InvoiceId == NexusCoreTestData.InvoiceDraftId));

        lines.Count.ShouldBe(2);
        lines.ShouldAllBe(l => l.InvoiceId == NexusCoreTestData.InvoiceDraftId);
        lines.ShouldAllBe(l => l.NetAmount > 0);
    }

    [Fact]
    public async Task Should_Query_By_InvoiceNumber()
    {
        var invoice = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _invoiceRepository.GetQueryableAsync();
            return await queryable
                .Where(i => i.InvoiceNumber == NexusCoreTestData.InvoiceDraftNumber)
                .FirstOrDefaultAsync();
        });

        invoice.ShouldNotBeNull();
        invoice.Id.ShouldBe(NexusCoreTestData.InvoiceDraftId);
    }

    [Fact]
    public async Task Should_Return_Invoice_With_Correct_Totals()
    {
        var invoice = await WithUnitOfWorkAsync(async () =>
            await _invoiceRepository.GetAsync(NexusCoreTestData.InvoiceDraftId));

        invoice.SubTotal.ShouldBeGreaterThan(0);
        invoice.VatAmount.ShouldBeGreaterThan(0);
        invoice.GrandTotal.ShouldBe(invoice.SubTotal + invoice.VatAmount);
    }

    [Fact]
    public async Task Should_Delete_Invoice_And_Lines()
    {
        var invoiceId = Guid.NewGuid();
        var lineId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var invoice = new ZatcaInvoice(
                invoiceId,
                NexusCoreTestData.SellerId,
                "INV-DELETE-REPO-001",
                ZatcaInvoiceType.Simplified,
                DateTime.UtcNow);
            await _invoiceRepository.InsertAsync(invoice);

            var line = new ZatcaInvoiceLine(lineId, invoiceId, "بند حذف", 1, 100m);
            await _invoiceLineRepository.InsertAsync(line);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            await _invoiceLineRepository.DeleteAsync(lineId);
            await _invoiceRepository.DeleteAsync(invoiceId);
        });

        var deletedInvoice = await WithUnitOfWorkAsync(async () =>
            await _invoiceRepository.FirstOrDefaultAsync(i => i.Id == invoiceId));
        deletedInvoice.ShouldBeNull();

        var deletedLine = await WithUnitOfWorkAsync(async () =>
            await _invoiceLineRepository.FirstOrDefaultAsync(l => l.Id == lineId));
        deletedLine.ShouldBeNull();
    }
}
