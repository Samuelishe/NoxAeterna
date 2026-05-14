namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Represents a render-independent radial point within a circular chart.
/// </summary>
public readonly record struct RadialPoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RadialPoint"/> struct.
    /// </summary>
    /// <param name="angle">The chart-space angle.</param>
    /// <param name="radiusRatio">The normalized radius ratio relative to the chart outer radius.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="radiusRatio"/> is not a finite non-negative number.</exception>
    public RadialPoint(AngularPosition angle, double radiusRatio)
    {
        if (double.IsNaN(radiusRatio) || double.IsInfinity(radiusRatio) || radiusRatio < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(radiusRatio), "Radius ratio must be a finite non-negative number.");
        }

        Angle = angle;
        RadiusRatio = radiusRatio;
    }

    /// <summary>
    /// Gets the chart-space angle.
    /// </summary>
    public AngularPosition Angle { get; }

    /// <summary>
    /// Gets the normalized radius ratio relative to the chart outer radius.
    /// </summary>
    public double RadiusRatio { get; }

    /// <summary>
    /// Gets the normalized X coordinate using a chart convention where 0 degrees is at the top and angles increase clockwise.
    /// </summary>
    public double X => Math.Sin(Angle.Degrees * Math.PI / 180d) * RadiusRatio;

    /// <summary>
    /// Gets the normalized Y coordinate using a chart convention where 0 degrees is at the top and angles increase clockwise.
    /// </summary>
    public double Y => -Math.Cos(Angle.Degrees * Math.PI / 180d) * RadiusRatio;
}
