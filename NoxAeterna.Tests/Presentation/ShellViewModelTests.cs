using NoxAeterna.Presentation.Shell;

namespace NoxAeterna.Tests.Presentation;

public sealed class ShellViewModelTests
{
    [Fact]
    public void CreateDefault_UsesDeterministicDefaultSection()
    {
        var shell = ShellViewModel.CreateDefault();

        Assert.Equal(ShellSectionId.Astrology, shell.SelectedSectionId);
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
}
