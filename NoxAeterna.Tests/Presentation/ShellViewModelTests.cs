using NoxAeterna.Presentation.Shell;

namespace NoxAeterna.Tests.Presentation;

public sealed class ShellViewModelTests
{
    [Fact]
    public void CreateDefault_UsesDeterministicDefaultSection()
    {
        var shell = ShellViewModel.CreateDefault();

        Assert.Equal(ShellSectionId.Astrology, shell.SelectedSectionId);
        Assert.True(shell.NavigationState.IsExpanded);
        Assert.True(shell.NavigationState.WideModeExpandedPreference);
        Assert.False(shell.NavigationState.IsCompactViewport);
    }

    [Fact]
    public void CreateDefault_ExposesExpectedFutureSections()
    {
        var shell = ShellViewModel.CreateDefault();

        Assert.Equal(
            new[]
            {
                ShellSectionId.Astrology,
                ShellSectionId.Tarot,
                ShellSectionId.Archive,
                ShellSectionId.Settings
            },
            shell.NavigationItems.Select(item => item.Id));
        Assert.Equal(
            new[]
            {
                ShellNavigationIconId.Astrology,
                ShellNavigationIconId.Tarot,
                ShellNavigationIconId.Archive,
                ShellNavigationIconId.Settings
            },
            shell.NavigationItems.Select(item => item.IconId));
    }

    [Fact]
    public void NavigationItems_AreLocalizationKeyBased()
    {
        var shell = ShellViewModel.CreateDefault();

        Assert.All(shell.NavigationItems, item => Assert.False(string.IsNullOrWhiteSpace(item.LabelKey.Value)));
    }

    [Fact]
    public void CreateDefault_DoesNotExposeTemporaryNavigationItems()
    {
        var shell = ShellViewModel.CreateDefault();

        Assert.DoesNotContain(shell.NavigationItems, item => item.IsTemporary);
    }

    [Fact]
    public void TarotOmitsRedundantShellHeaderWhileOtherSectionsRetainIt()
    {
        var shell = ShellViewModel.CreateDefault();

        Assert.False(shell.NavigationItems.Single(item => item.Id == ShellSectionId.Tarot).ShowHeader);
        Assert.All(
            shell.NavigationItems.Where(item => item.Id != ShellSectionId.Tarot),
            item => Assert.True(item.ShowHeader));
    }

    [Fact]
    public void NavigationToggleChangesWidePreferenceWithoutChangingSelectedSection()
    {
        var shell = ShellViewModel.CreateDefault();
        shell.SelectedSectionId = ShellSectionId.Tarot;

        shell.NavigationState.Toggle();

        Assert.False(shell.NavigationState.IsExpanded);
        Assert.False(shell.NavigationState.WideModeExpandedPreference);
        Assert.Equal(ShellSectionId.Tarot, shell.SelectedSectionId);

        shell.NavigationState.Toggle();

        Assert.True(shell.NavigationState.IsExpanded);
        Assert.True(shell.NavigationState.WideModeExpandedPreference);
        Assert.Equal(ShellSectionId.Tarot, shell.SelectedSectionId);
    }

    [Fact]
    public void CompactViewportForcesCollapseAndRestoresExpandedWidePreference()
    {
        var state = new ShellNavigationState();

        state.UpdateViewportWidth(ShellNavigationLayout.CompactViewportThreshold - 1d);

        Assert.True(state.IsCompactViewport);
        Assert.False(state.IsExpanded);
        Assert.True(state.WideModeExpandedPreference);

        state.Toggle();
        state.UpdateViewportWidth(ShellNavigationLayout.CompactViewportThreshold);

        Assert.False(state.IsCompactViewport);
        Assert.True(state.IsExpanded);
        Assert.True(state.WideModeExpandedPreference);
    }

    [Fact]
    public void CompactViewportPreservesCollapsedWidePreference()
    {
        var state = new ShellNavigationState();
        state.Toggle();

        state.UpdateViewportWidth(ShellNavigationLayout.CompactViewportThreshold - 1d);
        state.UpdateViewportWidth(ShellNavigationLayout.CompactViewportThreshold + 1d);

        Assert.False(state.IsCompactViewport);
        Assert.False(state.IsExpanded);
        Assert.False(state.WideModeExpandedPreference);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-1d)]
    public void NavigationViewportRejectsInvalidWidths(double width)
    {
        var state = new ShellNavigationState();

        Assert.Throws<ArgumentOutOfRangeException>(() => state.UpdateViewportWidth(width));
    }
}
