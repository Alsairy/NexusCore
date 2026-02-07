using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.Nafath;

/// <summary>
/// Input for initiating Nafath login
/// </summary>
public class NafathInitiateLoginInput
{
    /// <summary>
    /// National ID to authenticate (10 digits)
    /// </summary>
    [Required]
    [StringLength(NafathConsts.NationalIdLength, MinimumLength = NafathConsts.NationalIdLength)]
    public string NationalId { get; set; } = null!;
}
