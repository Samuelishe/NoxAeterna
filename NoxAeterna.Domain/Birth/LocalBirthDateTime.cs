using NodaTime;

namespace NoxAeterna.Domain.Birth;

/// <summary>
/// Represents the user-provided local birth date and optional local time before timezone resolution.
/// </summary>
public readonly record struct LocalBirthDateTime
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalBirthDateTime"/> struct.
    /// </summary>
    /// <param name="date">The local birth date.</param>
    /// <param name="time">The optional local birth time.</param>
    public LocalBirthDateTime(LocalDate date, LocalTime? time = null)
    {
        Date = date;
        Time = time;
    }

    /// <summary>
    /// Gets the local birth date.
    /// </summary>
    public LocalDate Date { get; }

    /// <summary>
    /// Gets the optional local birth time.
    /// </summary>
    public LocalTime? Time { get; }

    /// <summary>
    /// Gets a value indicating whether an explicit local birth time is available.
    /// </summary>
    public bool HasKnownTime => Time.HasValue;

    /// <summary>
    /// Converts the value to a resolved <see cref="LocalDateTime"/> when a time is known.
    /// </summary>
    /// <returns>The local birth date and time.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the local birth time is unknown.</exception>
    public LocalDateTime ToLocalDateTime()
    {
        if (Time is null)
        {
            throw new InvalidOperationException("A LocalDateTime cannot be created when the birth time is unknown.");
        }

        return Date.At(Time.Value);
    }
}
