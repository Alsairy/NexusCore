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
public class ZatcaSellerRepositoryTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IRepository<ZatcaSeller, Guid> _sellerRepository;

    public ZatcaSellerRepositoryTests()
    {
        _sellerRepository = GetRequiredService<IRepository<ZatcaSeller, Guid>>();
    }

    [Fact]
    public async Task Should_Query_By_VatRegistrationNumber()
    {
        var seller = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _sellerRepository.GetQueryableAsync();
            return await queryable
                .Where(s => s.VatRegistrationNumber == NexusCoreTestData.SellerVatNumber)
                .FirstOrDefaultAsync();
        });

        seller.ShouldNotBeNull();
        seller.Id.ShouldBe(NexusCoreTestData.SellerId);
        seller.SellerNameAr.ShouldBe(NexusCoreTestData.SellerNameAr);
    }

    [Fact]
    public async Task Should_Filter_By_IsDefault()
    {
        var defaultSellers = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _sellerRepository.GetQueryableAsync();
            return await queryable.Where(s => s.IsDefault).ToListAsync();
        });

        defaultSellers.ShouldNotBeEmpty();
        defaultSellers.ShouldContain(s => s.Id == NexusCoreTestData.SellerId);
    }

    [Fact]
    public async Task Should_Insert_New_Seller()
    {
        var newId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _sellerRepository.InsertAsync(new ZatcaSeller(
                newId,
                "شركة جديدة للاختبار",
                "300000000000055")
            {
                City = "مكة",
                CountryCode = "SA"
            });
        });

        var inserted = await WithUnitOfWorkAsync(async () =>
            await _sellerRepository.GetAsync(newId));

        inserted.ShouldNotBeNull();
        inserted.SellerNameAr.ShouldBe("شركة جديدة للاختبار");
        inserted.VatRegistrationNumber.ShouldBe("300000000000055");
    }

    [Fact]
    public async Task Should_Update_Seller()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var seller = await _sellerRepository.GetAsync(NexusCoreTestData.SellerId);
            seller.City = "المدينة المنورة";
            await _sellerRepository.UpdateAsync(seller);
        });

        var updated = await WithUnitOfWorkAsync(async () =>
            await _sellerRepository.GetAsync(NexusCoreTestData.SellerId));

        updated.City.ShouldBe("المدينة المنورة");
    }

    [Fact]
    public async Task Should_Delete_Seller()
    {
        var newId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _sellerRepository.InsertAsync(new ZatcaSeller(
                newId, "شركة للحذف", "300000000000066"));
        });

        await WithUnitOfWorkAsync(async () =>
        {
            await _sellerRepository.DeleteAsync(newId);
        });

        var deleted = await WithUnitOfWorkAsync(async () =>
            await _sellerRepository.FirstOrDefaultAsync(s => s.Id == newId));

        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Return_All_Seeded_Sellers()
    {
        var count = await WithUnitOfWorkAsync(async () =>
            await _sellerRepository.GetCountAsync());

        count.ShouldBeGreaterThanOrEqualTo(1);
    }
}
