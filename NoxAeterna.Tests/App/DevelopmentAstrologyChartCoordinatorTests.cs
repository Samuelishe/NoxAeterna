using NodaTime;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Astronomy.Time;
using NoxAeterna.App.Astrology;
using NoxAeterna.App.Samples;
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
    }

    [Fact]
    public void TryBuild_UsesTechnicalFallbackForUnknownTimeWhilePreservingAccuracy()
    {
        var coordinator = CreateCoordinator();
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
        new(
            new DevelopmentAstrologyChartPipeline(
                new TzdbBirthMomentResolver(),
                new DevelopmentEphemerisCalculator()),
            initialBuildResult);

    private static BirthTimeAccuracyOption[] CreateAccuracies() =>
    [
        new(BirthTimeAccuracy.ExactTime, new LocalizationKey("ui.birth_data.time_accuracy.exact")),
        new(BirthTimeAccuracy.ApproximateTime, new LocalizationKey("ui.birth_data.time_accuracy.approximate")),
        new(BirthTimeAccuracy.UnknownTime, new LocalizationKey("ui.birth_data.time_accuracy.unknown"))
    ];
}
