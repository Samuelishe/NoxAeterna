using System.Collections.ObjectModel;

namespace NoxAeterna.Interpretation.Tarot.Contracts;

/// <summary>Identifies one exact production-owned three-card synthesis resource.</summary>
public sealed record TarotSynthesisResourceIdentity(
    TarotSynthesisResourceType ResourceType,
    TarotSynthesisResourceId ResourceId);

/// <summary>Owns the finite production inventory for deterministic Three Cards synthesis.</summary>
public static class TarotThreeCardSynthesisContract
{
    public const string Improving = "improving";
    public const string Deteriorating = "deteriorating";
    public const string ConstructiveContinuity = "constructive-continuity";
    public const string DifficultContinuity = "difficult-continuity";
    public const string TurningPoint = "turning-point";
    public const string Unsettled = "unsettled";
    public const string NeutralContinuity = "neutral-continuity";

    public const string MutuallySupportive = "mutually-supportive";
    public const string MutuallyConflicted = "mutually-conflicted";
    public const string TensionEases = "tension-eases";
    public const string TensionEmerges = "tension-emerges";
    public const string UnevenInfluence = "uneven-influence";
    public const string MixedTransitions = "mixed-transitions";

    private static readonly IReadOnlyList<TarotSynthesisResourceIdentity> Inventory =
        Array.AsReadOnly(
        [
            Profile(ConstructiveContinuity),
            Profile(Deteriorating),
            Profile(DifficultContinuity),
            Profile(Improving),
            Profile(NeutralContinuity),
            Profile(TurningPoint),
            Profile(Unsettled),
            Fragment(MixedTransitions),
            Fragment(MutuallyConflicted),
            Fragment(MutuallySupportive),
            Fragment(TensionEases),
            Fragment(TensionEmerges),
            Fragment(UnevenInfluence)
        ]);

    private static readonly IReadOnlySet<TarotSynthesisResourceIdentity> InventorySet =
        new ReadOnlySet<TarotSynthesisResourceIdentity>(Inventory.ToHashSet());

    public static IReadOnlyList<TarotSynthesisResourceIdentity> RequiredResources => Inventory;

    public static bool IsRequired(TarotSynthesisResourceType resourceType, TarotSynthesisResourceId resourceId) =>
        InventorySet.Contains(new(resourceType, resourceId));

    private static TarotSynthesisResourceIdentity Profile(string id) =>
        new(TarotSynthesisResourceType.TrajectoryProfile, new(id));

    private static TarotSynthesisResourceIdentity Fragment(string id) =>
        new(TarotSynthesisResourceType.SynthesisFragment, new(id));
}

/// <summary>Structured metrics resolved before locale-owned synthesis resources are selected.</summary>
public sealed record TarotThreeCardSynthesisInput(
    int PastValence,
    int PresentValence,
    int FutureValence,
    int PastPresentRelationValence,
    int PastPresentRelationIntensity,
    int PresentFutureRelationValence,
    int PresentFutureRelationIntensity);

/// <summary>Exact pair of locale-owned resources needed to render an Overall block.</summary>
public sealed record TarotThreeCardSynthesisPlan(
    TarotSynthesisResourceId TrajectoryProfileId,
    TarotSynthesisResourceId SynthesisFragmentId);

/// <summary>Selects finite synthesis resources from structured valence and intensity buckets only.</summary>
public static class TarotThreeCardSynthesisPlanner
{
    public static TarotThreeCardSynthesisPlan Plan(TarotThreeCardSynthesisInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var past = Polarity(input.PastValence, nameof(input.PastValence));
        var present = Polarity(input.PresentValence, nameof(input.PresentValence));
        var future = Polarity(input.FutureValence, nameof(input.FutureValence));
        var pastPresent = Polarity(input.PastPresentRelationValence, nameof(input.PastPresentRelationValence));
        var presentFuture = Polarity(input.PresentFutureRelationValence, nameof(input.PresentFutureRelationValence));
        ValidateIntensity(input.PastPresentRelationIntensity, nameof(input.PastPresentRelationIntensity));
        ValidateIntensity(input.PresentFutureRelationIntensity, nameof(input.PresentFutureRelationIntensity));

        return new(
            new(Trajectory(past, present, future)),
            new(Fragment(
                pastPresent,
                input.PastPresentRelationIntensity,
                presentFuture,
                input.PresentFutureRelationIntensity)));
    }

    private static string Trajectory(PolarityBucket past, PolarityBucket present, PolarityBucket future)
    {
        if (past == present && present == future)
        {
            return future switch
            {
                PolarityBucket.Positive => TarotThreeCardSynthesisContract.ConstructiveContinuity,
                PolarityBucket.Negative => TarotThreeCardSynthesisContract.DifficultContinuity,
                _ => TarotThreeCardSynthesisContract.NeutralContinuity
            };
        }

        if (past == future && present != past)
        {
            return TarotThreeCardSynthesisContract.TurningPoint;
        }

        if ((past == PolarityBucket.Negative && future == PolarityBucket.Positive) ||
            (past == PolarityBucket.Neutral && future == PolarityBucket.Positive && present != PolarityBucket.Negative) ||
            (past == PolarityBucket.Negative && future == PolarityBucket.Neutral && present != PolarityBucket.Positive))
        {
            return TarotThreeCardSynthesisContract.Improving;
        }

        if ((past == PolarityBucket.Positive && future == PolarityBucket.Negative) ||
            (past == PolarityBucket.Neutral && future == PolarityBucket.Negative && present != PolarityBucket.Positive) ||
            (past == PolarityBucket.Positive && future == PolarityBucket.Neutral && present != PolarityBucket.Negative))
        {
            return TarotThreeCardSynthesisContract.Deteriorating;
        }

        return TarotThreeCardSynthesisContract.Unsettled;
    }

    private static string Fragment(
        PolarityBucket pastPresent,
        int pastPresentIntensity,
        PolarityBucket presentFuture,
        int presentFutureIntensity)
    {
        if (IsMateriallyUneven(pastPresentIntensity, presentFutureIntensity))
        {
            return TarotThreeCardSynthesisContract.UnevenInfluence;
        }

        return (pastPresent, presentFuture) switch
        {
            (PolarityBucket.Positive, PolarityBucket.Positive) => TarotThreeCardSynthesisContract.MutuallySupportive,
            (PolarityBucket.Negative, PolarityBucket.Negative) => TarotThreeCardSynthesisContract.MutuallyConflicted,
            (PolarityBucket.Negative, PolarityBucket.Positive) => TarotThreeCardSynthesisContract.TensionEases,
            (PolarityBucket.Positive, PolarityBucket.Negative) => TarotThreeCardSynthesisContract.TensionEmerges,
            _ => TarotThreeCardSynthesisContract.MixedTransitions
        };
    }

    private static bool IsMateriallyUneven(int first, int second) =>
        (first == 1 && second == 3) || (first == 3 && second == 1);

    private static PolarityBucket Polarity(int value, string parameterName) => value switch
    {
        -2 or -1 => PolarityBucket.Negative,
        0 => PolarityBucket.Neutral,
        1 or 2 => PolarityBucket.Positive,
        _ => throw new ArgumentOutOfRangeException(parameterName, value, "Valence must be in -2..2.")
    };

    private static void ValidateIntensity(int value, string parameterName)
    {
        if (value is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Intensity must be in 1..3.");
        }
    }

    private enum PolarityBucket
    {
        Negative,
        Neutral,
        Positive
    }
}
