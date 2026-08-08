using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Shell;

namespace NoxAeterna.Tests.Presentation;

public sealed class WindowPlacementRepairCalculatorTests
{
    private static readonly WindowPlacementConstraints Constraints = new(1360, 860, 1180, 760);
    private static readonly WindowPlacementScreen Primary = new("PRIMARY", 0, 0, 1920, 1040, 1d, true);

    [Fact]
    public void Repair_FirstRunCentersDefaultSizeInsidePrimaryWorkArea()
    {
        var result = WindowPlacementRepairCalculator.Repair(null, [Primary], Constraints);

        Assert.Equal(280, result.X);
        Assert.Equal(90, result.Y);
        Assert.Equal(1360, result.Width);
        Assert.Equal(860, result.Height);
        Assert.Equal(1180, result.EffectiveMinimumWidth);
        Assert.Equal(760, result.EffectiveMinimumHeight);
        Assert.False(result.IsMaximized);
        Assert.Same(Primary, result.Screen);
    }

    [Fact]
    public void Repair_ValidNormalPlacementRestoresExactDipSizeAndPhysicalPosition()
    {
        var saved = Placement(100, 50, 1280, 800, screenId: "PRIMARY");

        var result = WindowPlacementRepairCalculator.Repair(saved, [Primary], Constraints);

        Assert.Equal(100, result.X);
        Assert.Equal(50, result.Y);
        Assert.Equal(1280, result.Width);
        Assert.Equal(800, result.Height);
    }

    [Fact]
    public void Repair_NegativeSecondaryMonitorCoordinatesRemainOnMatchingScreen()
    {
        var secondary = new WindowPlacementScreen("LEFT", -1920, 0, 1920, 1040, 1d, false);
        var saved = Placement(150, 70, 1200, 780, screenId: "LEFT", sourceX: -1920);

        var result = WindowPlacementRepairCalculator.Repair(saved, [Primary, secondary], Constraints);

        Assert.Same(secondary, result.Screen);
        Assert.Equal(-1770, result.X);
        Assert.Equal(70, result.Y);
    }

    [Fact]
    public void Repair_MonitorOrderChangeUsesStableScreenIdentity()
    {
        var target = new WindowPlacementScreen("TARGET", 1920, -200, 2560, 1400, 1.25, false);
        var saved = Placement(80, 40, 1200, 780, screenId: "TARGET");

        var result = WindowPlacementRepairCalculator.Repair(saved, [target, Primary], Constraints);

        Assert.Same(target, result.Screen);
        Assert.InRange(result.X, target.X, target.X + target.Width);
        Assert.InRange(result.Y, target.Y, target.Y + target.Height);
    }

    [Fact]
    public void Repair_RemovedMonitorFallsBackToPrimaryAndKeepsRelativePlacementVisible()
    {
        var saved = Placement(280, 90, 1360, 860, screenId: "REMOVED", sourceX: -1920);

        var result = WindowPlacementRepairCalculator.Repair(saved, [Primary], Constraints);

        Assert.Same(Primary, result.Screen);
        Assert.Equal(280, result.X);
        Assert.Equal(90, result.Y);
    }

