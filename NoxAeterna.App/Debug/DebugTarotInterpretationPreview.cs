#if DEBUG
using NoxAeterna.App.Tarot;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Interpretation.Tarot.Validation;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.App.Debug;

/// <summary>Opt-in synthetic resolved content for real-control presentation smoke only.</summary>
internal sealed class DebugTarotInterpretationPreview :
    ITarotWorkspaceInterpretationResolver,
    ITarotSingleCardPresentationLabelSource
{
    internal const string EnvironmentVariableName = "NOXAETERNA_DEBUG_INTERPRETATION_PREVIEW";
    private readonly bool noContentInEnglish;

    private DebugTarotInterpretationPreview(bool noContentInEnglish) =>
        this.noContentInEnglish = noContentInEnglish;

    internal static DebugTarotInterpretationPreview? TryCreate()
    {
        var value = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        return value switch
        {
            null or "" => null,
            "resolved" => new(false),
            "resolved-then-none" => new(true),
            _ => throw new InvalidOperationException(
                $"{EnvironmentVariableName} must be 'resolved' or 'resolved-then-none'.")
        };
    }

    public TarotInterpretationResolution<TarotSingleCardEntry> ResolveSingleCard(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotCardId cardId,
        TarotCardOrientation orientation)
    {
        if (noContentInEnglish && requestedLocale.Value == "en")
        {
            return new NoTarotInterpretationContent<TarotSingleCardEntry>(TarotNoContentReason.NoReadyLocale);
        }

        var locale = requestedLocale.Value == "ru" ? new TarotInterpretationLocale("ru") : new TarotInterpretationLocale("en");
        var document = CreateDocument(locale, cardId, orientation);
        var validation = TarotInterpretationValidator.ValidateSingleCard(document, StandardTarotCatalog.Deck);
        if (!validation.IsValid || validation.Value is null)
        {
            return new NoTarotInterpretationContent<TarotSingleCardEntry>(
                TarotNoContentReason.ValidationFailed,
                new TarotResolutionDiagnostic("debug.preview.invalid", "The DEBUG preview fixture is invalid."));
        }

        return new ResolvedTarotInterpretation<TarotSingleCardEntry>(
            packId,
            1,
            TarotInterpretationMode.SingleCard,
            requestedLocale,
            locale,
            validation.Value);
    }

    public TarotInterpretationResolution<TarotThreeCardPositionEntry> ResolveThreeCardPosition(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotThreeCardPosition position,
        TarotCardId cardId,
        TarotCardOrientation orientation) =>
        new NoTarotInterpretationContent<TarotThreeCardPositionEntry>(TarotNoContentReason.NoReadyLocale);

    public TarotSingleCardInterpretationLabels? Resolve(
        TarotInterpretationPackId packId,
        int contentVersion,
        TarotInterpretationLocale resolvedLocale)
    {
        var russian = resolvedLocale.Value == "ru";
        var sectionLabels = russian
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["situation"] = "Основная ситуация",
                ["development"] = "Развитие",
                ["risk"] = "Риск",
                ["outcome"] = "Возможный исход",
                ["advice"] = "Совет"
            }
            : new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["situation"] = "Core situation",
                ["development"] = "Development",
                ["risk"] = "Risk",
                ["outcome"] = "Possible outcome",
                ["advice"] = "Advice"
            };
        var tagLabels = (russian
                ? new[] { "Конфликт", "Выбор", "Перемены", "Возможность", "Риск", "Освобождение" }
                : new[] { "Conflict", "Choice", "Change", "Opportunity", "Risk", "Release" })
            .Select((label, index) => (Id: TagIds[index], Label: label))
            .ToDictionary(static item => item.Id, static item => item.Label);
        return new TarotSingleCardInterpretationLabels(sectionLabels, tagLabels);
    }

    private static TarotSingleCardDocument CreateDocument(
        TarotInterpretationLocale locale,
        TarotCardId cardId,
        TarotCardOrientation orientation)
    {
        var russian = locale.Value == "ru";
        var reversed = orientation == TarotCardOrientation.Reversed;
        var orientationText = russian
            ? reversed ? "Перевёрнутая тестовая карта" : "Прямая тестовая карта"
            : reversed ? "Reversed test card" : "Upright test card";
        string Text(string subject) => russian
            ? $"{orientationText}: {subject}. Это намеренно длинный синтетический текст для проверки переноса строк и общего вертикального прокручивания; он не является толкованием Classic и не входит в production corpus."
            : $"{orientationText}: {subject}. This deliberately long synthetic text verifies wrapping and the shared vertical scroll surface; it is not Classic meaning and is not part of the production corpus.";

        return new TarotSingleCardDocument
        {
            SchemaVersion = 1,
            CardId = cardId.Value,
            Orientation = orientation,
            Sections = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["situation"] = Text(russian ? "ситуация для визуального smoke" : "visual-smoke situation"),
                ["development"] = Text(russian ? "развитие для визуального smoke" : "visual-smoke development"),
                ["risk"] = Text(russian ? "риск для визуального smoke" : "visual-smoke risk"),
                ["outcome"] = Text(russian ? "исход для визуального smoke" : "visual-smoke outcome"),
                ["advice"] = Text(russian ? "совет для визуального smoke" : "visual-smoke advice")
            },
            Tags = TagIds.Select((id, index) => (TarotTagAssignmentDocument?)new TarotTagAssignmentDocument
            {
                ConceptId = id.Value,
                Valence = Valences[index],
                Intensity = Intensities[index]
            }).ToList(),
            OverallValence = reversed ? -1 : 1,
            OverallIntensity = 3,
            ReversalMechanisms = reversed ? [TarotReversalMechanism.Blocked] : []
        };
    }

    private static readonly TarotTagConceptId[] TagIds =
    [
        new("conflict"), new("choice"), new("change"),
        new("opportunity"), new("risk"), new("release")
    ];

    private static readonly int[] Valences = [-2, -1, 0, 1, 2, 2];
    private static readonly int[] Intensities = [1, 2, 3, 1, 2, 3];
}
#endif
