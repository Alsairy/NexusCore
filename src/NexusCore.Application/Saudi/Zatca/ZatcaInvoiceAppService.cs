using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NexusCore.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Application service for ZATCA Invoice management
/// </summary>
[Authorize(NexusCorePermissions.Zatca.Invoices)]
public class ZatcaInvoiceAppService : NexusCoreAppService, IZatcaInvoiceAppService
{
    private readonly IRepository<ZatcaInvoice, Guid> _invoiceRepository;
    private readonly IRepository<ZatcaInvoiceLine, Guid> _invoiceLineRepository;
    private readonly IRepository<ZatcaSeller, Guid> _sellerRepository;
    private readonly IRepository<ZatcaCertificate, Guid> _certificateRepository;
    private readonly ZatcaInvoiceManager _invoiceManager;
    private readonly ZatcaApiClient _apiClient;

    public ZatcaInvoiceAppService(
        IRepository<ZatcaInvoice, Guid> invoiceRepository,
        IRepository<ZatcaInvoiceLine, Guid> invoiceLineRepository,
        IRepository<ZatcaSeller, Guid> sellerRepository,
        IRepository<ZatcaCertificate, Guid> certificateRepository,
        ZatcaInvoiceManager invoiceManager,
        ZatcaApiClient apiClient)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceLineRepository = invoiceLineRepository;
        _sellerRepository = sellerRepository;
        _certificateRepository = certificateRepository;
        _invoiceManager = invoiceManager;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Get a paginated list of invoices with filtering and sorting
    /// </summary>
    public async Task<PagedResultDto<ZatcaInvoiceListDto>> GetListAsync(GetZatcaInvoiceListInput input)
    {
        var queryable = await _invoiceRepository.GetQueryableAsync();

        // Apply filters
        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        if (input.InvoiceType.HasValue)
        {
            queryable = queryable.Where(x => x.InvoiceType == input.InvoiceType.Value);
        }

        if (input.DateFrom.HasValue)
        {
            queryable = queryable.Where(x => x.IssueDate >= input.DateFrom.Value);
        }

        if (input.DateTo.HasValue)
        {
            queryable = queryable.Where(x => x.IssueDate <= input.DateTo.Value);
        }

        if (input.SellerId.HasValue)
        {
            queryable = queryable.Where(x => x.SellerId == input.SellerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.InvoiceNumber.Contains(input.Filter) ||
                (x.BuyerName != null && x.BuyerName.Contains(input.Filter)));
        }

        // Get total count
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderByDescending(x => x.CreationTime);
        }

        // Apply paging
        queryable = queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var invoices = await AsyncExecuter.ToListAsync(queryable);

        // Map to DTOs
        var dtos = invoices.Select(MapToListDto).ToList();

