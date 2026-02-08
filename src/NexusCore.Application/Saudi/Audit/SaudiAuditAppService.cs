using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AuditLogging;
using Volo.Abp.Domain.Repositories;

namespace NexusCore.Saudi.Audit;

[Authorize]
public class SaudiAuditAppService : NexusCoreAppService, ISaudiAuditAppService
{
    private readonly IRepository<AuditLog, Guid> _auditLogRepository;

    // Saudi-related service name prefixes for filtering
    private static readonly string[] SaudiServicePrefixes = new[]
    {
        "NexusCore.Saudi.",
        "NexusCore.Saudi.Zatca.",
        "NexusCore.Saudi.Nafath.",
        "NexusCore.Saudi.Onboarding.",
        "NexusCore.Saudi.Dashboard.",
        "NexusCore.Saudi.Workflows.",
        "NexusCore.Saudi.Audit.",
    };

    // Saudi-related URL path prefixes
    private static readonly string[] SaudiUrlPrefixes = new[]
    {
        "/api/app/zatca",
        "/api/app/nafath",
        "/api/app/onboarding",
        "/api/app/saudi-",
        "/api/app/approval",
        "/api/app/delegation",
        "/api/saudi/",
    };

    // Module-to-prefix mapping
    private static readonly Dictionary<string, string[]> ModuleFilters = new()
    {
        ["Zatca"] = new[] { "Zatca", "/api/app/zatca" },
        ["Nafath"] = new[] { "Nafath", "/api/app/nafath" },
        ["Workflow"] = new[] { "Workflow", "Approval", "Delegation", "/api/app/approval", "/api/app/delegation" },
    };

