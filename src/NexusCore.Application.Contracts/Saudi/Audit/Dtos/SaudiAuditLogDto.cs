using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace NexusCore.Saudi.Audit;

public class SaudiAuditLogDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ServiceName { get; set; }
    public string? MethodName { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? BrowserInfo { get; set; }
    public bool HasException { get; set; }
    public string? ExceptionMessage { get; set; }
    public List<SaudiAuditLogActionDto> Actions { get; set; } = new();
    public List<SaudiEntityChangeDto> EntityChanges { get; set; } = new();
}

public class SaudiAuditLogActionDto
{
    public string? ServiceName { get; set; }
    public string? MethodName { get; set; }
    public string? Parameters { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
}

public class SaudiEntityChangeDto
{
    public Guid Id { get; set; }
    public string? EntityTypeFullName { get; set; }
    public string? EntityId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public DateTime ChangeTime { get; set; }
    public List<SaudiPropertyChangeDto> PropertyChanges { get; set; } = new();
}

public class SaudiPropertyChangeDto
{
    public string? PropertyName { get; set; }
    public string? OriginalValue { get; set; }
    public string? NewValue { get; set; }
}

public class GetSaudiAuditListInput : PagedAndSortedResultRequestDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? UserId { get; set; }
    public string? Module { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public int? MinDuration { get; set; }
}
