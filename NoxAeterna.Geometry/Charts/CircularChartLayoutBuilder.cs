using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Builds a minimal render-independent circular layout for a natal chart snapshot.
/// </summary>
public sealed class CircularChartLayoutBuilder
{
    private const double ClusterThresholdDegrees = 7d;
    private const double MinimumAdjacentDisplaySeparationDegrees = 12d;
    private const double MaximumAngularNudgeDegrees = 55d;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularChartLayoutBuilder"/> class.
    /// </summary>
    /// <param name="radialLanes">The named radial zones, or the project defaults.</param>
    public CircularChartLayoutBuilder(ChartRadialLanes? radialLanes = null)
    {
        RadialLanes = radialLanes ?? ChartRadialLanes.Default;
    }

    /// <summary>
    /// Gets the named radial zones used by the builder.
    /// </summary>
    public ChartRadialLanes RadialLanes { get; }

    /// <summary>
    /// Builds a render-independent circular chart layout for the supplied natal chart snapshot.
    /// </summary>
    /// <param name="chart">The source chart snapshot.</param>
    /// <returns>The prepared circular chart layout.</returns>
    public CircularChartLayout Build(NatalChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        var zodiacSectors = Enum
            .GetValues<ZodiacSign>()
            .Select(sign => new ZodiacSectorGeometry(
                sign,
                new AngularPosition((int)sign * 30d),
                new AngularPosition(((int)sign + 1) * 30d),
                RadialLanes.ZodiacRing.InnerRadiusRatio,
                RadialLanes.ZodiacRing.OuterRadiusRatio))
            .ToArray();

        var placementsByBody = BuildPlanetPlacements(chart.Positions);
        var glyphSlots = chart.Positions
            .OrderBy(static position => position.Body)
            .Select((position, index) =>
            {
                var sourceAngle = AngularPosition.FromLongitude(position.EclipticLongitude);
                var placement = placementsByBody[position.Body];
                var displayAngle = new AngularPosition(placement.DisplayLongitude);
                var radiusRatio = RadialLanes.PlanetSubLaneRadiusRatios[placement.RadialLaneIndex];
                return new PlanetGlyphSlot(
                    position.Body,
                    position.EclipticLongitude,
                    sourceAngle,
                    displayAngle,
                    new RadialPoint(displayAngle, radiusRatio),
                    index,
                    placement.RadialLaneIndex,
                    placement.ClusterIndex);
            })
            .ToArray();

        var glyphSlotsByBody = glyphSlots.ToDictionary(static slot => slot.Body);

        var aspectLines = chart.Aspects
            .OrderBy(static aspect => aspect.SourceBody)
            .ThenBy(static aspect => aspect.TargetBody)
            .ThenBy(static aspect => aspect.AspectType)
            .Select(aspect =>
            {
                var sourceSlot = glyphSlotsByBody[aspect.SourceBody];
                var targetSlot = glyphSlotsByBody[aspect.TargetBody];

                return new AspectLineGeometry(
                    aspect.SourceBody,
                    aspect.TargetBody,
                    sourceSlot.SourceAngle,
                    targetSlot.SourceAngle,
                    new RadialPoint(sourceSlot.SourceAngle, RadialLanes.AspectInteriorRadiusRatio),
                    new RadialPoint(targetSlot.SourceAngle, RadialLanes.AspectInteriorRadiusRatio),
                    aspect.AspectType);
            })
            .ToArray();

        return new CircularChartLayout(RadialLanes, zodiacSectors, glyphSlots, aspectLines);
    }

    private IReadOnlyDictionary<CelestialBody, PlanetPlacement> BuildPlanetPlacements(
        IEnumerable<PlanetPosition> positions)
    {
        var ordered = positions
            .OrderBy(static position => position.EclipticLongitude.Degrees)
            .ThenBy(static position => position.Body)
            .ToArray();

        if (ordered.Length == 0)
        {
            return new Dictionary<CelestialBody, PlanetPlacement>();
        }

        var largestGapIndex = FindLargestCircularGapIndex(ordered);
        var unwrapped = UnwrapAfterGap(ordered, largestGapIndex);
        var clusters = SplitClusters(unwrapped);
        var placements = new Dictionary<CelestialBody, PlanetPlacement>(ordered.Length);

        for (var clusterIndex = 0; clusterIndex < clusters.Count; clusterIndex++)
        {
            var cluster = clusters[clusterIndex];
            var displayLongitudes = BuildDisplayLongitudes(cluster);

            for (var index = 0; index < cluster.Count; index++)
            {
                placements[cluster[index].Position.Body] = new PlanetPlacement(
                    displayLongitudes[index],
                    index % RadialLanes.PlanetSubLaneRadiusRatios.Count,
                    clusterIndex);
            }
        }

        return placements;
    }

