namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Represents an inclusive normalized radial interval owned by chart geometry.
/// </summary>
public readonly record struct RadialLaneBounds
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RadialLaneBounds"/> struct.
    /// </summary>
    public RadialLaneBounds(double innerRadiusRatio, double outerRadiusRatio)
    {
        ValidateRadius(nameof(innerRadiusRatio), innerRadiusRatio);
        ValidateRadius(nameof(outerRadiusRatio), outerRadiusRatio);

        if (innerRadiusRatio >= outerRadiusRatio)
        {
            throw new ArgumentOutOfRangeException(
                nameof(innerRadiusRatio),
                "The inner radius must be smaller than the outer radius.");
        }

        InnerRadiusRatio = innerRadiusRatio;
        OuterRadiusRatio = outerRadiusRatio;
    }

    /// <summary>
    /// Gets the inclusive inner normalized radius.
    /// </summary>
    public double InnerRadiusRatio { get; }

    /// <summary>
    /// Gets the inclusive outer normalized radius.
    /// </summary>
    public double OuterRadiusRatio { get; }

    /// <summary>
    /// Gets the midpoint normalized radius.
    /// </summary>
    public double MidpointRadiusRatio => (InnerRadiusRatio + OuterRadiusRatio) / 2d;

    /// <summary>
    /// Determines whether a normalized radius is inside this interval.
    /// </summary>
    public bool Contains(double radiusRatio) =>
        radiusRatio >= InnerRadiusRatio && radiusRatio <= OuterRadiusRatio;

    private static void ValidateRadius(string parameterName, double radiusRatio)
    {
        if (!double.IsFinite(radiusRatio) || radiusRatio < 0d || radiusRatio > 1d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Radius ratio must be a finite number in the range [0, 1].");
        }
    }
}
