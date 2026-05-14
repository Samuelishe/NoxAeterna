using NoxAeterna.Presentation.Shell;

namespace NoxAeterna.Tests.Presentation;

public sealed class ShellViewModelTests
{
    [Fact]
    public void CreateDefault_UsesDeterministicDefaultSection()
    {
        var shell = ShellViewModel.CreateDefault();

        Assert.Equal(ShellSectionId.DebugPreview, shell.SelectedSectionId);
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
                ShellSectionId.Settings,
                ShellSectionId.DebugPreview
            },
            shell.NavigationItems.Select(item => item.Id));
    }

    [Fact]
    public void NavigationItems_AreLocalizationKeyBased()
    {
        var shell = ShellViewModel.CreateDefault();

        Assert.All(shell.NavigationItems, item => Assert.False(string.IsNullOrWhiteSpace(item.LabelKey.Value)));
    }

    [Fact]
    public void DebugPreviewItem_RemainsClearlyTemporary()
    {
        var shell = ShellViewModel.CreateDefault();

        var debugItem = Assert.Single(shell.NavigationItems, item => item.Id == ShellSectionId.DebugPreview);

        Assert.True(debugItem.IsTemporary);
    }
}
