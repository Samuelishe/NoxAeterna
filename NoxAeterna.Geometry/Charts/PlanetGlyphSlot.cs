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
    /// <param name="preferredGlyphAnchor">The preferred exact-angle radial glyph anchor.</param>
    /// <param name="slotIndex">The deterministic slot index within the layout.</param>
    /// <param name="preferredRadialLaneIndex">The ordered preferred planet sub-lane index.</param>
    /// <param name="clusterIndex">The deterministic circular cluster index.</param>
    /// <param name="sourceHouseNumber">The exact source house when houses are available.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when slot indices are negative.</exception>
    public PlanetGlyphSlot(
        CelestialBody body,
        ZodiacLongitude longitude,
        AngularPosition sourceAngle,
        RadialPoint preferredGlyphAnchor,
        bool isRetrograde,
        int slotIndex,
        int preferredRadialLaneIndex,
        int clusterIndex,
        HouseNumber? sourceHouseNumber)
    {
        if (slotIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot index must be non-negative.");
        }

        if (preferredRadialLaneIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(preferredRadialLaneIndex), "Radial lane index must be non-negative.");
        }

        if (clusterIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(clusterIndex), "Cluster index must be non-negative.");
        }

        Body = body;
        Longitude = longitude;
        SourceAngle = sourceAngle;
        PreferredGlyphAnchor = preferredGlyphAnchor;
        IsRetrograde = isRetrograde;
        SlotIndex = slotIndex;
        PreferredRadialLaneIndex = preferredRadialLaneIndex;
        ClusterIndex = clusterIndex;
        SourceHouseNumber = sourceHouseNumber;
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
    /// Gets the preferred normalized radial glyph anchor at the exact source angle.
    /// </summary>
    public RadialPoint PreferredGlyphAnchor { get; }

    /// <summary>
    /// Gets whether the source planetary position is retrograde.
    /// </summary>
    public bool IsRetrograde { get; }

    /// <summary>
    /// Gets the deterministic slot index within the layout.
    /// </summary>
    public int SlotIndex { get; }

    /// <summary>
    /// Gets the ordered planet sub-lane index.
    /// </summary>
    public int PreferredRadialLaneIndex { get; }

    /// <summary>
    /// Gets the deterministic circular cluster index.
    /// </summary>
    public int ClusterIndex { get; }

    /// <summary>
    /// Gets the exact source house when reliable houses are available.
    /// </summary>
    public HouseNumber? SourceHouseNumber { get; }
}
