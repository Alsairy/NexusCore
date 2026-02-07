using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.Settings;

public class ZatcaSettingsDto
{
    [Required]
    public string Environment { get; set; } = "Sandbox";

    [Required]
    public string ApiBaseUrl { get; set; } = string.Empty;

    public string? ComplianceCsid { get; set; }

    public string? ProductionCsid { get; set; }

    public string? Secret { get; set; }
}
