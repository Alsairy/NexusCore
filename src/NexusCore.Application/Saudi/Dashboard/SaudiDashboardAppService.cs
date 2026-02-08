using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NexusCore.Saudi.Nafath;
using NexusCore.Saudi.Workflows;
using NexusCore.Saudi.Zatca;
using Volo.Abp.Domain.Repositories;

namespace NexusCore.Saudi.Dashboard;

[Authorize]
public class SaudiDashboardAppService : NexusCoreAppService, ISaudiDashboardAppService
{
    private readonly IRepository<ZatcaInvoice, Guid> _invoiceRepository;
    private readonly IRepository<NafathAuthRequest, Guid> _nafathAuthRepository;
    private readonly IRepository<NafathUserLink, Guid> _nafathUserLinkRepository;
    private readonly IRepository<ApprovalTask, Guid> _approvalTaskRepository;

    public SaudiDashboardAppService(
        IRepository<ZatcaInvoice, Guid> invoiceRepository,
        IRepository<NafathAuthRequest, Guid> nafathAuthRepository,
        IRepository<NafathUserLink, Guid> nafathUserLinkRepository,
        IRepository<ApprovalTask, Guid> approvalTaskRepository)
    {
        _invoiceRepository = invoiceRepository;
        _nafathAuthRepository = nafathAuthRepository;
        _nafathUserLinkRepository = nafathUserLinkRepository;
        _approvalTaskRepository = approvalTaskRepository;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var dto = new DashboardDto();

        // Invoice statistics
        var invoices = await _invoiceRepository.GetListAsync();

        dto.TotalInvoices = invoices.Count;
        dto.DraftInvoices = invoices.Count(i => i.Status == ZatcaInvoiceStatus.Draft);
        dto.SubmittedInvoices = invoices.Count(i =>
            i.Status == ZatcaInvoiceStatus.Reported || i.Status == ZatcaInvoiceStatus.Cleared);
        dto.RejectedInvoices = invoices.Count(i => i.Status == ZatcaInvoiceStatus.Rejected);

        var clearedInvoices = invoices.Where(i => i.Status == ZatcaInvoiceStatus.Cleared).ToList();
        dto.TotalRevenue = clearedInvoices.Sum(i => i.GrandTotal);
        dto.TotalVat = clearedInvoices.Sum(i => i.VatAmount);

        // Last ZATCA submission
        var lastSubmitted = invoices
            .Where(i => i.Status != ZatcaInvoiceStatus.Draft)
            .OrderByDescending(i => i.LastModificationTime ?? i.CreationTime)
            .FirstOrDefault();
        dto.LastZatcaSubmission = lastSubmitted?.LastModificationTime ?? lastSubmitted?.CreationTime;

        // Monthly stats (last 12 months)
        dto.MonthlyStats = BuildMonthlyStats(invoices);

        // Nafath statistics
        var nafathAuths = await _nafathAuthRepository.GetListAsync();
        dto.TotalNafathAuths = nafathAuths.Count;
        dto.CompletedNafathAuths = nafathAuths.Count(a => a.Status == NafathRequestStatus.Completed);
        dto.ExpiredNafathAuths = nafathAuths.Count(a => a.Status == NafathRequestStatus.Expired);
        dto.LinkedUsers = await _nafathUserLinkRepository.CountAsync(l => l.IsActive);

        // Approval statistics
        var approvalTasks = await _approvalTaskRepository.GetListAsync();
        dto.PendingApprovals = approvalTasks.Count(t => t.Status == ApprovalStatus.Pending);

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        dto.ApprovedThisMonth = approvalTasks.Count(t =>
            t.Status == ApprovalStatus.Approved && t.CompletedAt >= startOfMonth);
        dto.RejectedThisMonth = approvalTasks.Count(t =>
            t.Status == ApprovalStatus.Rejected && t.CompletedAt >= startOfMonth);

        // Health status — simple check based on recent data
        dto.ZatcaApiStatus = dto.LastZatcaSubmission.HasValue &&
            dto.LastZatcaSubmission.Value > DateTime.UtcNow.AddDays(-7) ? "Healthy" : "Unknown";
        dto.NafathApiStatus = nafathAuths.Any(a =>
            a.RequestedAt > DateTime.UtcNow.AddDays(-7)) ? "Healthy" : "Unknown";

        return dto;
    }

    public async Task<List<MonthlyInvoiceStatsDto>> GetMonthlyStatsAsync(int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var invoices = await _invoiceRepository.GetListAsync(i => i.IssueDate.Year == targetYear);
        return BuildMonthlyStatsForYear(invoices, targetYear);
    }

