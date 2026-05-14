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
    /// <param name="angle">The chart-space angle.</param>
    /// <param name="anchorPoint">The normalized radial anchor point.</param>
    /// <param name="slotIndex">The deterministic slot index within the layout.</param>
    /// <param name="radialBandIndex">The radial band index reserved for future collision handling.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when slot indices are negative.</exception>
    public PlanetGlyphSlot(
        CelestialBody body,
        ZodiacLongitude longitude,
        AngularPosition angle,
        RadialPoint anchorPoint,
        int slotIndex,
        int radialBandIndex)
    {
        if (slotIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot index must be non-negative.");
        }

        if (radialBandIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radialBandIndex), "Radial band index must be non-negative.");
        }

        Body = body;
        Longitude = longitude;
        Angle = angle;
        AnchorPoint = anchorPoint;
        SlotIndex = slotIndex;
        RadialBandIndex = radialBandIndex;
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
    /// Gets the chart-space angle.
    /// </summary>
    public AngularPosition Angle { get; }

    /// <summary>
    /// Gets the normalized radial anchor point.
    /// </summary>
    public RadialPoint AnchorPoint { get; }

    /// <summary>
    /// Gets the deterministic slot index within the layout.
    /// </summary>
    public int SlotIndex { get; }

    /// <summary>
    /// Gets the reserved radial band index for future collision-safe placement.
    /// </summary>
    public int RadialBandIndex { get; }
}
