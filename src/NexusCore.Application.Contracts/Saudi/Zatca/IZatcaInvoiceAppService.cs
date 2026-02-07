using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Application service interface for ZATCA Invoice management
/// </summary>
public interface IZatcaInvoiceAppService : IApplicationService
{
    /// <summary>
    /// Get a paginated list of invoices with filtering and sorting
    /// </summary>
    Task<PagedResultDto<ZatcaInvoiceListDto>> GetListAsync(GetZatcaInvoiceListInput input);

    /// <summary>
    /// Get a single invoice by ID with full details including lines
    /// </summary>
    Task<ZatcaInvoiceDto> GetAsync(Guid id);

    /// <summary>
    /// Create a new invoice with lines
    /// </summary>
    Task<ZatcaInvoiceDto> CreateAsync(CreateUpdateZatcaInvoiceDto input);

    /// <summary>
    /// Update an existing invoice (only if not yet submitted)
    /// </summary>
    Task<ZatcaInvoiceDto> UpdateAsync(Guid id, CreateUpdateZatcaInvoiceDto input);

    /// <summary>
    /// Delete an invoice (only if not yet submitted)
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Submit an invoice to ZATCA for validation and acceptance
    /// </summary>
    Task<ZatcaSubmitResultDto> SubmitAsync(Guid id);

    /// <summary>
    /// Validate an invoice locally before submission (without sending to ZATCA)
    /// </summary>
    Task<ZatcaSubmitResultDto> ValidateAsync(Guid id);
}
