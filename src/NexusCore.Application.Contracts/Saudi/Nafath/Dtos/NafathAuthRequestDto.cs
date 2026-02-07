using System;

namespace NexusCore.Saudi.Nafath;

/// <summary>
/// DTO for Nafath authentication request
/// </summary>
public class NafathAuthRequestDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Transaction ID used to track the authentication request
    /// </summary>
    public string TransactionId { get; set; } = null!;

    /// <summary>
    /// National ID being authenticated
    /// </summary>
    public string NationalId { get; set; } = null!;

    /// <summary>
    /// Random number (0-99) displayed to user for verification
    /// </summary>
    public int RandomNumber { get; set; }

    /// <summary>
    /// Current status of the authentication request
    /// </summary>
    public NafathRequestStatus Status { get; set; }

    /// <summary>
    /// When the authentication request was created
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// When the authentication request expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the authentication request was completed (if applicable)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// The user ID if authentication was successful
    /// </summary>
    public Guid? UserId { get; set; }
}
