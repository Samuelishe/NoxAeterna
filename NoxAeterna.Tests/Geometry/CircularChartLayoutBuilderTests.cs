using NodaTime;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Tests.Geometry;

public sealed class CircularChartLayoutBuilderTests
{
    [Theory]
    [InlineData(-1d, 359d)]
    [InlineData(360d, 0d)]
    [InlineData(721d, 1d)]
    public void AngularPosition_NormalizesInput(double input, double expected)
    {
        var position = new AngularPosition(input);

        Assert.Equal(expected, position.Degrees, precision: 10);
    }

    [Theory]
    [InlineData(0d, 0d, -0.88d)]
    [InlineData(90d, 0.88d, 0d)]
    [InlineData(180d, 0d, 0.88d)]
    public void RadialPoint_ProducesDeterministicCircularCoordinates(
        double degrees,
        double expectedX,
        double expectedY)
    {
        var point = new RadialPoint(new AngularPosition(degrees), 0.88d);

        Assert.Equal(expectedX, point.X, precision: 10);
        Assert.Equal(expectedY, point.Y, precision: 10);
    }

    [Fact]
    public void Build_CreatesDeterministicCircularLayoutForNormalNonClusterChart()
    {
        var layout = new CircularChartLayoutBuilder().Build(CreateNormalChart());

        Assert.Equal(12, layout.ZodiacSectors.Count);
        Assert.Equal(ZodiacSign.Aries, layout.ZodiacSectors[0].Sign);
        Assert.Equal(0d, layout.ZodiacSectors[0].StartAngle.Degrees, precision: 10);
        Assert.Equal(30d, layout.ZodiacSectors[0].EndAngle.Degrees, precision: 10);
        Assert.Equal(ZodiacSign.Pisces, layout.ZodiacSectors[^1].Sign);
        Assert.Equal(330d, layout.ZodiacSectors[^1].StartAngle.Degrees, precision: 10);
        Assert.Equal(0d, layout.ZodiacSectors[^1].EndAngle.Degrees, precision: 10);

        Assert.Equal(3, layout.PlanetGlyphSlots.Count);
        Assert.All(
            layout.PlanetGlyphSlots,
            slot =>
            {
                Assert.Equal(slot.SourceAngle, slot.DisplayAngle);
                Assert.Equal(0, slot.RadialLaneIndex);
            });
        Assert.Equal(10d, layout.PlanetGlyphSlots[0].SourceAngle.Degrees, precision: 10);
        Assert.Equal(220d, layout.PlanetGlyphSlots[1].SourceAngle.Degrees, precision: 10);
        Assert.Equal(100d, layout.PlanetGlyphSlots[2].SourceAngle.Degrees, precision: 10);

        Assert.Equal(2, layout.AspectLines.Count);
        Assert.Equal(AspectType.Square, layout.AspectLines[0].AspectType);
        Assert.Equal(CelestialBody.Sun, layout.AspectLines[0].SourceBody);
        Assert.Equal(CelestialBody.Neptune, layout.AspectLines[0].TargetBody);
        Assert.Equal(AspectType.Trine, layout.AspectLines[1].AspectType);
    }

    [Fact]
    public void Build_RecognizesClusterAcrossZeroAsOneCircularCluster()
    {
        var chart = CreateChart(
            (CelestialBody.Sun, 358d),
            (CelestialBody.Moon, 359d),
            (CelestialBody.Mercury, 0d),
            (CelestialBody.Venus, 1d),
            (CelestialBody.Mars, 2d));

        var layout = new CircularChartLayoutBuilder().Build(chart);
        var circularOrder = new[]
        {
            CelestialBody.Sun,
            CelestialBody.Moon,
            CelestialBody.Mercury,
            CelestialBody.Venus,
            CelestialBody.Mars
        };
        var displayAngles = circularOrder
            .Select(body => layout.PlanetGlyphSlots.Single(slot => slot.Body == body).DisplayAngle.Degrees)
            .ToArray();

        for (var index = 2; index < displayAngles.Length; index++)
        {
            displayAngles[index] += 360d;
        }

        Assert.Single(layout.PlanetGlyphSlots.Select(static slot => slot.ClusterIndex).Distinct());
        Assert.True(displayAngles.SequenceEqual(displayAngles.Order()));
    }

