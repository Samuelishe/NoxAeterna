using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Represents a minimal natal chart snapshot with resolved birth time, planetary positions, and detected aspects.
/// </summary>
public sealed record NatalChart
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NatalChart"/> class.
    /// </summary>
    /// <param name="birthMoment">The resolved birth moment used for calculation.</param>
    /// <param name="positions">The calculated planetary positions.</param>
    /// <param name="aspects">The detected planetary aspects.</param>
    /// <param name="ephemerisSourceVersion">The optional ephemeris source metadata.</param>
    /// <exception cref="ArgumentException">Thrown when duplicate celestial bodies are present or an aspect references a body without a position.</exception>
    public NatalChart(
        BirthMoment birthMoment,
        IEnumerable<PlanetPosition> positions,
        IEnumerable<CalculatedAspect> aspects,
        string? ephemerisSourceVersion = null)
    {
        var copiedPositions = (positions ?? throw new ArgumentNullException(nameof(positions)))
            .OrderBy(static position => position.Body)
            .ToArray();

        var duplicateBody = copiedPositions
            .GroupBy(static position => position.Body)
            .FirstOrDefault(static group => group.Count() > 1);

        if (duplicateBody is not null)
        {
            throw new ArgumentException(
                $"Duplicate planetary position detected for body '{duplicateBody.Key}'.",
                nameof(positions));
        }

        var copiedAspects = (aspects ?? throw new ArgumentNullException(nameof(aspects)))
            .OrderBy(static aspect => aspect.SourceBody)
            .ThenBy(static aspect => aspect.TargetBody)
            .ThenBy(static aspect => aspect.AspectType)
            .ToArray();

        var availableBodies = copiedPositions.Select(static position => position.Body).ToHashSet();

        var invalidAspect = copiedAspects.FirstOrDefault(aspect =>
            !availableBodies.Contains(aspect.SourceBody) || !availableBodies.Contains(aspect.TargetBody));

        if (invalidAspect is not null)
        {
            throw new ArgumentException(
                "All calculated aspects must reference bodies present in the chart positions.",
                nameof(aspects));
        }

        BirthMoment = birthMoment;
        Positions = Array.AsReadOnly(copiedPositions);
        Aspects = Array.AsReadOnly(copiedAspects);
        EphemerisSourceVersion = string.IsNullOrWhiteSpace(ephemerisSourceVersion) ? null : ephemerisSourceVersion.Trim();
    }

    /// <summary>
    /// Gets the resolved birth moment used for the chart snapshot.
    /// </summary>
    public BirthMoment BirthMoment { get; }

    /// <summary>
    /// Gets the calculated planetary positions.
    /// </summary>
    public IReadOnlyList<PlanetPosition> Positions { get; }

    /// <summary>
    /// Gets the calculated major aspects between the chart positions.
    /// </summary>
    public IReadOnlyList<CalculatedAspect> Aspects { get; }

    /// <summary>
    /// Gets the optional ephemeris source metadata carried with the chart snapshot.
    /// </summary>
    public string? EphemerisSourceVersion { get; }

    /// <summary>
    /// Creates a natal chart snapshot and calculates major aspects from the supplied positions.
    /// </summary>
    /// <param name="birthMoment">The resolved birth moment used for calculation.</param>
    /// <param name="positions">The calculated planetary positions.</param>
    /// <param name="aspectOrbDegrees">The allowed orb in degrees for aspect detection.</param>
    /// <param name="ephemerisSourceVersion">The optional ephemeris source metadata.</param>
    /// <returns>A new natal chart snapshot with detected major aspects.</returns>
    public static NatalChart Create(
        BirthMoment birthMoment,
        IEnumerable<PlanetPosition> positions,
        double aspectOrbDegrees = AspectMath.DefaultOrbDegrees,
        string? ephemerisSourceVersion = null)
    {
        var copiedPositions = (positions ?? throw new ArgumentNullException(nameof(positions))).ToArray();
        var calculatedAspects = PlanetaryAspectCalculator.Calculate(copiedPositions, aspectOrbDegrees);

        return new NatalChart(birthMoment, copiedPositions, calculatedAspects, ephemerisSourceVersion);
    }
}
