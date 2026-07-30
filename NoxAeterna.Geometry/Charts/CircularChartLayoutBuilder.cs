using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Builds a minimal render-independent circular layout for a natal chart snapshot.
/// </summary>
public sealed class CircularChartLayoutBuilder
{
    private const double ClusterThresholdDegrees = 16d;
    private const double MinimumAdjacentDisplaySeparationDegrees = 18d;
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

        var availableHouses = chart.Houses is { IsAvailable: true, Angles: not null }
            ? chart.Houses
            : null;
        var orientation = availableHouses?.Angles is { } chartAngles
            ? ChartOrientation.AscendantAtLeft(chartAngles.Ascendant)
            : ChartOrientation.AriesAtTop;

        var zodiacSectors = Enum
            .GetValues<ZodiacSign>()
            .Select(sign => new ZodiacSectorGeometry(
                sign,
                orientation.TransformDegrees((int)sign * 30d),
                orientation.TransformDegrees(((int)sign + 1) * 30d),
                RadialLanes.ZodiacRing.InnerRadiusRatio,
                RadialLanes.ZodiacRing.OuterRadiusRatio))
            .ToArray();

        var placementsByBody = BuildPlanetPlacements(chart.Positions);
        var glyphSlots = chart.Positions
            .OrderBy(static position => position.Body)
            .Select((position, index) =>
            {
                var sourceAngle = orientation.Transform(position.EclipticLongitude);
                var placement = placementsByBody[position.Body];
                var displayAngle = orientation.TransformDegrees(placement.DisplayLongitude);
                var radiusRatio = RadialLanes.PlanetSubLaneRadiusRatios[placement.RadialLaneIndex];
                return new PlanetGlyphSlot(
                    position.Body,
                    position.EclipticLongitude,
                    sourceAngle,
                    displayAngle,
                    new RadialPoint(displayAngle, radiusRatio),
                    position.IsRetrograde,
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

        var houseCusps = availableHouses is null
            ? Array.Empty<HouseCuspGeometry>()
            : BuildHouseCusps(availableHouses, orientation);
        var houseNumberAnchors = availableHouses is null
            ? Array.Empty<HouseNumberAnchor>()
            : BuildHouseNumberAnchors(availableHouses, orientation);
        var angleAxes = availableHouses?.Angles is null
            ? Array.Empty<ChartAngleAxisGeometry>()
            : BuildAngleAxes(availableHouses.Angles, orientation);

        return new CircularChartLayout(
            RadialLanes,
            orientation,
            zodiacSectors,
            glyphSlots,
            aspectLines,
            houseCusps,
            houseNumberAnchors,
            angleAxes);
    }

    private HouseCuspGeometry[] BuildHouseCusps(NatalHouses houses, ChartOrientation orientation) =>
        houses.Cusps
            .Select(cusp =>
            {
                var displayAngle = orientation.Transform(cusp.Longitude);
                return new HouseCuspGeometry(
                    cusp.HouseNumber,
                    cusp.Longitude,
                    displayAngle,
                    new RadialPoint(displayAngle, RadialLanes.HouseRing.InnerRadiusRatio),
                    new RadialPoint(displayAngle, RadialLanes.ZodiacRing.InnerRadiusRatio));
            })
            .ToArray();

    private HouseNumberAnchor[] BuildHouseNumberAnchors(
        NatalHouses houses,
        ChartOrientation orientation)
    {
        var cusps = houses.Cusps;
        var anchorRadius = (RadialLanes.HouseNumberLane.InnerRadiusRatio +
                            RadialLanes.HouseNumberLane.OuterRadiusRatio) / 2d;
        var anchors = new HouseNumberAnchor[cusps.Count];

        for (var index = 0; index < cusps.Count; index++)
        {
            var current = cusps[index];
            var next = cusps[(index + 1) % cusps.Count];
            var span = ZodiacLongitude.Normalize(next.Longitude.Degrees - current.Longitude.Degrees);
            var midpoint = orientation.TransformDegrees(current.Longitude.Degrees + (span / 2d));

            anchors[index] = new HouseNumberAnchor(
                current.HouseNumber,
                midpoint,
                new RadialPoint(midpoint, anchorRadius));
        }

        return anchors;
    }

    private ChartAngleAxisGeometry[] BuildAngleAxes(
        ChartAngles angles,
        ChartOrientation orientation)
    {
        return
        [
            BuildAngleAxis(
                ChartAngleAxisType.AscendantDescendant,
                angles.Ascendant,
                angles.Descendant,
                orientation),
            BuildAngleAxis(
                ChartAngleAxisType.MidheavenImumCoeli,
                angles.Midheaven,
                angles.ImumCoeli,
                orientation)
        ];
    }

    private ChartAngleAxisGeometry BuildAngleAxis(
        ChartAngleAxisType axisType,
        ZodiacLongitude primaryLongitude,
        ZodiacLongitude oppositeLongitude,
        ChartOrientation orientation)
    {
        var primaryAngle = orientation.Transform(primaryLongitude);
        var oppositeAngle = orientation.Transform(oppositeLongitude);

        return new ChartAngleAxisGeometry(
            axisType,
            primaryLongitude,
            oppositeLongitude,
            primaryAngle,
            oppositeAngle,
            new RadialPoint(primaryAngle, RadialLanes.ZodiacRing.InnerRadiusRatio),
            new RadialPoint(oppositeAngle, RadialLanes.ZodiacRing.InnerRadiusRatio),
            new RadialPoint(primaryAngle, RadialLanes.AngleLabelRadiusRatio),
            new RadialPoint(oppositeAngle, RadialLanes.AngleLabelRadiusRatio));
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
