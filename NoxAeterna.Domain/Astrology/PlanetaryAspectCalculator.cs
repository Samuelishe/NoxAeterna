namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Detects major aspects across a set of planetary positions.
/// </summary>
public static class PlanetaryAspectCalculator
{
    private static readonly AspectType[] SupportedAspectTypes =
    [
        AspectType.Conjunction,
        AspectType.Sextile,
        AspectType.Square,
        AspectType.Trine,
        AspectType.Opposition
    ];

    /// <summary>
    /// Calculates major aspects between the supplied planetary positions.
    /// </summary>
    /// <param name="positions">The positions to compare.</param>
    /// <param name="orbDegrees">The allowed orb in degrees.</param>
    /// <returns>A deterministic read-only collection of detected aspects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="positions"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the positions contain duplicate celestial bodies.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="orbDegrees"/> is not a finite non-negative number.</exception>
    public static IReadOnlyList<CalculatedAspect> Calculate(
        IEnumerable<PlanetPosition> positions,
        double orbDegrees = AspectMath.DefaultOrbDegrees)
    {
        if (double.IsNaN(orbDegrees) || double.IsInfinity(orbDegrees) || orbDegrees < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(orbDegrees), "Orb must be a finite non-negative number.");
        }

        var orderedPositions = (positions ?? throw new ArgumentNullException(nameof(positions)))
            .OrderBy(static position => position.Body)
            .ToArray();

        var duplicateBody = orderedPositions
            .GroupBy(static position => position.Body)
            .FirstOrDefault(static group => group.Count() > 1);

        if (duplicateBody is not null)
        {
            throw new ArgumentException(
                $"Duplicate planetary position detected for body '{duplicateBody.Key}'.",
                nameof(positions));
        }

        var detectedAspects = new List<CalculatedAspect>();

        for (var i = 0; i < orderedPositions.Length; i++)
        {
            for (var j = i + 1; j < orderedPositions.Length; j++)
            {
                var source = orderedPositions[i];
                var target = orderedPositions[j];

                if (source.Body == target.Body)
                {
                    continue;
                }

                var angularDeltaDegrees = AspectMath.CalculateAngularDelta(
                    source.EclipticLongitude,
                    target.EclipticLongitude);

                var bestMatch = SupportedAspectTypes
                    .Select(aspectType => new
                    {
                        AspectType = aspectType,
                        OrbDistanceDegrees = Math.Abs(angularDeltaDegrees - (double)aspectType)
                    })
                    .Where(candidate => candidate.OrbDistanceDegrees <= orbDegrees)
                    .OrderBy(candidate => candidate.OrbDistanceDegrees)
                    .ThenBy(candidate => (double)candidate.AspectType)
                    .FirstOrDefault();

                if (bestMatch is null)
                {
                    continue;
                }

                detectedAspects.Add(new CalculatedAspect(
                    source.Body,
                    target.Body,
                    bestMatch.AspectType,
                    source.EclipticLongitude,
                    target.EclipticLongitude));
            }
        }

        return Array.AsReadOnly(detectedAspects.ToArray());
    }
}
