using System;

namespace NexusCore.Saudi.Nafath;

/// <summary>
/// DTO for Nafath user identity link
/// </summary>
public class NafathUserLinkDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Linked national ID
    /// </summary>
    public string NationalId { get; set; } = null!;

    /// <summary>
    /// When the link was verified
    /// </summary>
    public DateTime VerifiedAt { get; set; }

    /// <summary>
    /// Whether the link is currently active
    /// </summary>
    public bool IsActive { get; set; }
}
