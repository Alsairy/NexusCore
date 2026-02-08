using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NexusCore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Application service for ZATCA Seller management
/// </summary>
[Authorize(NexusCorePermissions.Zatca.SellersManage)]
public class ZatcaSellerAppService : NexusCoreAppService, IZatcaSellerAppService
{
    private readonly IRepository<ZatcaSeller, Guid> _sellerRepository;

    public ZatcaSellerAppService(IRepository<ZatcaSeller, Guid> sellerRepository)
    {
        _sellerRepository = sellerRepository;
    }

    /// <summary>
    /// Get a paginated list of sellers
    /// </summary>
    public async Task<PagedResultDto<ZatcaSellerDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _sellerRepository.GetQueryableAsync();

        // Get total count
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderByDescending(x => x.IsDefault).ThenBy(x => x.SellerNameAr);
        }

        // Apply paging
        queryable = queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var sellers = await AsyncExecuter.ToListAsync(queryable);

        // Map to DTOs
        var dtos = sellers.Select(MapToDto).ToList();

        return new PagedResultDto<ZatcaSellerDto>(totalCount, dtos);
    }

    /// <summary>
    /// Get a single seller by ID
    /// </summary>
    public async Task<ZatcaSellerDto> GetAsync(Guid id)
    {
        var seller = await _sellerRepository.GetAsync(id);
        return MapToDto(seller);
    }

    /// <summary>
    /// Create a new seller
    /// </summary>
    public async Task<ZatcaSellerDto> CreateAsync(CreateUpdateZatcaSellerDto input)
    {
        var seller = new ZatcaSeller(
            GuidGenerator.Create(),
            input.SellerNameAr,
            input.VatRegistrationNumber)
        {
            SellerNameEn = input.SellerNameEn,
            CommercialRegistrationNumber = input.CommercialRegistrationNumber,
            Street = input.Street,
            BuildingNumber = input.BuildingNumber,
            City = input.City,
            District = input.District,
            PostalCode = input.PostalCode,
            CountryCode = input.CountryCode,
            IsDefault = input.IsDefault
        };

        // If this seller is being set as default, unset all others
        if (seller.IsDefault)
        {
            await UnsetAllDefaultSellersAsync();
        }

        await _sellerRepository.InsertAsync(seller);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToDto(seller);
    }

    /// <summary>
    /// Update an existing seller
    /// </summary>
    public async Task<ZatcaSellerDto> UpdateAsync(Guid id, CreateUpdateZatcaSellerDto input)
    {
        var seller = await _sellerRepository.GetAsync(id);

        seller.SellerNameAr = input.SellerNameAr;
        seller.SellerNameEn = input.SellerNameEn;
        seller.VatRegistrationNumber = input.VatRegistrationNumber;
        seller.CommercialRegistrationNumber = input.CommercialRegistrationNumber;
        seller.Street = input.Street;
        seller.BuildingNumber = input.BuildingNumber;
        seller.District = input.District;
        seller.City = input.City;
        seller.PostalCode = input.PostalCode;
        seller.CountryCode = input.CountryCode;

        await _sellerRepository.UpdateAsync(seller);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToDto(seller);
    }

    /// <summary>
    /// Delete a seller
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await _sellerRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Set a seller as the default seller (deactivates all others)
    /// </summary>
    public async Task SetDefaultAsync(Guid id)
    {
        var seller = await _sellerRepository.GetAsync(id);

        // Unset all other defaults
        await UnsetAllDefaultSellersAsync(id);

        // Set this seller as default
        seller.IsDefault = true;
        await _sellerRepository.UpdateAsync(seller);
        await CurrentUnitOfWork!.SaveChangesAsync();
    }

    /// <summary>
    /// Get the default seller
    /// </summary>
    public async Task<ZatcaSellerDto?> GetDefaultAsync()
    {
        var seller = await _sellerRepository.FirstOrDefaultAsync(x => x.IsDefault);
        return seller != null ? MapToDto(seller) : null;
    }

    #region Private Methods

    private async Task UnsetAllDefaultSellersAsync(Guid? exceptId = null)
    {
        var queryable = await _sellerRepository.GetQueryableAsync();
        var defaultSellers = queryable.Where(x => x.IsDefault);

        if (exceptId.HasValue)
        {
            defaultSellers = defaultSellers.Where(x => x.Id != exceptId.Value);
        }

        var sellers = await AsyncExecuter.ToListAsync(defaultSellers);

        foreach (var seller in sellers)
        {
            seller.IsDefault = false;
            await _sellerRepository.UpdateAsync(seller);
        }
    }

    private static ZatcaSellerDto MapToDto(ZatcaSeller seller)
    {
        return new ZatcaSellerDto
        {
            Id = seller.Id,
            SellerNameAr = seller.SellerNameAr,
            SellerNameEn = seller.SellerNameEn,
            VatRegistrationNumber = seller.VatRegistrationNumber,
            CommercialRegistrationNumber = seller.CommercialRegistrationNumber,
            Street = seller.Street,
            BuildingNumber = seller.BuildingNumber,
            District = seller.District,
            City = seller.City,
            PostalCode = seller.PostalCode,
            CountryCode = seller.CountryCode,
            IsDefault = seller.IsDefault,
            CreationTime = seller.CreationTime
        };
    }

    #endregion
}
