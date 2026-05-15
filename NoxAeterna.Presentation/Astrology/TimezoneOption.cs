namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents one selectable TZDB timezone option.
/// </summary>
public sealed record TimezoneOption(string TimezoneId)
{
    /// <inheritdoc />
    public override string ToString() => TimezoneId;
}