        return new PagedResultDto<ZatcaInvoiceListDto>(totalCount, dtos);
    }

    /// <summary>
    /// Get a single invoice by ID with full details including lines
    /// </summary>
    public async Task<ZatcaInvoiceDto> GetAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetAsync(id);
        var lines = await _invoiceLineRepository.GetListAsync(x => x.InvoiceId == id);

        return MapToDto(invoice, lines);
    }

    /// <summary>
    /// Create a new invoice with lines
    /// </summary>
    [Authorize(NexusCorePermissions.Zatca.InvoicesCreate)]
    public async Task<ZatcaInvoiceDto> CreateAsync(CreateUpdateZatcaInvoiceDto input)
    {
        // Verify seller exists
        await _sellerRepository.GetAsync(input.SellerId);

        // Create invoice entity
        var invoice = new ZatcaInvoice(
            GuidGenerator.Create(),
            input.SellerId,
            input.InvoiceNumber,
            input.InvoiceType,
            input.IssueDate)
        {
            IssueDateHijri = input.IssueDateHijri,
            BuyerName = input.BuyerName,
            BuyerVatNumber = input.BuyerVatNumber,
            CurrencyCode = input.CurrencyCode
        };

        await _invoiceRepository.InsertAsync(invoice);

        // Create invoice lines
        var lines = new List<ZatcaInvoiceLine>();
        foreach (var lineDto in input.Lines)
        {
            var line = new ZatcaInvoiceLine(
                GuidGenerator.Create(),
                invoice.Id,
                lineDto.ItemName,
                lineDto.Quantity,
                lineDto.UnitPrice)
            {
                TaxCategoryCode = lineDto.TaxCategoryCode,
                TaxPercent = lineDto.TaxPercent
            };

            // CalculateAmounts is called in the constructor, but recalculate
            // in case TaxPercent was changed from the default after construction
            line.CalculateAmounts();

            lines.Add(line);
            await _invoiceLineRepository.InsertAsync(line);
        }

        // Recalculate invoice totals from lines
        RecalculateInvoiceTotals(invoice, lines);
        await _invoiceRepository.UpdateAsync(invoice);

        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToDto(invoice, lines);
    }

    /// <summary>
    /// Update an existing invoice (only if not yet submitted)
    /// </summary>
    [Authorize(NexusCorePermissions.Zatca.InvoicesEdit)]
    public async Task<ZatcaInvoiceDto> UpdateAsync(Guid id, CreateUpdateZatcaInvoiceDto input)
    {
        var invoice = await _invoiceRepository.GetAsync(id);

        // Can only update draft invoices
        if (invoice.Status != ZatcaInvoiceStatus.Draft)
        {
            throw new UserFriendlyException("Cannot update an invoice that has been submitted to ZATCA");
        }

        // Verify seller exists
        await _sellerRepository.GetAsync(input.SellerId);

        // Update invoice properties
        invoice.SellerId = input.SellerId;
        invoice.InvoiceNumber = input.InvoiceNumber;
        invoice.InvoiceType = input.InvoiceType;
        invoice.IssueDate = input.IssueDate;
        invoice.IssueDateHijri = input.IssueDateHijri;
        invoice.BuyerName = input.BuyerName;
        invoice.BuyerVatNumber = input.BuyerVatNumber;
        invoice.CurrencyCode = input.CurrencyCode;

        // Delete existing lines
        var existingLines = await _invoiceLineRepository.GetListAsync(x => x.InvoiceId == id);
        await _invoiceLineRepository.DeleteManyAsync(existingLines);

        // Create new lines
        var lines = new List<ZatcaInvoiceLine>();
        foreach (var lineDto in input.Lines)
        {
            var line = new ZatcaInvoiceLine(
                GuidGenerator.Create(),
                invoice.Id,
                lineDto.ItemName,
                lineDto.Quantity,
                lineDto.UnitPrice)
            {
                TaxCategoryCode = lineDto.TaxCategoryCode,
                TaxPercent = lineDto.TaxPercent
            };

            line.CalculateAmounts();

            lines.Add(line);
            await _invoiceLineRepository.InsertAsync(line);
        }

        // Recalculate invoice totals from lines
        RecalculateInvoiceTotals(invoice, lines);
        await _invoiceRepository.UpdateAsync(invoice);

        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToDto(invoice, lines);
    }

    /// <summary>
    /// Delete an invoice (only if not yet submitted)
    /// </summary>
    [Authorize(NexusCorePermissions.Zatca.InvoicesDelete)]
    public async Task DeleteAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetAsync(id);

        // Can only delete draft invoices
        if (invoice.Status != ZatcaInvoiceStatus.Draft)
        {
            throw new UserFriendlyException("Cannot delete an invoice that has been submitted to ZATCA");
        }

        // Delete lines first
        var lines = await _invoiceLineRepository.GetListAsync(x => x.InvoiceId == id);
        await _invoiceLineRepository.DeleteManyAsync(lines);

        // Delete invoice
        await _invoiceRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Submit an invoice to ZATCA for validation and acceptance.
    /// Uses the domain ZatcaInvoiceManager to prepare (validate, generate QR, build XML, sign, hash),
    /// then submits to ZATCA via the API client.
    /// </summary>
    [Authorize(NexusCorePermissions.Zatca.InvoicesSubmit)]
    public async Task<ZatcaSubmitResultDto> SubmitAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetAsync(id);

        // Validate invoice can be submitted
        if (invoice.Status != ZatcaInvoiceStatus.Draft && invoice.Status != ZatcaInvoiceStatus.Validated)
        {
            throw new UserFriendlyException("Only draft or validated invoices can be submitted");
        }

        // Load related data
        var lines = await _invoiceLineRepository.GetListAsync(x => x.InvoiceId == id);
        var seller = await _sellerRepository.GetAsync(invoice.SellerId);

        // Attach seller and lines to the invoice entity for the domain manager
        invoice.Seller = seller;
        invoice.Lines = lines;

        // Find the active certificate for this seller
        var certificateQueryable = await _certificateRepository.GetQueryableAsync();
        var certificate = await AsyncExecuter.FirstOrDefaultAsync(
            certificateQueryable.Where(c => c.SellerId == invoice.SellerId && c.IsActive));

        if (certificate == null)
        {
            throw new UserFriendlyException(
                "No active ZATCA certificate found for this seller. Please configure a certificate first.");
        }

        // Use the domain manager to prepare (validate, QR, XML, sign, hash)
        invoice = await _invoiceManager.PrepareInvoiceForSubmissionAsync(invoice, certificate);

        // Submit to ZATCA API based on invoice type
        var isSimplified = invoice.InvoiceType == ZatcaInvoiceType.Simplified
                           || invoice.InvoiceType == ZatcaInvoiceType.SimplifiedDebitNote
                           || invoice.InvoiceType == ZatcaInvoiceType.SimplifiedCreditNote;

        var result = isSimplified
            ? await _apiClient.ReportInvoiceAsync(invoice.XmlContent!, invoice.Id.ToString())
            : await _apiClient.ClearInvoiceAsync(invoice.XmlContent!, invoice.Id.ToString());

        // Update invoice with submission results
        invoice.Status = result.Status;
        invoice.ZatcaRequestId = result.RequestId;
        invoice.QrCode = result.QrCode ?? invoice.QrCode;
        invoice.ZatcaWarnings = result.Warnings.Any() ? string.Join("; ", result.Warnings) : null;
        invoice.ZatcaErrors = result.Errors.Any() ? string.Join("; ", result.Errors) : null;

        await _invoiceRepository.UpdateAsync(invoice);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return result;
    }

    /// <summary>
    /// Validate an invoice locally before submission (without sending to ZATCA).
    /// Uses the domain ZatcaInvoiceManager.ValidateInvoice method.
    /// </summary>
    public async Task<ZatcaSubmitResultDto> ValidateAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetAsync(id);
        var lines = await _invoiceLineRepository.GetListAsync(x => x.InvoiceId == id);
        var seller = await _sellerRepository.GetAsync(invoice.SellerId);

        // Attach seller and lines to the invoice entity for validation
        invoice.Seller = seller;
        invoice.Lines = lines;

        // Validate using domain manager
        var validationResult = _invoiceManager.ValidateInvoice(invoice);

        return new ZatcaSubmitResultDto
        {
            Status = validationResult.IsValid ? ZatcaInvoiceStatus.Draft : ZatcaInvoiceStatus.Rejected,
            Errors = validationResult.Errors,
            RequestId = string.Empty
        };
    }

    #region Private Methods

    /// <summary>
    /// Recalculates invoice totals from line items.
    /// </summary>
    private static void RecalculateInvoiceTotals(ZatcaInvoice invoice, List<ZatcaInvoiceLine> lines)
    {
        invoice.SubTotal = Math.Round(lines.Sum(l => l.NetAmount), 2);
        invoice.VatAmount = Math.Round(lines.Sum(l => l.VatAmount), 2);
        invoice.GrandTotal = Math.Round(lines.Sum(l => l.TotalAmount), 2);
    }

    private static ZatcaInvoiceListDto MapToListDto(ZatcaInvoice invoice)
    {
        return new ZatcaInvoiceListDto
        {
            Id = invoice.Id,
            SellerId = invoice.SellerId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceType = invoice.InvoiceType,
            IssueDate = invoice.IssueDate,
            BuyerName = invoice.BuyerName,
            BuyerVatNumber = invoice.BuyerVatNumber,
            SubTotal = invoice.SubTotal,
            VatAmount = invoice.VatAmount,
            GrandTotal = invoice.GrandTotal,
            Status = invoice.Status,
            ZatcaRequestId = invoice.ZatcaRequestId,
            CreationTime = invoice.CreationTime
        };
    }

    private static ZatcaInvoiceDto MapToDto(ZatcaInvoice invoice, List<ZatcaInvoiceLine> lines)
    {
        return new ZatcaInvoiceDto
        {
            Id = invoice.Id,
            SellerId = invoice.SellerId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceType = invoice.InvoiceType,
            IssueDate = invoice.IssueDate,
            IssueDateHijri = invoice.IssueDateHijri,
            BuyerName = invoice.BuyerName,
            BuyerVatNumber = invoice.BuyerVatNumber,
            CurrencyCode = invoice.CurrencyCode,
            SubTotal = invoice.SubTotal,
            VatAmount = invoice.VatAmount,
            GrandTotal = invoice.GrandTotal,
            QrCode = invoice.QrCode,
            XmlContent = invoice.XmlContent,
            InvoiceHash = invoice.InvoiceHash,
            PreviousInvoiceHash = invoice.PreviousInvoiceHash,
            Status = invoice.Status,
            ZatcaRequestId = invoice.ZatcaRequestId,
            ZatcaWarnings = invoice.ZatcaWarnings,
            ZatcaErrors = invoice.ZatcaErrors,
            CreationTime = invoice.CreationTime,
            Lines = lines.Select(MapLineToDto).ToList()
        };
    }

    private static ZatcaInvoiceLineDto MapLineToDto(ZatcaInvoiceLine line)
    {
        return new ZatcaInvoiceLineDto
        {
            Id = line.Id,
            InvoiceId = line.InvoiceId,
            ItemName = line.ItemName,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            TaxCategoryCode = line.TaxCategoryCode,
            TaxPercent = line.TaxPercent,
            NetAmount = line.NetAmount,
            VatAmount = line.VatAmount,
            TotalAmount = line.TotalAmount
        };
    }

    #endregion
}
