using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.Tests.Presentation;

public sealed class ThemeRegistryTests
{
    [Fact]
    public void CreateDefault_ExposesExpectedThemes()
    {
        var registry = ThemeRegistry.CreateDefault();

        Assert.Equal(
            new[] { new ThemeId("dark"), new ThemeId("light") },
            registry.Themes.Select(theme => theme.Id));
    }
}
