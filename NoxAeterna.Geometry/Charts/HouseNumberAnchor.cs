using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Represents a render-independent anchor for one house number.
/// </summary>
public readonly record struct HouseNumberAnchor(
    HouseNumber HouseNumber,
    AngularPosition DisplayAngle,
    RadialPoint AnchorPoint);
