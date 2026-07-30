namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Represents the four principal angles of a natal chart.
/// </summary>
public sealed record ChartAngles
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartAngles"/> class.
    /// Opposite angles are derived so their geometric relationship cannot drift.
    /// </summary>
    public ChartAngles(ZodiacLongitude ascendant, ZodiacLongitude midheaven)
    {
        Ascendant = ascendant;
        Midheaven = midheaven;
        Descendant = new ZodiacLongitude(ascendant.Degrees + 180d);
        ImumCoeli = new ZodiacLongitude(midheaven.Degrees + 180d);
    }

    /// <summary>
    /// Gets the Ascendant longitude.
    /// </summary>
    public ZodiacLongitude Ascendant { get; }

    /// <summary>
    /// Gets the Midheaven longitude.
    /// </summary>
    public ZodiacLongitude Midheaven { get; }

    /// <summary>
    /// Gets the Descendant longitude.
    /// </summary>
    public ZodiacLongitude Descendant { get; }

    /// <summary>
    /// Gets the Imum Coeli longitude.
    /// </summary>
    public ZodiacLongitude ImumCoeli { get; }
}
