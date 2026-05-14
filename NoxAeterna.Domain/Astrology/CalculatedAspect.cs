namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Represents a detected aspect between two distinct celestial bodies.
/// </summary>
public sealed record CalculatedAspect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculatedAspect"/> class.
    /// </summary>
    /// <param name="sourceBody">The first body in the aspect pair.</param>
    /// <param name="targetBody">The second body in the aspect pair.</param>
    /// <param name="aspectType">The detected aspect type.</param>
    /// <param name="sourceLongitude">The longitude for the first body.</param>
    /// <param name="targetLongitude">The longitude for the second body.</param>
    /// <exception cref="ArgumentException">Thrown when both bodies are the same.</exception>
    public CalculatedAspect(
        CelestialBody sourceBody,
        CelestialBody targetBody,
        AspectType aspectType,
        ZodiacLongitude sourceLongitude,
        ZodiacLongitude targetLongitude)
    {
        if (sourceBody == targetBody)
        {
            throw new ArgumentException("Calculated aspects require two distinct celestial bodies.", nameof(targetBody));
        }

        if (sourceBody > targetBody)
        {
            (sourceBody, targetBody) = (targetBody, sourceBody);
            (sourceLongitude, targetLongitude) = (targetLongitude, sourceLongitude);
        }

        SourceBody = sourceBody;
        TargetBody = targetBody;
        AspectType = aspectType;
        SourceLongitude = sourceLongitude;
        TargetLongitude = targetLongitude;
        AngularDeltaDegrees = AspectMath.CalculateAngularDelta(sourceLongitude, targetLongitude);
        OrbDistanceDegrees = Math.Abs(AngularDeltaDegrees - (double)aspectType);
    }

    /// <summary>
    /// Gets the first body in the canonical aspect pair.
    /// </summary>
    public CelestialBody SourceBody { get; }

    /// <summary>
    /// Gets the second body in the canonical aspect pair.
    /// </summary>
    public CelestialBody TargetBody { get; }

    /// <summary>
    /// Gets the detected aspect type.
    /// </summary>
    public AspectType AspectType { get; }

    /// <summary>
    /// Gets the smallest angular distance between the two longitudes.
    /// </summary>
    public double AngularDeltaDegrees { get; }

    /// <summary>
    /// Gets the absolute orb distance from the exact aspect angle.
    /// </summary>
    public double OrbDistanceDegrees { get; }

    /// <summary>
    /// Gets the canonical source longitude.
    /// </summary>
    public ZodiacLongitude SourceLongitude { get; }

    /// <summary>
    /// Gets the canonical target longitude.
    /// </summary>
    public ZodiacLongitude TargetLongitude { get; }
}
