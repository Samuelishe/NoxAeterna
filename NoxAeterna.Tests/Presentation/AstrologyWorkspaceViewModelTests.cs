using NoxAeterna.Presentation.Astrology;

namespace NoxAeterna.Tests.Presentation;

public sealed class AstrologyWorkspaceViewModelTests
{
    [Fact]
    public void CreateFoundation_UsesDeterministicPanelOrder()
    {
        var workspace = AstrologyWorkspaceViewModel.CreateFoundation();

        Assert.Equal(
            new[]
            {
                AstrologyWorkspacePanelId.Chart,
                AstrologyWorkspacePanelId.BirthData,
                AstrologyWorkspacePanelId.Interpretation
            },
            workspace.Panels.Select(panel => panel.Id));
    }

    [Fact]
    public void CreateFoundation_ExposesLocalizationKeyBasedPanels()
    {
        var workspace = AstrologyWorkspaceViewModel.CreateFoundation();

        Assert.NotNull(workspace.BirthDataInput);
        Assert.All(workspace.Panels, panel =>
        {
            Assert.False(string.IsNullOrWhiteSpace(panel.TitleKey.Value));
        });
    }
}
