using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Represents one transformed house cusp line.
/// </summary>
public readonly record struct HouseCuspGeometry(
    HouseNumber HouseNumber,
    ZodiacLongitude Longitude,
    AngularPosition DisplayAngle,
    RadialPoint InnerPoint,
    RadialPoint OuterPoint,
    RadialPoint NumberLaneMarkerInnerPoint,
    RadialPoint NumberLaneMarkerOuterPoint);
