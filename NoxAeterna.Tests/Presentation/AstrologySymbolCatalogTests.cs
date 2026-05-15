using NoxAeterna.Domain.Astrology;
using NoxAeterna.Presentation.Astrology;

namespace NoxAeterna.Tests.Presentation;

public sealed class AstrologySymbolCatalogTests
{
    [Fact]
    public void BodyMappings_AreDefinedForAllSupportedBodies()
    {
        foreach (var body in Enum.GetValues<CelestialBody>())
        {
            Assert.False(string.IsNullOrWhiteSpace(AstrologySymbolCatalog.GetBodyGlyph(body)));
            Assert.False(string.IsNullOrWhiteSpace(AstrologySymbolCatalog.GetBodyLabelKey(body).Value));
        }
    }

    [Fact]
    public void SignMappings_AreDefinedForAllSigns()
    {
        foreach (var sign in Enum.GetValues<ZodiacSign>())
        {
            Assert.False(string.IsNullOrWhiteSpace(AstrologySymbolCatalog.GetSignGlyph(sign)));
            Assert.False(string.IsNullOrWhiteSpace(AstrologySymbolCatalog.GetSignLabelKey(sign).Value));
        }
    }
}
