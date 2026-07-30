using NodaTime;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Astronomy.Time;
using NoxAeterna.App.Astrology;
using NoxAeterna.App.Samples;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Presentation.Astrology;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Tests.App;

public sealed class DevelopmentAstrologyChartCoordinatorTests
{
    [Fact]
    public void TryBuild_RebuildsChartFromValidInput()
    {
        var coordinator = CreateCoordinator();
        var viewModel = new BirthDataInputViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ExactTime,
                "Prague, Czechia",
                "50.0755",
                "14.4378",
                "Europe/Prague",
                LocationSource.ManualCoordinates),
            CreateAccuracies());

        var rebuilt = coordinator.TryBuild(viewModel);

        Assert.True(rebuilt);
        Assert.NotNull(coordinator.CurrentBuildResult);
        Assert.Equal(BirthTimeAccuracy.ExactTime, coordinator.CurrentBuildResult.NatalChart.BirthMoment.BirthTimeAccuracy);
        Assert.Equal(10, coordinator.CurrentBuildResult.NatalChart.Positions.Count);
        Assert.True(coordinator.CurrentBuildResult.NatalChart.Houses?.IsAvailable);
        Assert.Equal(12, coordinator.CurrentBuildResult.RenderScene.HouseCusps.Count);
    }

    [Fact]
    public void TryBuild_CalculatesHousesForApproximateTimeWithoutChangingAccuracy()
    {
        var houseCalculator = new FakeHouseCalculator();
        var coordinator = CreateCoordinator(
            DevelopmentSampleChartBuildResultFactory.Create(),
            houseCalculator);
        var viewModel = new BirthDataInputViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ApproximateTime,
                "Prague, Czechia",
                "50.0755",
                "14.4378",
                "Europe/Prague",
                LocationSource.ManualCoordinates),
            CreateAccuracies());

        Assert.True(coordinator.TryBuild(viewModel));
        Assert.Equal(1, houseCalculator.CallCount);
        Assert.Equal(
            BirthTimeAccuracy.ApproximateTime,
            coordinator.CurrentBuildResult.NatalChart.BirthMoment.BirthTimeAccuracy);
        Assert.True(coordinator.CurrentBuildResult.NatalChart.Houses?.IsAvailable);
    }

    [Fact]
    public void TryBuild_HouseProviderFailureKeepsPlanetChartWithoutFakeHouseGeometry()
    {
        var coordinator = CreateCoordinator(
            DevelopmentSampleChartBuildResultFactory.Create(),
            new UnavailableHouseCalculator());
        var viewModel = new BirthDataInputViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ExactTime,
                "High latitude fixture",
                "75",
                "14.4378",
                "Europe/Prague",
                LocationSource.ManualCoordinates),
            CreateAccuracies());

        Assert.True(coordinator.TryBuild(viewModel));
        Assert.Equal(10, coordinator.CurrentBuildResult.NatalChart.Positions.Count);
        Assert.Equal(
            NatalHousesAvailability.UnavailableCalculation,
            coordinator.CurrentBuildResult.NatalChart.Houses?.Availability);
        Assert.Empty(coordinator.CurrentBuildResult.RenderScene.HouseCusps);
        Assert.Empty(coordinator.CurrentBuildResult.RenderScene.AngleAxes);
    }

    [Fact]
    public void TryBuild_UsesTechnicalFallbackForUnknownTimeWhilePreservingAccuracy()
    {
        var houseCalculator = new FakeHouseCalculator();
        var coordinator = CreateCoordinator(
            DevelopmentSampleChartBuildResultFactory.Create(),
            houseCalculator);
        var viewModel = new BirthDataInputViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                null,
                BirthTimeAccuracy.UnknownTime,
                "Prague, Czechia",
                "50.0755",
                "14.4378",
                "Europe/Prague",
                LocationSource.ManualCoordinates),
            CreateAccuracies());

        var rebuilt = coordinator.TryBuild(viewModel);

        Assert.True(rebuilt);
        Assert.NotNull(coordinator.CurrentBuildResult);
        Assert.Equal(BirthTimeAccuracy.UnknownTime, coordinator.CurrentBuildResult.NatalChart.BirthMoment.BirthTimeAccuracy);
        Assert.Equal(12, coordinator.CurrentBuildResult.NatalChart.BirthMoment.OriginalLocalDateTime.Hour);
        Assert.Equal(0, houseCalculator.CallCount);
        Assert.Equal(
            NatalHousesAvailability.UnavailableUnknownTime,
            coordinator.CurrentBuildResult.NatalChart.Houses?.Availability);
        Assert.Empty(coordinator.CurrentBuildResult.RenderScene.HouseCusps);
    }

    [Fact]
    public void TryBuild_DoesNotReplaceCurrentSceneWhenInputIsInvalid()
    {
        var initialBuildResult = DevelopmentSampleChartBuildResultFactory.Create();
        var coordinator = CreateCoordinator(initialBuildResult);
        var viewModel = BirthDataInputViewModel.CreateDefault();

        var rebuilt = coordinator.TryBuild(viewModel);

        Assert.False(rebuilt);
        Assert.Equal(initialBuildResult, coordinator.CurrentBuildResult);
        Assert.Equal(initialBuildResult.RenderScene, coordinator.CurrentScene);
    }

    private static DevelopmentAstrologyChartCoordinator CreateCoordinator() =>
        CreateCoordinator(DevelopmentSampleChartBuildResultFactory.Create());

    private static DevelopmentAstrologyChartCoordinator CreateCoordinator(DevelopmentChartBuildResult initialBuildResult) =>
        CreateCoordinator(initialBuildResult, new FakeHouseCalculator());

    private static DevelopmentAstrologyChartCoordinator CreateCoordinator(
        DevelopmentChartBuildResult initialBuildResult,
        IHouseCalculator houseCalculator) =>
        new(
            new DevelopmentAstrologyChartPipeline(
                new TzdbBirthMomentResolver(),
                new DevelopmentEphemerisCalculator(),
                houseCalculator),
            initialBuildResult);

    private static BirthTimeAccuracyOption[] CreateAccuracies() =>
    [
        new(BirthTimeAccuracy.ExactTime, new LocalizationKey("ui.birth_data.time_accuracy.exact")),
        new(BirthTimeAccuracy.ApproximateTime, new LocalizationKey("ui.birth_data.time_accuracy.approximate")),
        new(BirthTimeAccuracy.UnknownTime, new LocalizationKey("ui.birth_data.time_accuracy.unknown"))
    ];

    private sealed class FakeHouseCalculator : IHouseCalculator
    {
        public int CallCount { get; private set; }

        public HouseCalculationResult Calculate(HouseCalculationRequest request)
        {
            CallCount++;
            var cusps = Enumerable.Range(1, 12)
                .Select(index => new HouseCusp(
                    new HouseNumber(index),
                    new ZodiacLongitude(15d + ((index - 1) * 30d))))
                .ToArray();
            var houses = NatalHouses.CreateAvailable(
                request.HouseSystem,
                cusps,
                new ChartAngles(new ZodiacLongitude(15d), new ZodiacLongitude(285d)),
                "test-house-fake");

            return HouseCalculationResult.Available(houses);
        }
    }

    private sealed class UnavailableHouseCalculator : IHouseCalculator
    {
        public HouseCalculationResult Calculate(HouseCalculationRequest request) =>
            HouseCalculationResult.Unavailable(
                request.HouseSystem,
                HouseCalculationFailureReason.UnsupportedGeographicConditions,
                "test-unavailable");
    }
}
