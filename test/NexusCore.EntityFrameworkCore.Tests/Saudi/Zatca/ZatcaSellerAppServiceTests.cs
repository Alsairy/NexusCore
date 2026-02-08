using System;
using System.Threading.Tasks;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class ZatcaSellerAppServiceTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IZatcaSellerAppService _sellerAppService;

    public ZatcaSellerAppServiceTests()
    {
        _sellerAppService = GetRequiredService<IZatcaSellerAppService>();
    }

    [Fact]
    public async Task GetList_Should_Return_Seeded_Seller()
    {
        var result = await _sellerAppService.GetListAsync(
            new PagedAndSortedResultRequestDto { MaxResultCount = 10 });

        result.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
        result.Items.ShouldContain(s => s.Id == NexusCoreTestData.SellerId);
    }

    [Fact]
    public async Task Get_Should_Return_Seeded_Seller_Details()
    {
        var seller = await _sellerAppService.GetAsync(NexusCoreTestData.SellerId);

        seller.ShouldNotBeNull();
        seller.SellerNameAr.ShouldBe(NexusCoreTestData.SellerNameAr);
        seller.SellerNameEn.ShouldBe(NexusCoreTestData.SellerNameEn);
        seller.VatRegistrationNumber.ShouldBe(NexusCoreTestData.SellerVatNumber);
        seller.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public async Task Create_Should_Add_New_Seller()
    {
        var input = new CreateUpdateZatcaSellerDto
        {
            SellerNameAr = "شركة جديدة",
            SellerNameEn = "New Company",
            VatRegistrationNumber = "300000000000099",
            City = "جدة",
            CountryCode = "SA"
        };

        var result = await _sellerAppService.CreateAsync(input);

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.SellerNameAr.ShouldBe(input.SellerNameAr);
        result.VatRegistrationNumber.ShouldBe(input.VatRegistrationNumber);
    }

    [Fact]
    public async Task Update_Should_Modify_Seller()
    {
        var input = new CreateUpdateZatcaSellerDto
        {
            SellerNameAr = "شركة محدثة",
            SellerNameEn = "Updated Company",
            VatRegistrationNumber = NexusCoreTestData.SellerVatNumber,
            City = "الدمام",
            CountryCode = "SA"
        };

        var result = await _sellerAppService.UpdateAsync(NexusCoreTestData.SellerId, input);

        result.SellerNameAr.ShouldBe("شركة محدثة");
        result.SellerNameEn.ShouldBe("Updated Company");
        result.City.ShouldBe("الدمام");
    }

    [Fact]
    public async Task Delete_Should_Remove_Seller()
    {
        // Create a seller to delete (don't delete the seeded one)
        var created = await _sellerAppService.CreateAsync(new CreateUpdateZatcaSellerDto
        {
            SellerNameAr = "شركة للحذف",
            VatRegistrationNumber = "300000000000088"
        });

        await _sellerAppService.DeleteAsync(created.Id);

        var list = await _sellerAppService.GetListAsync(
            new PagedAndSortedResultRequestDto { MaxResultCount = 100 });
        list.Items.ShouldNotContain(s => s.Id == created.Id);
    }

    [Fact]
    public async Task SetDefault_Should_Deactivate_Others()
    {
        // Create a new seller and set it as default
        var newSeller = await _sellerAppService.CreateAsync(new CreateUpdateZatcaSellerDto
        {
            SellerNameAr = "شركة افتراضية جديدة",
            VatRegistrationNumber = "300000000000077"
        });

        await _sellerAppService.SetDefaultAsync(newSeller.Id);

        // Verify new seller is default
        var defaultSeller = await _sellerAppService.GetDefaultAsync();
        defaultSeller.ShouldNotBeNull();
        defaultSeller.Id.ShouldBe(newSeller.Id);

        // Verify old seeded seller is no longer default
        var oldSeller = await _sellerAppService.GetAsync(NexusCoreTestData.SellerId);
        oldSeller.IsDefault.ShouldBeFalse();

        // Restore original default to avoid test ordering issues
        await _sellerAppService.SetDefaultAsync(NexusCoreTestData.SellerId);
    }

    [Fact]
    public async Task GetDefault_Should_Return_Seeded_Default()
    {
        var defaultSeller = await _sellerAppService.GetDefaultAsync();

        defaultSeller.ShouldNotBeNull();
        defaultSeller.Id.ShouldBe(NexusCoreTestData.SellerId);
        defaultSeller.IsDefault.ShouldBeTrue();
    }
}
