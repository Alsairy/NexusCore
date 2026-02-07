using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.Settings;

public class NafathSettingsDto
{
    public string? AppId { get; set; }

    public string? AppKey { get; set; }

    [Required]
    public string ApiBaseUrl { get; set; } = string.Empty;

    public string? CallbackUrl { get; set; }

    [Range(30, 600)]
    public int TimeoutSeconds { get; set; } = 120;
}