    public async Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 10)
    {
        var activities = new List<RecentActivityDto>();

        // Recent invoice activities
        var recentInvoices = (await _invoiceRepository.GetListAsync())
            .Where(i => i.Status != ZatcaInvoiceStatus.Draft)
            .OrderByDescending(i => i.LastModificationTime ?? i.CreationTime)
            .Take(count)
            .ToList();

        foreach (var invoice in recentInvoices)
        {
            var timestamp = invoice.LastModificationTime ?? invoice.CreationTime;
            var statusText = invoice.Status switch
            {
                ZatcaInvoiceStatus.Validated => "validated",
                ZatcaInvoiceStatus.Reported => "reported to ZATCA",
                ZatcaInvoiceStatus.Cleared => "cleared by ZATCA",
                ZatcaInvoiceStatus.Rejected => "rejected by ZATCA",
                ZatcaInvoiceStatus.Archived => "archived",
                _ => "updated"
            };

            activities.Add(new RecentActivityDto
            {
                Timestamp = timestamp,
                ActivityType = "Invoice",
                Description = $"Invoice #{invoice.InvoiceNumber} {statusText}",
                EntityId = invoice.Id.ToString(),
                EntityType = "ZatcaInvoice"
            });
        }

        // Recent approval activities
        var recentApprovals = (await _approvalTaskRepository.GetListAsync())
            .Where(t => t.Status != ApprovalStatus.Pending)
            .OrderByDescending(t => t.CompletedAt ?? t.CreationTime)
            .Take(count)
            .ToList();

        foreach (var task in recentApprovals)
        {
            var timestamp = task.CompletedAt ?? task.CreationTime;
            var statusText = task.Status switch
            {
                ApprovalStatus.Approved => "approved",
                ApprovalStatus.Rejected => "rejected",
                ApprovalStatus.Escalated => "escalated",
                ApprovalStatus.Delegated => "delegated",
                _ => "updated"
            };

            activities.Add(new RecentActivityDto
            {
                Timestamp = timestamp,
                ActivityType = "Approval",
                Description = $"{task.TaskName} {statusText}",
                EntityId = task.Id.ToString(),
                EntityType = "ApprovalTask"
            });
        }

        // Recent Nafath completions
        var recentNafath = (await _nafathAuthRepository.GetListAsync())
            .Where(a => a.Status == NafathRequestStatus.Completed)
            .OrderByDescending(a => a.CompletedAt ?? a.CreationTime)
            .Take(count)
            .ToList();

        foreach (var auth in recentNafath)
        {
            activities.Add(new RecentActivityDto
            {
                Timestamp = auth.CompletedAt ?? auth.CreationTime,
                ActivityType = "Nafath",
                Description = "Nafath authentication completed",
                EntityId = auth.Id.ToString(),
                EntityType = "NafathAuthRequest"
            });
        }

        return activities
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToList();
    }

    private static List<MonthlyInvoiceStatsDto> BuildMonthlyStats(List<ZatcaInvoice> invoices)
    {
        var now = DateTime.UtcNow;
        var stats = new List<MonthlyInvoiceStatsDto>();

        for (int i = 11; i >= 0; i--)
        {
            var monthDate = now.AddMonths(-i);
            var year = monthDate.Year;
            var month = monthDate.Month;
            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month);

            var monthInvoices = invoices
                .Where(inv => inv.IssueDate.Year == year && inv.IssueDate.Month == month)
                .ToList();

            var cleared = monthInvoices.Where(inv => inv.Status == ZatcaInvoiceStatus.Cleared).ToList();

            stats.Add(new MonthlyInvoiceStatsDto
            {
                Year = year,
                Month = month,
                MonthName = monthName,
                InvoiceCount = monthInvoices.Count,
                Revenue = cleared.Sum(inv => inv.GrandTotal),
                Vat = cleared.Sum(inv => inv.VatAmount)
            });
        }

        return stats;
    }

    private static List<MonthlyInvoiceStatsDto> BuildMonthlyStatsForYear(List<ZatcaInvoice> invoices, int year)
    {
        var stats = new List<MonthlyInvoiceStatsDto>();

        for (int month = 1; month <= 12; month++)
        {
            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month);

            var monthInvoices = invoices
                .Where(inv => inv.IssueDate.Month == month)
                .ToList();

            var cleared = monthInvoices.Where(inv => inv.Status == ZatcaInvoiceStatus.Cleared).ToList();

            stats.Add(new MonthlyInvoiceStatsDto
            {
                Year = year,
                Month = month,
                MonthName = monthName,
                InvoiceCount = monthInvoices.Count,
                Revenue = cleared.Sum(inv => inv.GrandTotal),
                Vat = cleared.Sum(inv => inv.VatAmount)
            });
        }

        return stats;
    }
}
