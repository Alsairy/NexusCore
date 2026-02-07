using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NexusCore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace NexusCore.Saudi.Zatca;

[Authorize(NexusCorePermissions.Zatca.CertificatesManage)]
public class ZatcaCertificateAppService : NexusCoreAppService, IZatcaCertificateAppService
{
    private readonly IRepository<ZatcaCertificate, Guid> _certificateRepository;
    private readonly IRepository<ZatcaSeller, Guid> _sellerRepository;

    public ZatcaCertificateAppService(
        IRepository<ZatcaCertificate, Guid> certificateRepository,
        IRepository<ZatcaSeller, Guid> sellerRepository)
    {
        _certificateRepository = certificateRepository;
        _sellerRepository = sellerRepository;
    }

    public async Task<PagedResultDto<ZatcaCertificateDto>> GetListAsync(Guid sellerId, PagedAndSortedResultRequestDto input)
    {
        var queryable = await _certificateRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.SellerId == sellerId);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.CreationTime);
        }

        queryable = queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var certificates = await AsyncExecuter.ToListAsync(queryable);
        var dtos = certificates.Select(MapToDto).ToList();

        return new PagedResultDto<ZatcaCertificateDto>(totalCount, dtos);
    }

    public async Task<ZatcaCertificateDto> GetAsync(Guid id)
    {
        var certificate = await _certificateRepository.GetAsync(id);
        return MapToDto(certificate);
    }

    public async Task<ZatcaCertificateDto> CreateAsync(CreateZatcaCertificateDto input)
    {
        // Verify seller exists
        await _sellerRepository.GetAsync(input.SellerId);

        var certificate = new ZatcaCertificate(
            GuidGenerator.Create(),
            input.SellerId,
            input.Csid,
            input.Secret,
            input.Environment)
        {
            SerialNumber = null,
            IssuedAt = input.IssuedAt,
            ExpiresAt = input.ExpiresAt,
            IsActive = input.IsActive
        };

        // If this certificate is being set as active, deactivate all others for this seller
        if (certificate.IsActive)
        {
            await DeactivateAllCertificatesForSellerAsync(certificate.SellerId);
        }

        await _certificateRepository.InsertAsync(certificate);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToDto(certificate);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _certificateRepository.DeleteAsync(id);
    }

    public async Task ActivateAsync(Guid id)
    {
        var certificate = await _certificateRepository.GetAsync(id);

        // Deactivate all other certificates for this seller first
        await DeactivateAllCertificatesForSellerAsync(certificate.SellerId, id);

        certificate.IsActive = true;
        await _certificateRepository.UpdateAsync(certificate);
        await CurrentUnitOfWork!.SaveChangesAsync();
    }

    public async Task DeactivateAsync(Guid id)
    {
        var certificate = await _certificateRepository.GetAsync(id);
        certificate.IsActive = false;
        await _certificateRepository.UpdateAsync(certificate);
        await CurrentUnitOfWork!.SaveChangesAsync();
    }

    #region Private Methods

    private async Task DeactivateAllCertificatesForSellerAsync(Guid sellerId, Guid? exceptId = null)
    {
        var queryable = await _certificateRepository.GetQueryableAsync();
        var activeCertificates = queryable.Where(x => x.SellerId == sellerId && x.IsActive);

        if (exceptId.HasValue)
        {
            activeCertificates = activeCertificates.Where(x => x.Id != exceptId.Value);
        }

        var certificates = await AsyncExecuter.ToListAsync(activeCertificates);

        foreach (var cert in certificates)
        {
            cert.IsActive = false;
            await _certificateRepository.UpdateAsync(cert);
        }
    }

    private static ZatcaCertificateDto MapToDto(ZatcaCertificate certificate)
    {
        return new ZatcaCertificateDto
        {
            Id = certificate.Id,
            SellerId = certificate.SellerId,
            Environment = certificate.Environment,
            Csid = certificate.CertificateContent,
            Secret = certificate.PrivateKeyEncrypted,
            IssuedAt = certificate.IssuedAt ?? DateTime.MinValue,
            ExpiresAt = certificate.ExpiresAt,
            IsActive = certificate.IsActive,
            CreationTime = certificate.CreationTime
        };
    }

    #endregion
}
