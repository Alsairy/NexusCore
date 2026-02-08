using System;
using System.Collections.Generic;

namespace NexusCore.Saudi.Dashboard;

public class DashboardDto
{
    // Invoice Statistics
    public int TotalInvoices { get; set; }
    public int DraftInvoices { get; set; }
    public int SubmittedInvoices { get; set; }
    public int RejectedInvoices { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalVat { get; set; }

    // Monthly Breakdown (last 12 months)
    public List<MonthlyInvoiceStatsDto> MonthlyStats { get; set; } = new();

    // Nafath Statistics
    public int TotalNafathAuths { get; set; }
    public int CompletedNafathAuths { get; set; }
    public int ExpiredNafathAuths { get; set; }
    public int LinkedUsers { get; set; }

    // Approval Statistics
    public int PendingApprovals { get; set; }
    public int ApprovedThisMonth { get; set; }
    public int RejectedThisMonth { get; set; }

    // System Health
    public string ZatcaApiStatus { get; set; } = "Unknown";
    public string NafathApiStatus { get; set; } = "Unknown";
    public DateTime? LastZatcaSubmission { get; set; }
}

public class MonthlyInvoiceStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Vat { get; set; }
}

public class RecentActivityDto
{
    public DateTime Timestamp { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
}