    [Fact]
    public void Repair_OffScreenOffsetsClampToCurrentWorkArea()
    {
        var saved = Placement(50_000, -10_000, 1200, 780, screenId: "PRIMARY");

        var result = WindowPlacementRepairCalculator.Repair(saved, [Primary], Constraints);

        Assert.Equal(720, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(1920, result.X + result.Width);
        Assert.Equal(780, result.Y + result.Height);
    }

    [Theory]
    [InlineData(400, 300, 1180, 760)]
    [InlineData(10_000, 8_000, 1920, 1040)]
    public void Repair_StoredSizeClampsToMinimumAndCurrentWorkArea(
        double storedWidth,
        double storedHeight,
        double expectedWidth,
        double expectedHeight)
    {
        var saved = Placement(0, 0, storedWidth, storedHeight, screenId: "PRIMARY");

        var result = WindowPlacementRepairCalculator.Repair(saved, [Primary], Constraints);

        Assert.Equal(expectedWidth, result.Width);
        Assert.Equal(expectedHeight, result.Height);
        Assert.InRange(result.X, 0, 1920 - (int)expectedWidth);
        Assert.InRange(result.Y, 0, 1040 - (int)expectedHeight);
    }

    [Fact]
    public void Repair_DisplaySmallerThanNominalMinimumUsesRecoverableEffectiveMinimum()
    {
        var small = new WindowPlacementScreen("SMALL", 0, 0, 1000, 700, 1d, true);

        var result = WindowPlacementRepairCalculator.Repair(null, [small], Constraints);

        Assert.Equal(1000, result.Width);
        Assert.Equal(700, result.Height);
        Assert.Equal(1000, result.EffectiveMinimumWidth);
        Assert.Equal(700, result.EffectiveMinimumHeight);
        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
    }

    [Fact]
    public void Repair_ScalingChangePreservesDipSizeAndRelativeWorkAreaPosition()
    {
        var scaled = new WindowPlacementScreen("DISPLAY", 0, 0, 2560, 1600, 2d, true);
        var saved = Placement(360, 140, 1200, 760, screenId: "DISPLAY");

        var result = WindowPlacementRepairCalculator.Repair(saved, [scaled], Constraints);

        Assert.Equal(1200, result.Width);
        Assert.Equal(760, result.Height);
        Assert.Equal(80, result.X);
        Assert.Equal(40, result.Y);
    }

    [Fact]
    public void Repair_WorkAreaChangeMapsSavedRelativePositionIntoNewAvailableArea()
    {
        var changed = new WindowPlacementScreen("PRIMARY", 0, 0, 1600, 900, 1d, true);
        var saved = Placement(280, 90, 1360, 860, screenId: "PRIMARY");

        var result = WindowPlacementRepairCalculator.Repair(saved, [changed], Constraints);

        Assert.Equal(120, result.X);
        Assert.Equal(20, result.Y);
        Assert.Equal(1360, result.Width);
        Assert.Equal(860, result.Height);
    }

    [Fact]
    public void Repair_NonFiniteSavedGeometryFallsBackToSafeCenteredDefault()
    {
        var corrupt = Placement(double.NaN, 50, 1200, 780, screenId: "PRIMARY");

        var result = WindowPlacementRepairCalculator.Repair(corrupt, [Primary], Constraints);

        Assert.Equal(280, result.X);
        Assert.Equal(90, result.Y);
        Assert.Equal(1360, result.Width);
        Assert.Equal(860, result.Height);
        Assert.False(result.IsMaximized);
    }

    [Fact]
    public void Repair_ValidMaximizedPreferenceKeepsNormalRestoreBoundsAndStartupState()
    {
        var saved = Placement(90, 60, 1250, 790, isMaximized: true, screenId: "PRIMARY");

        var result = WindowPlacementRepairCalculator.Repair(saved, [Primary], Constraints);

        Assert.True(result.IsMaximized);
        Assert.Equal(90, result.X);
        Assert.Equal(60, result.Y);
        Assert.Equal(1250, result.Width);
        Assert.Equal(790, result.Height);
    }

    [Fact]
    public void Session_MinimizedFromNormalNeverBecomesPersistedStartupState()
    {
        var session = new WindowPlacementSession(Placement(100, 50, 1280, 800));

        session.ObserveState(WindowPlacementState.Minimized);

        Assert.False(session.CreatePreference().IsMaximized);
    }

    [Fact]
    public void Session_MinimizedFromMaximizedKeepsMaximizedAsLastMeaningfulState()
    {
        var normal = Placement(100, 50, 1280, 800);
        var session = new WindowPlacementSession(normal);

        session.ObserveState(WindowPlacementState.Maximized);
        session.ObserveState(WindowPlacementState.Minimized);

        Assert.Equal(normal with { IsMaximized = true }, session.CreatePreference());
    }

    [Fact]
    public void Session_MaximizedNeverOverwritesRememberedNormalRestoreBounds()
    {
        var first = Placement(100, 50, 1280, 800);
        var latestNormal = Placement(220, 140, 1220, 770);
        var session = new WindowPlacementSession(first);

        session.ObserveNormalPlacement(latestNormal);
        session.ObserveState(WindowPlacementState.Maximized);

        Assert.Equal(latestNormal with { IsMaximized = true }, session.CreatePreference());
    }

    private static WindowPlacementPreference Placement(
        double x,
        double y,
        double width,
        double height,
        bool isMaximized = false,
        string? screenId = null,
        int sourceX = 0) => new(
        x,
        y,
        width,
        height,
        isMaximized,
        screenId,
        sourceX,
        0,
        1920,
        1040,
        1d);
}