    public SaudiAuditAppService(IRepository<AuditLog, Guid> auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResultDto<SaudiAuditLogDto>> GetListAsync(GetSaudiAuditListInput input)
    {
        var queryable = await _auditLogRepository.GetQueryableAsync();

        // Filter to Saudi-related entries by URL pattern
        queryable = queryable.Where(log =>
            log.Url != null && (
                log.Url.Contains("/api/app/zatca") ||
                log.Url.Contains("/api/app/nafath") ||
                log.Url.Contains("/api/app/onboarding") ||
                log.Url.Contains("/api/app/saudi-") ||
                log.Url.Contains("/api/app/approval") ||
                log.Url.Contains("/api/app/delegation")
            ));

        // Apply filters
        if (input.StartDate.HasValue)
        {
            queryable = queryable.Where(log => log.ExecutionTime >= input.StartDate.Value);
        }

        if (input.EndDate.HasValue)
        {
            var endDateExclusive = input.EndDate.Value.Date.AddDays(1);
            queryable = queryable.Where(log => log.ExecutionTime < endDateExclusive);
        }

        if (input.UserId.HasValue)
        {
            queryable = queryable.Where(log => log.UserId == input.UserId.Value);
        }

        if (!string.IsNullOrEmpty(input.Module) && ModuleFilters.TryGetValue(input.Module, out var moduleKeywords))
        {
            var keywords = moduleKeywords;
            queryable = queryable.Where(log =>
                log.Url != null && keywords.Any(k => log.Url.Contains(k)));
        }

        if (input.MinDuration.HasValue)
        {
            queryable = queryable.Where(log => log.ExecutionDuration >= input.MinDuration.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var sorting = !string.IsNullOrEmpty(input.Sorting)
            ? input.Sorting
            : "ExecutionTime DESC";

        queryable = queryable.OrderBy(sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var auditLogs = await AsyncExecuter.ToListAsync(queryable);

        var dtos = auditLogs.Select(MapToDto).ToList();

        // Apply entity-level filters in memory (entity changes are navigations)
        if (!string.IsNullOrEmpty(input.EntityType) || !string.IsNullOrEmpty(input.EntityId))
        {
            // Need to load with entity changes for this filter
            // For now, filter what we can from the DTO
        }

        return new PagedResultDto<SaudiAuditLogDto>(totalCount, dtos);
    }

    public async Task<SaudiAuditLogDto> GetAsync(Guid id)
    {
        var auditLog = await _auditLogRepository.GetAsync(id);
        return MapToDto(auditLog);
    }

    public async Task<List<SaudiEntityChangeDto>> GetEntityHistoryAsync(string entityType, string entityId)
    {
        var queryable = await _auditLogRepository.GetQueryableAsync();

        // Get audit logs that have entity changes matching the given entity
        var auditLogs = await AsyncExecuter.ToListAsync(
            queryable.Where(log => log.EntityChanges.Any(
                ec => ec.EntityTypeFullName != null &&
                      ec.EntityTypeFullName.Contains(entityType) &&
                      ec.EntityId == entityId))
            .OrderByDescending(log => log.ExecutionTime)
            .Take(100));

        var result = new List<SaudiEntityChangeDto>();

        foreach (var log in auditLogs)
        {
            var matchingChanges = log.EntityChanges
                .Where(ec => ec.EntityTypeFullName != null &&
                             ec.EntityTypeFullName.Contains(entityType) &&
                             ec.EntityId == entityId);

            foreach (var change in matchingChanges)
            {
                result.Add(MapEntityChangeToDto(change));
            }
        }

        return result.OrderByDescending(c => c.ChangeTime).ToList();
    }

    private static SaudiAuditLogDto MapToDto(AuditLog auditLog)
    {
        return new SaudiAuditLogDto
        {
            Id = auditLog.Id,
            TenantId = auditLog.TenantId,
            UserId = auditLog.UserId,
            UserName = auditLog.UserName,
            ServiceName = SimplifyServiceName(auditLog.Actions.FirstOrDefault()?.ServiceName),
            MethodName = auditLog.Actions.FirstOrDefault()?.MethodName,
            ExecutionTime = auditLog.ExecutionTime,
            ExecutionDuration = auditLog.ExecutionDuration,
            HttpMethod = auditLog.HttpMethod,
            Url = auditLog.Url,
            HttpStatusCode = auditLog.HttpStatusCode,
            ClientIpAddress = auditLog.ClientIpAddress,
            BrowserInfo = auditLog.BrowserInfo,
            HasException = auditLog.Exceptions?.Length > 0,
            ExceptionMessage = TruncateException(auditLog.Exceptions),
            Actions = auditLog.Actions.Select(a => new SaudiAuditLogActionDto
            {
                ServiceName = SimplifyServiceName(a.ServiceName),
                MethodName = a.MethodName,
                Parameters = a.Parameters,
                ExecutionTime = a.ExecutionTime,
                ExecutionDuration = a.ExecutionDuration,
            }).ToList(),
            EntityChanges = auditLog.EntityChanges.Select(MapEntityChangeToDto).ToList(),
        };
    }

    private static SaudiEntityChangeDto MapEntityChangeToDto(EntityChange change)
    {
        return new SaudiEntityChangeDto
        {
            Id = change.Id,
            EntityTypeFullName = SimplifyEntityType(change.EntityTypeFullName),
            EntityId = change.EntityId,
            ChangeType = change.ChangeType.ToString(),
            ChangeTime = change.ChangeTime,
            PropertyChanges = change.PropertyChanges.Select(p => new SaudiPropertyChangeDto
            {
                PropertyName = p.PropertyName,
                OriginalValue = p.OriginalValue,
                NewValue = p.NewValue,
            }).ToList(),
        };
    }

    private static string? SimplifyServiceName(string? fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return null;
        var lastDot = fullName.LastIndexOf('.');
        return lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;
    }

    private static string? SimplifyEntityType(string? fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return null;
        var lastDot = fullName.LastIndexOf('.');
        return lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;
    }

    private static string? TruncateException(string? exception)
    {
        if (string.IsNullOrEmpty(exception)) return null;
        return exception.Length > 500 ? exception[..500] + "..." : exception;
    }
}
