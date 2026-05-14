using NodaTime;

namespace NoxAeterna.Domain.Birth;

/// <summary>
/// Represents a resolved birth time ready for astronomy-facing calculations.
/// </summary>
public readonly record struct BirthMoment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BirthMoment"/> struct.
    /// </summary>
    /// <param name="originalLocalDateTime">The original local birth date and time.</param>
    /// <param name="timezoneId">The explicit timezone identifier used for resolution.</param>
    /// <param name="instant">The resolved UTC instant.</param>
    /// <param name="resolutionStatus">The way ambiguity or invalid local time was handled.</param>
    /// <param name="birthTimeAccuracy">The declared reliability of the birth time.</param>
    /// <param name="sourceNote">Optional source or confidence note carried from the original birth data.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="birthTimeAccuracy"/> is <see cref="Birth.BirthTimeAccuracy.UnknownTime"/>.</exception>
    public BirthMoment(
        LocalDateTime originalLocalDateTime,
        TimezoneId timezoneId,
        Instant instant,
        TimeResolutionStatus resolutionStatus,
        BirthTimeAccuracy birthTimeAccuracy,
        string? sourceNote = null)
    {
        if (birthTimeAccuracy == BirthTimeAccuracy.UnknownTime)
        {
            throw new ArgumentException("BirthMoment requires a known birth time.", nameof(birthTimeAccuracy));
        }

        OriginalLocalDateTime = originalLocalDateTime;
        TimezoneId = timezoneId;
        Instant = instant;
        ResolutionStatus = resolutionStatus;
        BirthTimeAccuracy = birthTimeAccuracy;
        SourceNote = string.IsNullOrWhiteSpace(sourceNote) ? null : sourceNote.Trim();
    }

    /// <summary>
    /// Gets the original local birth date and time.
    /// </summary>
    public LocalDateTime OriginalLocalDateTime { get; }

    /// <summary>
    /// Gets the explicit timezone identifier used for resolution.
    /// </summary>
    public TimezoneId TimezoneId { get; }

    /// <summary>
    /// Gets the resolved UTC instant.
    /// </summary>
    public Instant Instant { get; }

    /// <summary>
    /// Gets the strategy result describing how ambiguity or invalid local time was handled.
    /// </summary>
    public TimeResolutionStatus ResolutionStatus { get; }

    /// <summary>
    /// Gets the declared reliability of the birth time.
    /// </summary>
    public BirthTimeAccuracy BirthTimeAccuracy { get; }

    /// <summary>
    /// Gets the optional source or confidence note.
    /// </summary>
    public string? SourceNote { get; }
}
