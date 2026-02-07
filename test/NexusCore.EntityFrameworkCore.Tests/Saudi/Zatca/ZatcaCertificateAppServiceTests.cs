using System;
using System.Threading.Tasks;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class ZatcaCertificateAppServiceTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IZatcaCertificateAppService _certificateAppService;

    public ZatcaCertificateAppServiceTests()
    {
        _certificateAppService = GetRequiredService<IZatcaCertificateAppService>();
    }

    [Fact]
    public async Task GetList_By_Seller_Should_Return_Seeded_Certificate()
    {
        var result = await _certificateAppService.GetListAsync(
            NexusCoreTestData.SellerId,
            new PagedAndSortedResultRequestDto { MaxResultCount = 10 });

        result.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
        result.Items.ShouldContain(c => c.Id == NexusCoreTestData.CertificateId);
    }

    [Fact]
    public async Task Get_Should_Return_Seeded_Certificate_Details()
    {
        var cert = await _certificateAppService.GetAsync(NexusCoreTestData.CertificateId);

        cert.ShouldNotBeNull();
        cert.SellerId.ShouldBe(NexusCoreTestData.SellerId);
        cert.Environment.ShouldBe(ZatcaEnvironment.Sandbox);
        cert.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Create_Should_Add_New_Certificate()
    {
        var input = new CreateZatcaCertificateDto
        {
            SellerId = NexusCoreTestData.SellerId,
            Environment = ZatcaEnvironment.Sandbox,
            Csid = "new-test-csid-002",
            Secret = "new-test-secret-002",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            IsActive = false
        };

        var result = await _certificateAppService.CreateAsync(input);

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Csid.ShouldBe("new-test-csid-002");
        result.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Create_Active_Certificate_Should_Deactivate_Others()
    {
        var input = new CreateZatcaCertificateDto
        {
            SellerId = NexusCoreTestData.SellerId,
            Environment = ZatcaEnvironment.Production,
            Csid = "production-csid-001",
            Secret = "production-secret-001",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            IsActive = true
        };

        var newCert = await _certificateAppService.CreateAsync(input);

        // New certificate should be active
        newCert.IsActive.ShouldBeTrue();

        // Old seeded certificate should be deactivated
        var oldCert = await _certificateAppService.GetAsync(NexusCoreTestData.CertificateId);
        oldCert.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Delete_Should_Remove_Certificate()
    {
        // Create a certificate to delete
        var created = await _certificateAppService.CreateAsync(new CreateZatcaCertificateDto
        {
            SellerId = NexusCoreTestData.SellerId,
            Environment = ZatcaEnvironment.Sandbox,
            Csid = "delete-csid",
            Secret = "delete-secret",
            IssuedAt = DateTime.UtcNow,
            IsActive = false
        });

        await _certificateAppService.DeleteAsync(created.Id);

        var list = await _certificateAppService.GetListAsync(
            NexusCoreTestData.SellerId,
            new PagedAndSortedResultRequestDto { MaxResultCount = 100 });
        list.Items.ShouldNotContain(c => c.Id == created.Id);
    }

    [Fact]
    public async Task Activate_Should_Deactivate_Others()
    {
        // Create an inactive certificate
        var newCert = await _certificateAppService.CreateAsync(new CreateZatcaCertificateDto
        {
            SellerId = NexusCoreTestData.SellerId,
            Environment = ZatcaEnvironment.Sandbox,
            Csid = "activate-csid",
            Secret = "activate-secret",
            IssuedAt = DateTime.UtcNow,
            IsActive = false
        });

        // Activate it
        await _certificateAppService.ActivateAsync(newCert.Id);

        // It should now be active
        var activated = await _certificateAppService.GetAsync(newCert.Id);
        activated.IsActive.ShouldBeTrue();

        // The seeded certificate should be deactivated
        var oldCert = await _certificateAppService.GetAsync(NexusCoreTestData.CertificateId);
        oldCert.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Deactivate_Should_Set_Inactive()
    {
        // Create an active certificate first
        var newCert = await _certificateAppService.CreateAsync(new CreateZatcaCertificateDto
        {
            SellerId = NexusCoreTestData.SellerId,
            Environment = ZatcaEnvironment.Sandbox,
            Csid = "deactivate-csid",
            Secret = "deactivate-secret",
            IssuedAt = DateTime.UtcNow,
            IsActive = false
        });

        // Deactivate it
        await _certificateAppService.DeactivateAsync(newCert.Id);

        var deactivated = await _certificateAppService.GetAsync(newCert.Id);
        deactivated.IsActive.ShouldBeFalse();
    }
}
