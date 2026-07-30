using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Represents a render-independent glyph slot for a planetary body.
/// </summary>
public readonly record struct PlanetGlyphSlot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlanetGlyphSlot"/> struct.
    /// </summary>
    /// <param name="body">The celestial body.</param>
    /// <param name="longitude">The source zodiac longitude.</param>
    /// <param name="sourceAngle">The source astronomical chart-space angle.</param>
    /// <param name="displayAngle">The collision-safe display angle.</param>
    /// <param name="anchorPoint">The normalized radial anchor point.</param>
    /// <param name="slotIndex">The deterministic slot index within the layout.</param>
    /// <param name="radialLaneIndex">The ordered planet sub-lane index.</param>
    /// <param name="clusterIndex">The deterministic circular cluster index.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when slot indices are negative.</exception>
    public PlanetGlyphSlot(
        CelestialBody body,
        ZodiacLongitude longitude,
        AngularPosition sourceAngle,
        AngularPosition displayAngle,
        RadialPoint anchorPoint,
        int slotIndex,
        int radialLaneIndex,
        int clusterIndex)
    {
        if (slotIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot index must be non-negative.");
        }

        if (radialLaneIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radialLaneIndex), "Radial lane index must be non-negative.");
        }

        if (clusterIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(clusterIndex), "Cluster index must be non-negative.");
        }

        Body = body;
        Longitude = longitude;
        SourceAngle = sourceAngle;
        DisplayAngle = displayAngle;
        AnchorPoint = anchorPoint;
        SlotIndex = slotIndex;
        RadialLaneIndex = radialLaneIndex;
        ClusterIndex = clusterIndex;
    }

    /// <summary>
    /// Gets the celestial body assigned to the slot.
    /// </summary>
    public CelestialBody Body { get; }

    /// <summary>
    /// Gets the source zodiac longitude.
    /// </summary>
    public ZodiacLongitude Longitude { get; }

    /// <summary>
    /// Gets the source astronomical chart-space angle.
    /// </summary>
    public AngularPosition SourceAngle { get; }

    /// <summary>
    /// Gets the collision-safe display angle without changing the source longitude.
    /// </summary>
    public AngularPosition DisplayAngle { get; }

    /// <summary>
    /// Gets the normalized radial anchor point.
    /// </summary>
    public RadialPoint AnchorPoint { get; }

    /// <summary>
    /// Gets the deterministic slot index within the layout.
    /// </summary>
    public int SlotIndex { get; }

    /// <summary>
    /// Gets the ordered planet sub-lane index.
    /// </summary>
    public int RadialLaneIndex { get; }

    /// <summary>
    /// Gets the deterministic circular cluster index.
    /// </summary>
    public int ClusterIndex { get; }
}
