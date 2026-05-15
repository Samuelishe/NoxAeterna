using System.Globalization;
using NoxAeterna.App.Localization;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Tests.App;

public sealed class ApplicationCultureControllerTests
{
    [Theory]
    [InlineData("ru", "ru-RU")]
    [InlineData("en", "en-US")]
    [InlineData("en-us", "en-US")]
    public void ResolveCulture_ReturnsDeterministicUiCulture(string languageCode, string expectedCultureName)
    {
        var culture = ApplicationCultureController.ResolveCulture(new LanguageCode(languageCode));

        Assert.Equal(expectedCultureName, culture.Name);
    }

    [Fact]
    public void Apply_UpdatesDefaultThreadCultures()
    {
        var originalCulture = CultureInfo.DefaultThreadCurrentCulture;
        var originalUiCulture = CultureInfo.DefaultThreadCurrentUICulture;

        try
        {
            ApplicationCultureController.Apply(new LanguageCode("ru"));

            Assert.Equal("ru-RU", CultureInfo.DefaultThreadCurrentCulture?.Name);
            Assert.Equal("ru-RU", CultureInfo.DefaultThreadCurrentUICulture?.Name);
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentCulture = originalCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalUiCulture;
        }
    }
}
