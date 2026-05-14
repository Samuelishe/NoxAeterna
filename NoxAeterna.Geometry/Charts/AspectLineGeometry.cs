using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Represents a render-independent aspect line between two planetary bodies.
/// </summary>
public readonly record struct AspectLineGeometry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AspectLineGeometry"/> struct.
    /// </summary>
    /// <param name="sourceBody">The source celestial body.</param>
    /// <param name="targetBody">The target celestial body.</param>
    /// <param name="sourceAngle">The source chart-space angle.</param>
    /// <param name="targetAngle">The target chart-space angle.</param>
    /// <param name="sourcePoint">The normalized source point.</param>
    /// <param name="targetPoint">The normalized target point.</param>
    /// <param name="aspectType">The aspect type.</param>
    public AspectLineGeometry(
        CelestialBody sourceBody,
        CelestialBody targetBody,
        AngularPosition sourceAngle,
        AngularPosition targetAngle,
        RadialPoint sourcePoint,
        RadialPoint targetPoint,
        AspectType aspectType)
    {
        SourceBody = sourceBody;
        TargetBody = targetBody;
        SourceAngle = sourceAngle;
        TargetAngle = targetAngle;
        SourcePoint = sourcePoint;
        TargetPoint = targetPoint;
        AspectType = aspectType;
    }

    /// <summary>
    /// Gets the source celestial body.
    /// </summary>
    public CelestialBody SourceBody { get; }

    /// <summary>
    /// Gets the target celestial body.
    /// </summary>
    public CelestialBody TargetBody { get; }

    /// <summary>
    /// Gets the source chart-space angle.
    /// </summary>
    public AngularPosition SourceAngle { get; }

    /// <summary>
    /// Gets the target chart-space angle.
    /// </summary>
    public AngularPosition TargetAngle { get; }

    /// <summary>
    /// Gets the normalized source point.
    /// </summary>
    public RadialPoint SourcePoint { get; }

    /// <summary>
    /// Gets the normalized target point.
    /// </summary>
    public RadialPoint TargetPoint { get; }

    /// <summary>
    /// Gets the aspect type.
    /// </summary>
    public AspectType AspectType { get; }
}