    private static int FindLargestCircularGapIndex(IReadOnlyList<PlanetPosition> ordered)
    {
        var largestGapIndex = 0;
        var largestGap = double.NegativeInfinity;

        for (var index = 0; index < ordered.Count; index++)
        {
            var current = ordered[index].EclipticLongitude.Degrees;
            var next = ordered[(index + 1) % ordered.Count].EclipticLongitude.Degrees;
            var gap = index == ordered.Count - 1 ? next + 360d - current : next - current;

            if (gap > largestGap)
            {
                largestGap = gap;
                largestGapIndex = index;
            }
        }

        return largestGapIndex;
    }

    private static IReadOnlyList<UnwrappedPosition> UnwrapAfterGap(
        IReadOnlyList<PlanetPosition> ordered,
        int gapIndex)
    {
        var result = new UnwrappedPosition[ordered.Count];
        var startIndex = (gapIndex + 1) % ordered.Count;
        var previousLongitude = ordered[startIndex].EclipticLongitude.Degrees;
        var wrapOffset = 0d;

        for (var index = 0; index < ordered.Count; index++)
        {
            var position = ordered[(startIndex + index) % ordered.Count];
            var longitude = position.EclipticLongitude.Degrees;

            if (index > 0 && longitude < previousLongitude)
            {
                wrapOffset += 360d;
            }

            result[index] = new UnwrappedPosition(position, longitude + wrapOffset);
            previousLongitude = longitude;
        }

        return result;
    }

    private static IReadOnlyList<IReadOnlyList<UnwrappedPosition>> SplitClusters(
        IReadOnlyList<UnwrappedPosition> ordered)
    {
        var clusters = new List<IReadOnlyList<UnwrappedPosition>>();
        var current = new List<UnwrappedPosition> { ordered[0] };

        for (var index = 1; index < ordered.Count; index++)
        {
            if (ordered[index].Longitude - ordered[index - 1].Longitude >= ClusterThresholdDegrees)
            {
                clusters.Add(current);
                current = [];
            }

            current.Add(ordered[index]);
        }

        clusters.Add(current);
        return clusters;
    }

    private IReadOnlyList<double> BuildDisplayLongitudes(IReadOnlyList<UnwrappedPosition> cluster)
    {
        var sourceLongitudes = cluster.Select(static item => item.Longitude).ToArray();

        if (cluster.Count == 1)
        {
            return sourceLongitudes;
        }

        var minimumAdjacentSeparation = MinimumAdjacentDisplaySeparationDegrees;
        var displayLongitudes = new double[sourceLongitudes.Length];
        displayLongitudes[0] = sourceLongitudes[0];

        for (var index = 1; index < sourceLongitudes.Length; index++)
        {
            displayLongitudes[index] = Math.Max(
                sourceLongitudes[index],
                displayLongitudes[index - 1] + minimumAdjacentSeparation);
        }

        var centeringOffset = displayLongitudes
            .Select((display, index) => display - sourceLongitudes[index])
            .Average();

        for (var index = 0; index < displayLongitudes.Length; index++)
        {
            displayLongitudes[index] -= centeringOffset;
        }

        var maximumNudge = displayLongitudes
            .Select((display, index) => Math.Abs(display - sourceLongitudes[index]))
            .Max();

        if (maximumNudge > MaximumAngularNudgeDegrees)
        {
            var center = sourceLongitudes.Average();
            var first = center - (minimumAdjacentSeparation * (sourceLongitudes.Length - 1) / 2d);

            for (var index = 0; index < displayLongitudes.Length; index++)
            {
                displayLongitudes[index] = first + (index * minimumAdjacentSeparation);
            }
        }

        return displayLongitudes;
    }

    private readonly record struct UnwrappedPosition(PlanetPosition Position, double Longitude);

    private readonly record struct PlanetPlacement(
        double DisplayLongitude,
        int RadialLaneIndex,
        int ClusterIndex);
}
