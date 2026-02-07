using System;
using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Full DTO for ZATCA Seller entity
/// </summary>
public class ZatcaSellerDto
{
    public Guid Id { get; set; }

    public string SellerNameAr { get; set; } = string.Empty;

    public string? SellerNameEn { get; set; }

    public string VatRegistrationNumber { get; set; } = string.Empty;

    public string? CommercialRegistrationNumber { get; set; }

    public string? Street { get; set; }

    public string? BuildingNumber { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public string? PostalCode { get; set; }

    public string? CountryCode { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreationTime { get; set; }
}

/// <summary>
/// DTO for creating or updating a ZATCA Seller
/// </summary>
public class CreateUpdateZatcaSellerDto
{
    [Required]
    [StringLength(ZatcaConsts.MaxSellerNameLength)]
    public string SellerNameAr { get; set; } = string.Empty;

    [StringLength(ZatcaConsts.MaxSellerNameLength)]
    public string? SellerNameEn { get; set; }

    [Required]
    [StringLength(ZatcaConsts.MaxVatRegistrationNumberLength)]
    [RegularExpression(@"^\d{15}$", ErrorMessage = "VAT Registration Number must be exactly 15 digits")]
    public string VatRegistrationNumber { get; set; } = string.Empty;

    [StringLength(ZatcaConsts.MaxCommercialRegistrationNumberLength)]
    public string? CommercialRegistrationNumber { get; set; }

    [StringLength(ZatcaConsts.MaxAddressFieldLength)]
    public string? Street { get; set; }

    [StringLength(ZatcaConsts.MaxAddressFieldLength)]
    public string? BuildingNumber { get; set; }

    [StringLength(ZatcaConsts.MaxAddressFieldLength)]
    public string? City { get; set; }

    [StringLength(ZatcaConsts.MaxAddressFieldLength)]
    public string? District { get; set; }

    [StringLength(ZatcaConsts.MaxPostalCodeLength)]
    public string? PostalCode { get; set; }

    [StringLength(ZatcaConsts.MaxCountryCodeLength)]
    public string CountryCode { get; set; } = ZatcaConsts.DefaultCountryCode;

    public bool IsDefault { get; set; }
}
