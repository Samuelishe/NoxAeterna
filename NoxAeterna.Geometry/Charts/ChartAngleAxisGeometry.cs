using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Represents a diameter joining a principal angle with its opposite.
/// </summary>
public readonly record struct ChartAngleAxisGeometry(
    ChartAngleAxisType AxisType,
    ZodiacLongitude PrimaryLongitude,
    ZodiacLongitude OppositeLongitude,
    AngularPosition PrimaryDisplayAngle,
    AngularPosition OppositeDisplayAngle,
    RadialPoint PrimaryPoint,
    RadialPoint OppositePoint,
    RadialPoint LabelAnchor);
