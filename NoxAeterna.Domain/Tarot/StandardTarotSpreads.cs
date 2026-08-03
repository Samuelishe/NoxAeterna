namespace NoxAeterna.Domain.Tarot;

/// <summary>Provides built-in language-neutral Tarot spread definitions.</summary>
public static class StandardTarotSpreads
{
    /// <summary>Gets the single-card spread.</summary>
    public static TarotSpreadDefinition SingleCard { get; } = new(
        new TarotSpreadId("single-card"),
        [new TarotSpreadPositionDefinition(new TarotSpreadPositionId("card"))]);

    /// <summary>Gets the ordered past, present, and future three-card spread.</summary>
    public static TarotSpreadDefinition ThreeCards { get; } = new(
        new TarotSpreadId("three-cards"),
        [
            new TarotSpreadPositionDefinition(new TarotSpreadPositionId("past")),
            new TarotSpreadPositionDefinition(new TarotSpreadPositionId("present")),
            new TarotSpreadPositionDefinition(new TarotSpreadPositionId("future"))
        ]);
}
