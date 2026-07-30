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

    [Theory]
    [InlineData("ru", "день", "месяц", "год", "час", "минута")]
    [InlineData("en", "day", "month", "year", "hour", "minute")]
    public void ResolvePickerSegmentLabels_ReturnsApplicationLanguageText(
        string languageCode,
        string day,
        string month,
        string year,
        string hour,
        string minute)
    {
        var labels = ApplicationCultureController.ResolvePickerSegmentLabels(new LanguageCode(languageCode));

        Assert.Equal(day, labels.Day);
        Assert.Equal(month, labels.Month);
        Assert.Equal(year, labels.Year);
        Assert.Equal(hour, labels.Hour);
        Assert.Equal(minute, labels.Minute);
    }
}