    [Fact]
    public void Build_DenseSixPlanetClusterUsesUniqueAnchorsAndSafeLaneSeparation()
    {
        var chart = CreateChart(
            (CelestialBody.Sun, 42d),
            (CelestialBody.Moon, 42d),
            (CelestialBody.Mercury, 42d),
            (CelestialBody.Venus, 42d),
            (CelestialBody.Mars, 42d),
            (CelestialBody.Jupiter, 42d));

        var layout = new CircularChartLayoutBuilder().Build(chart);
        var anchors = layout.PlanetGlyphSlots
            .Select(static slot => (slot.DisplayAngle.Degrees, slot.AnchorPoint.RadiusRatio))
            .ToArray();

        Assert.Equal(anchors.Length, anchors.Distinct().Count());
        for (var first = 0; first < layout.PlanetGlyphSlots.Count; first++)
        {
            for (var second = first + 1; second < layout.PlanetGlyphSlots.Count; second++)
            {
                var deltaX = layout.PlanetGlyphSlots[first].AnchorPoint.X -
                             layout.PlanetGlyphSlots[second].AnchorPoint.X;
                var deltaY = layout.PlanetGlyphSlots[first].AnchorPoint.Y -
                             layout.PlanetGlyphSlots[second].AnchorPoint.Y;
                Assert.True(Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY)) >= 0.12d);
            }
        }

        Assert.All(
            layout.PlanetGlyphSlots.GroupBy(static slot => slot.RadialLaneIndex),
            sameLane =>
            {
                var ordered = sameLane.OrderBy(static slot => slot.DisplayAngle.Degrees).ToArray();
                for (var index = 1; index < ordered.Length; index++)
                {
                    Assert.True(
                        CircularDelta(ordered[index - 1].DisplayAngle.Degrees, ordered[index].DisplayAngle.Degrees) >= 6d - 1e-9);
                }
            });
        Assert.Equal(
            Enum.GetValues<CelestialBody>().Take(6),
            layout.PlanetGlyphSlots.Select(static slot => slot.Body));
        Assert.All(
            layout.PlanetGlyphSlots,
            slot => Assert.True(layout.RadialLanes.PlanetGlyphLane.Contains(slot.AnchorPoint.RadiusRatio)));
    }

    [Fact]
    public void Build_PermutationOfInputProducesSamePlacementByBody()
    {
        var first = CreateChart(
            (CelestialBody.Sun, 358d),
            (CelestialBody.Moon, 359d),
            (CelestialBody.Mercury, 0d),
            (CelestialBody.Venus, 1d),
            (CelestialBody.Mars, 2d),
            (CelestialBody.Jupiter, 3d));
        var second = CreateChart(
            (CelestialBody.Jupiter, 3d),
            (CelestialBody.Venus, 1d),
            (CelestialBody.Sun, 358d),
            (CelestialBody.Mars, 2d),
            (CelestialBody.Mercury, 0d),
            (CelestialBody.Moon, 359d));
        var builder = new CircularChartLayoutBuilder();

        var firstByBody = builder.Build(first).PlanetGlyphSlots.ToDictionary(static slot => slot.Body);
        var secondByBody = builder.Build(second).PlanetGlyphSlots.ToDictionary(static slot => slot.Body);

        Assert.Equal(firstByBody, secondByBody);
    }

    [Fact]
    public void Build_RepeatedBuildIsIdentical()
    {
        var chart = CreateNormalChart();
        var builder = new CircularChartLayoutBuilder();

        var firstLayout = builder.Build(chart);
        var secondLayout = builder.Build(chart);

        Assert.Equal(firstLayout.RadialLanes, secondLayout.RadialLanes);
        Assert.Equal(firstLayout.ZodiacSectors, secondLayout.ZodiacSectors);
        Assert.Equal(firstLayout.PlanetGlyphSlots, secondLayout.PlanetGlyphSlots);
        Assert.Equal(firstLayout.AspectLines, secondLayout.AspectLines);
    }

    [Fact]
    public void Build_DeclaresNonOverlappingVisualLanesAndKeepsGeometryInsideThem()
    {
        var layout = new CircularChartLayoutBuilder().Build(CreateNormalChart());
        var lanes = layout.RadialLanes;

        Assert.True(lanes.PlanetGlyphLane.OuterRadiusRatio < lanes.ZodiacGlyphLane.InnerRadiusRatio);
        Assert.All(
            lanes.PlanetSubLaneRadiusRatios,
            radius => Assert.True(lanes.PlanetGlyphLane.Contains(radius)));
        Assert.All(
            layout.PlanetGlyphSlots,
            slot => Assert.True(lanes.PlanetGlyphLane.Contains(slot.AnchorPoint.RadiusRatio)));
        Assert.All(
            layout.AspectLines,
            line =>
            {
                Assert.True(line.SourcePoint.RadiusRatio <= lanes.AspectInteriorRadiusRatio);
                Assert.True(line.TargetPoint.RadiusRatio <= lanes.AspectInteriorRadiusRatio);
            });
        Assert.True(lanes.OuterBoundaryRadiusRatio < 0.98d);
    }

    [Fact]
    public void Build_PreservesSourceLongitudeWhenDisplayPlacementIsNudged()
    {
        var chart = CreateChart(
            (CelestialBody.Sun, 10d),
            (CelestialBody.Moon, 10d),
            (CelestialBody.Mercury, 10d),
            (CelestialBody.Venus, 10d),
            (CelestialBody.Mars, 10d),
            (CelestialBody.Jupiter, 10d));

        var layout = new CircularChartLayoutBuilder().Build(chart);

        Assert.All(
            layout.PlanetGlyphSlots,
            slot => Assert.Equal(slot.Longitude.Degrees, slot.SourceAngle.Degrees, precision: 10));
        Assert.Contains(
            layout.PlanetGlyphSlots,
            slot => Math.Abs(slot.SourceAngle.Degrees - slot.DisplayAngle.Degrees) > 0.01d);
        Assert.All(
            layout.AspectLines,
            line =>
            {
                Assert.Equal(
                    layout.PlanetGlyphSlots.Single(slot => slot.Body == line.SourceBody).SourceAngle,
                    line.SourceAngle);
                Assert.Equal(
                    layout.PlanetGlyphSlots.Single(slot => slot.Body == line.TargetBody).SourceAngle,
                    line.TargetAngle);
            });
    }

    [Fact]
    public void Build_WithHousesPlacesAscendantAtLeftAndUsesOneRotationEverywhere()
    {
        var chart = CreateChartWithHouses();

        var layout = new CircularChartLayoutBuilder().Build(chart);

        Assert.True(layout.Orientation.IsAscendantOriented);
        Assert.Equal(270d, layout.Orientation.Transform(new ZodiacLongitude(15d)).Degrees, precision: 10);
        Assert.Equal(255d, layout.ZodiacSectors[0].StartAngle.Degrees, precision: 10);

        var sunSlot = layout.PlanetGlyphSlots.Single(slot => slot.Body == CelestialBody.Sun);
        Assert.Equal(10d, sunSlot.Longitude.Degrees, precision: 10);
        Assert.Equal(265d, sunSlot.SourceAngle.Degrees, precision: 10);

        var firstCusp = layout.HouseCusps.Single(cusp => cusp.HouseNumber.Value == 1);
        Assert.Equal(15d, firstCusp.Longitude.Degrees, precision: 10);
        Assert.Equal(270d, firstCusp.DisplayAngle.Degrees, precision: 10);

        var ascendantAxis = layout.AngleAxes.Single(
            axis => axis.AxisType == ChartAngleAxisType.AscendantDescendant);
        Assert.Equal(270d, ascendantAxis.PrimaryDisplayAngle.Degrees, precision: 10);
        Assert.Equal(90d, ascendantAxis.OppositeDisplayAngle.Degrees, precision: 10);

        var midheavenAxis = layout.AngleAxes.Single(
            axis => axis.AxisType == ChartAngleAxisType.MidheavenImumCoeli);
        Assert.Equal(177d, midheavenAxis.PrimaryDisplayAngle.Degrees, precision: 10);

        Assert.All(
            layout.AspectLines,
            line =>
            {
                Assert.Equal(
                    layout.PlanetGlyphSlots.Single(slot => slot.Body == line.SourceBody).SourceAngle,
                    line.SourceAngle);
                Assert.Equal(
                    layout.PlanetGlyphSlots.Single(slot => slot.Body == line.TargetBody).SourceAngle,
                    line.TargetAngle);
            });
    }

    [Fact]
    public void Build_WithHousesCreatesDeterministicMultiRingGeometryInsideDeclaredLanes()
    {
        var chart = CreateChartWithHouses();
        var builder = new CircularChartLayoutBuilder();

        var first = builder.Build(chart);
        var second = builder.Build(chart);

        Assert.Equal(12, first.HouseCusps.Count);
        Assert.Equal(12, first.HouseNumberAnchors.Count);
        Assert.Equal(2, first.AngleAxes.Count);
        Assert.Equal(first.HouseCusps, second.HouseCusps);
        Assert.Equal(first.HouseNumberAnchors, second.HouseNumberAnchors);
        Assert.Equal(first.AngleAxes, second.AngleAxes);
        Assert.True(
            first.RadialLanes.HouseRing.OuterRadiusRatio <
            first.RadialLanes.PlanetGlyphLane.InnerRadiusRatio);
        Assert.All(
            first.HouseCusps,
            cusp =>
            {
                Assert.Equal(
                    first.RadialLanes.HouseRing.InnerRadiusRatio,
                    cusp.InnerPoint.RadiusRatio,
                    precision: 10);
                Assert.Equal(
                    first.RadialLanes.ZodiacRing.InnerRadiusRatio,
                    cusp.OuterPoint.RadiusRatio,
                    precision: 10);
            });
        Assert.All(
            first.HouseNumberAnchors,
            anchor => Assert.True(
                first.RadialLanes.HouseNumberLane.Contains(anchor.AnchorPoint.RadiusRatio)));
        Assert.All(
            first.AngleAxes,
            axis =>
            {
                Assert.Equal(
                    first.RadialLanes.ZodiacRing.InnerRadiusRatio,
                    axis.PrimaryPoint.RadiusRatio,
                    precision: 10);
                Assert.Equal(
                    first.RadialLanes.ZodiacRing.InnerRadiusRatio,
                    axis.OppositePoint.RadiusRatio,
                    precision: 10);
            });
    }

    [Fact]
    public void Build_UnavailableHousesPreserveAriesAtTopAndProduceNoHouseGeometry()
    {
        var chart = NatalChart.Create(
            CreateBirthMoment(),
            [new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(10d), false)],
            houses: NatalHouses.CreateUnavailable(
                HouseSystem.Placidus,
                NatalHousesAvailability.UnavailableUnknownTime));

        var layout = new CircularChartLayoutBuilder().Build(chart);

        Assert.False(layout.Orientation.IsAscendantOriented);
        Assert.Equal(0d, layout.ZodiacSectors[0].StartAngle.Degrees, precision: 10);
        Assert.Equal(10d, layout.PlanetGlyphSlots[0].SourceAngle.Degrees, precision: 10);
        Assert.Empty(layout.HouseCusps);
        Assert.Empty(layout.HouseNumberAnchors);
        Assert.Empty(layout.AngleAxes);
    }

    private static double CircularDelta(double first, double second)
    {
        var delta = Math.Abs(first - second);
        return Math.Min(delta, 360d - delta);
    }

    private static NatalChart CreateNormalChart() =>
        NatalChart.Create(
            CreateBirthMoment(),
            new[]
            {
                new PlanetPosition(CelestialBody.Neptune, new ZodiacLongitude(100d), false),
                new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(10d), false),
                new PlanetPosition(CelestialBody.Mars, new ZodiacLongitude(220d), true)
            });

    private static NatalChart CreateChart(params (CelestialBody Body, double Longitude)[] positions) =>
        NatalChart.Create(
            CreateBirthMoment(),
            positions.Select(static item =>
                new PlanetPosition(item.Body, new ZodiacLongitude(item.Longitude), false)));

    private static NatalChart CreateChartWithHouses()
    {
        var cuspLongitudes = new[]
        {
            15d, 44d, 74d, 102d, 134d, 165d,
            195d, 224d, 254d, 282d, 314d, 345d
        };
        var houses = NatalHouses.CreateAvailable(
            HouseSystem.Placidus,
            cuspLongitudes.Select((longitude, index) =>
                new HouseCusp(new HouseNumber(index + 1), new ZodiacLongitude(longitude))),
            new ChartAngles(new ZodiacLongitude(15d), new ZodiacLongitude(282d)),
            "geometry-house-fixture");

        return NatalChart.Create(
            CreateBirthMoment(),
            new[]
            {
                new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(10d), false),
                new PlanetPosition(CelestialBody.Moon, new ZodiacLongitude(100d), false),
                new PlanetPosition(CelestialBody.Mars, new ZodiacLongitude(220d), true)
            },
            houses: houses);
    }

    private static BirthMoment CreateBirthMoment() =>
        new(
            new LocalDateTime(1990, 7, 14, 13, 45),
            new TimezoneId("Europe/Moscow"),
            Instant.FromUtc(1990, 7, 14, 9, 45),
            TimeResolutionStatus.Resolved,
            BirthTimeAccuracy.ExactTime,
            "Geometry fixture");
}
