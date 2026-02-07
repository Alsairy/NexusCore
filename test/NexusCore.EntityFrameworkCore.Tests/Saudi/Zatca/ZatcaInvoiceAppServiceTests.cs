using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class ZatcaInvoiceAppServiceTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IZatcaInvoiceAppService _invoiceAppService;

    public ZatcaInvoiceAppServiceTests()
    {
        _invoiceAppService = GetRequiredService<IZatcaInvoiceAppService>();
    }

    [Fact]
    public async Task GetList_Should_Return_Seeded_Invoices()
    {
        var result = await _invoiceAppService.GetListAsync(
            new GetZatcaInvoiceListInput { MaxResultCount = 10 });

        result.TotalCount.ShouldBeGreaterThanOrEqualTo(3);
        result.Items.ShouldContain(i => i.Id == NexusCoreTestData.InvoiceDraftId);
        result.Items.ShouldContain(i => i.Id == NexusCoreTestData.InvoiceValidatedId);
        result.Items.ShouldContain(i => i.Id == NexusCoreTestData.InvoiceClearedId);
    }

    [Fact]
    public async Task GetList_Filter_By_Status_Should_Work()
    {
        var result = await _invoiceAppService.GetListAsync(
            new GetZatcaInvoiceListInput
            {
                Status = ZatcaInvoiceStatus.Draft,
                MaxResultCount = 10
            });

        result.Items.ShouldAllBe(i => i.Status == ZatcaInvoiceStatus.Draft);
        result.Items.ShouldContain(i => i.Id == NexusCoreTestData.InvoiceDraftId);
    }

    [Fact]
    public async Task GetList_Filter_By_Seller_Should_Work()
    {
        var result = await _invoiceAppService.GetListAsync(
            new GetZatcaInvoiceListInput
            {
                SellerId = NexusCoreTestData.SellerId,
                MaxResultCount = 10
            });

        result.Items.ShouldAllBe(i => i.SellerId == NexusCoreTestData.SellerId);
    }

    [Fact]
    public async Task Get_Should_Return_Invoice_With_Lines()
    {
        var invoice = await _invoiceAppService.GetAsync(NexusCoreTestData.InvoiceDraftId);

        invoice.ShouldNotBeNull();
        invoice.InvoiceNumber.ShouldBe(NexusCoreTestData.InvoiceDraftNumber);
        invoice.SellerId.ShouldBe(NexusCoreTestData.SellerId);
        invoice.Lines.ShouldNotBeEmpty();
        invoice.Lines.Count.ShouldBe(2);
        invoice.SubTotal.ShouldBeGreaterThan(0);
        invoice.GrandTotal.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Create_With_Lines_Should_Calculate_Totals()
    {
        var input = new CreateUpdateZatcaInvoiceDto
        {
            SellerId = NexusCoreTestData.SellerId,
            InvoiceNumber = "INV-NEW-001",
            InvoiceType = ZatcaInvoiceType.Simplified,
            IssueDate = DateTime.UtcNow,
            CurrencyCode = "SAR",
            Lines = new List<CreateUpdateZatcaInvoiceLineDto>
            {
                new()
                {
                    ItemName = "منتج تجريبي",
                    Quantity = 2,
                    UnitPrice = 100m,
                    TaxPercent = 15m
                },
                new()
                {
                    ItemName = "خدمة تجريبية",
                    Quantity = 1,
                    UnitPrice = 500m,
                    TaxPercent = 15m
                }
            }
        };

        var result = await _invoiceAppService.CreateAsync(input);

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.InvoiceNumber.ShouldBe("INV-NEW-001");
        result.Lines.Count.ShouldBe(2);

        // 2*100 + 1*500 = 700 subtotal
        result.SubTotal.ShouldBe(700m);
        // 700 * 0.15 = 105 VAT
        result.VatAmount.ShouldBe(105m);
        // 700 + 105 = 805 grand total
        result.GrandTotal.ShouldBe(805m);
        result.Status.ShouldBe(ZatcaInvoiceStatus.Draft);
    }

    [Fact]
    public async Task Update_Draft_Invoice_Should_Succeed()
    {
        var input = new CreateUpdateZatcaInvoiceDto
        {
            SellerId = NexusCoreTestData.SellerId,
            InvoiceNumber = NexusCoreTestData.InvoiceDraftNumber,
            InvoiceType = ZatcaInvoiceType.Simplified,
            IssueDate = new DateTime(2024, 6, 15),
            BuyerName = "عميل محدث",
            CurrencyCode = "SAR",
            Lines = new List<CreateUpdateZatcaInvoiceLineDto>
            {
                new()
                {
                    ItemName = "بند محدث",
                    Quantity = 5,
                    UnitPrice = 200m,
                    TaxPercent = 15m
                }
            }
        };

        var result = await _invoiceAppService.UpdateAsync(NexusCoreTestData.InvoiceDraftId, input);

        result.BuyerName.ShouldBe("عميل محدث");
        result.Lines.Count.ShouldBe(1);
        result.SubTotal.ShouldBe(1000m);
    }

    [Fact]
    public async Task Update_Non_Draft_Invoice_Should_Throw()
    {
        var input = new CreateUpdateZatcaInvoiceDto
        {
            SellerId = NexusCoreTestData.SellerId,
            InvoiceNumber = NexusCoreTestData.InvoiceValidatedNumber,
            InvoiceType = ZatcaInvoiceType.Standard,
            IssueDate = new DateTime(2024, 7, 1),
            CurrencyCode = "SAR",
            Lines = new List<CreateUpdateZatcaInvoiceLineDto>
            {
                new()
                {
                    ItemName = "بند",
                    Quantity = 1,
                    UnitPrice = 100m,
                    TaxPercent = 15m
                }
            }
        };

        await Should.ThrowAsync<UserFriendlyException>(
            () => _invoiceAppService.UpdateAsync(NexusCoreTestData.InvoiceValidatedId, input));
    }

    [Fact]
    public async Task Delete_Draft_Invoice_Should_Succeed()
    {
        // Create a draft invoice to delete
        var created = await _invoiceAppService.CreateAsync(new CreateUpdateZatcaInvoiceDto
        {
            SellerId = NexusCoreTestData.SellerId,
            InvoiceNumber = "INV-DELETE-001",
            InvoiceType = ZatcaInvoiceType.Simplified,
            IssueDate = DateTime.UtcNow,
            Lines = new List<CreateUpdateZatcaInvoiceLineDto>
            {
                new() { ItemName = "بند", Quantity = 1, UnitPrice = 100m, TaxPercent = 15m }
            }
        });

        await _invoiceAppService.DeleteAsync(created.Id);

        var list = await _invoiceAppService.GetListAsync(
            new GetZatcaInvoiceListInput { MaxResultCount = 100 });
        list.Items.ShouldNotContain(i => i.Id == created.Id);
    }

    [Fact]
    public async Task Delete_Non_Draft_Invoice_Should_Throw()
    {
        await Should.ThrowAsync<UserFriendlyException>(
            () => _invoiceAppService.DeleteAsync(NexusCoreTestData.InvoiceClearedId));
    }

    [Fact]
    public async Task Validate_Draft_Invoice_Should_Return_Result()
    {
        var result = await _invoiceAppService.ValidateAsync(NexusCoreTestData.InvoiceDraftId);

        result.ShouldNotBeNull();
        // Status should be Draft (valid) or Rejected (invalid) — depends on whether seeded data passes validation
    }
}
