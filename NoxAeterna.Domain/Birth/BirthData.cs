namespace NoxAeterna.Domain.Birth;

/// <summary>
/// Represents unresolved user-entered birth data before timezone-aware instant resolution.
/// </summary>
public readonly record struct BirthData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BirthData"/> struct.
    /// </summary>
    /// <param name="localBirthDateTime">The user-provided local birth date and optional time.</param>
    /// <param name="birthTimeAccuracy">The declared reliability of the provided birth time.</param>
    /// <param name="birthLocation">The user-provided birth location.</param>
    /// <param name="timezoneId">The explicit timezone identifier.</param>
    /// <param name="sourceNote">Optional source or confidence note for the entered data.</param>
    /// <exception cref="ArgumentException">Thrown when the birth time accuracy is inconsistent with the presence or absence of a local time.</exception>
    public BirthData(
        LocalBirthDateTime localBirthDateTime,
        BirthTimeAccuracy birthTimeAccuracy,
        BirthLocation birthLocation,
        TimezoneId timezoneId,
        string? sourceNote = null)
    {
        var hasKnownTime = localBirthDateTime.HasKnownTime;

        if (birthTimeAccuracy == BirthTimeAccuracy.UnknownTime && hasKnownTime)
        {
            throw new ArgumentException("UnknownTime cannot be used when a local birth time is provided.", nameof(birthTimeAccuracy));
        }

        if (birthTimeAccuracy != BirthTimeAccuracy.UnknownTime && !hasKnownTime)
        {
            throw new ArgumentException("A known local birth time is required when the birth time accuracy is not UnknownTime.", nameof(localBirthDateTime));
        }

        LocalBirthDateTime = localBirthDateTime;
        BirthTimeAccuracy = birthTimeAccuracy;
        BirthLocation = birthLocation;
        TimezoneId = timezoneId;
        SourceNote = string.IsNullOrWhiteSpace(sourceNote) ? null : sourceNote.Trim();
    }

    /// <summary>
    /// Gets the user-provided local birth date and optional time.
    /// </summary>
    public LocalBirthDateTime LocalBirthDateTime { get; }

    /// <summary>
    /// Gets the declared reliability of the birth time.
    /// </summary>
    public BirthTimeAccuracy BirthTimeAccuracy { get; }

    /// <summary>
    /// Gets the user-provided birth location.
    /// </summary>
    public BirthLocation BirthLocation { get; }

    /// <summary>
    /// Gets the explicit timezone identifier.
    /// </summary>
    public TimezoneId TimezoneId { get; }

    /// <summary>
    /// Gets the optional source or confidence note.
    /// </summary>
    public string? SourceNote { get; }
}
