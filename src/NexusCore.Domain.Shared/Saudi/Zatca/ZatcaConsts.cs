namespace NexusCore.Saudi.Zatca;

public static class ZatcaConsts
{
    public const int MaxInvoiceNumberLength = 128;
    public const int MaxVatRegistrationNumberLength = 15;
    public const int MaxCommercialRegistrationNumberLength = 20;
    public const int MaxItemNameLength = 512;
    public const int MaxTaxCategoryCodeLength = 10;
    public const int MaxSellerNameLength = 256;
    public const int MaxBuyerNameLength = 256;
    public const int MaxSerialNumberLength = 256;
    public const int MaxAddressFieldLength = 256;
    public const int MaxPostalCodeLength = 10;
    public const int MaxCountryCodeLength = 3;
    public const int MaxCurrencyCodeLength = 3;

    public const string SandboxBaseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal";
    public const string SimulationBaseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation";
    public const string ProductionBaseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/core";

    public const decimal DefaultVatRate = 15.00m;
    public const string DefaultCurrencyCode = "SAR";
    public const string DefaultCountryCode = "SA";
}
