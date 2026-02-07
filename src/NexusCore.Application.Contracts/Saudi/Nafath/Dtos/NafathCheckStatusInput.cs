using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.Nafath;

/// <summary>
/// Input for checking Nafath authentication status
/// </summary>
public class NafathCheckStatusInput
{
    /// <summary>
    /// Transaction ID to check status for
    /// </summary>
    [Required]
    [StringLength(NafathConsts.MaxTransactionIdLength)]
    public string TransactionId { get; set; } = null!;
}
