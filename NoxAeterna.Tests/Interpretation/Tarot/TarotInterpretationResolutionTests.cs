using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotInterpretationResolutionTests
{
    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    [InlineData("zh")]
    [InlineData("zh-hans")]
    [InlineData("sr-latn-rs")]
    public void Locale_AcceptsNarrowLowercaseAsciiTags(string value)
    {
        var locale = new TarotInterpretationLocale(value);

        Assert.Equal(value, locale.Value);
        Assert.Equal(value, locale.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("RU")]
    [InlineData("en_US")]
    [InlineData(" en")]
    [InlineData("en-")]
    [InlineData("e")]
    [InlineData("рус")]
    public void Locale_RejectsValuesOutsideFrozenNarrowContract(string value)
    {
        Assert.Throws<ArgumentException>(() => new TarotInterpretationLocale(value));
    }

    [Fact]
    public void Resolved_RetainsPackVersionModeLocalesAndStructuredContent()
    {
        var content = new object();
        var result = new ResolvedTarotInterpretation<object>(
            new TarotInterpretationPackId("classic"),
            3,
            TarotInterpretationMode.SingleCard,
            new TarotInterpretationLocale("zh"),
            new TarotInterpretationLocale("en"),
            content);

        Assert.Equal("classic", result.PackId.Value);
        Assert.Equal(3, result.ContentVersion);
        Assert.Equal(TarotInterpretationMode.SingleCard, result.ModeId);
        Assert.Equal("zh", result.RequestedLocale.Value);
        Assert.Equal("en", result.ResolvedLocale.Value);
        Assert.Same(content, result.Content);
        Assert.IsAssignableFrom<TarotInterpretationResolution<object>>(result);
    }

    [Fact]
    public void Resolved_RejectsNonPositiveVersionNullContentAndUnknownMode()
    {
        var pack = new TarotInterpretationPackId("classic");
        var locale = new TarotInterpretationLocale("ru");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ResolvedTarotInterpretation<object>(pack, 0, TarotInterpretationMode.SingleCard, locale, locale, new()));
        Assert.Throws<ArgumentNullException>(() =>
            new ResolvedTarotInterpretation<object>(pack, 1, TarotInterpretationMode.SingleCard, locale, locale, null!));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ResolvedTarotInterpretation<object>(pack, 1, (TarotInterpretationMode)99, locale, locale, new()));
    }

    [Fact]
    public void NoContent_UsesClosedReasonAndOptionalInternalDiagnosticWithoutFakeContent()
    {
        var diagnostic = new TarotResolutionDiagnostic("manifest.index", "Index validation failed.");
        var withDiagnostic = new NoTarotInterpretationContent<object>(
            TarotNoContentReason.BrokenReadyModule,
            diagnostic);
        var withoutDiagnostic = new NoTarotInterpretationContent<object>(TarotNoContentReason.NoReadyLocale);

        Assert.Equal(TarotNoContentReason.BrokenReadyModule, withDiagnostic.Reason);
        Assert.Same(diagnostic, withDiagnostic.Diagnostic);
        Assert.Null(withoutDiagnostic.Diagnostic);
        Assert.Null(typeof(NoTarotInterpretationContent<object>).GetProperty("Content"));
        Assert.IsAssignableFrom<TarotInterpretationResolution<object>>(withDiagnostic);
        Assert.Throws<ArgumentOutOfRangeException>(() => new NoTarotInterpretationContent<object>((TarotNoContentReason)99));
    }
}
