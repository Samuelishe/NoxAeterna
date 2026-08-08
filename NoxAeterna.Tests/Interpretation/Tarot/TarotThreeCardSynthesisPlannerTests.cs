using NoxAeterna.Interpretation.Tarot.Contracts;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotThreeCardSynthesisPlannerTests
{
    [Fact]
    public void ExhaustiveStructuredInputSpaceAlwaysResolvesDeterministicallyToReachableInventory()
    {
        var inventory = TarotThreeCardSynthesisContract.RequiredResources.ToHashSet();
        var reached = new HashSet<TarotSynthesisResourceIdentity>();
        var inputs = 0;

        foreach (var past in Enumerable.Range(-2, 5))
        foreach (var present in Enumerable.Range(-2, 5))
        foreach (var future in Enumerable.Range(-2, 5))
        foreach (var pastPresent in Enumerable.Range(-2, 5))
        foreach (var presentFuture in Enumerable.Range(-2, 5))
        foreach (var pastPresentIntensity in Enumerable.Range(1, 3))
        foreach (var presentFutureIntensity in Enumerable.Range(1, 3))
        {
            var input = new TarotThreeCardSynthesisInput(
                past, present, future,
                pastPresent, pastPresentIntensity,
                presentFuture, presentFutureIntensity);
            var first = TarotThreeCardSynthesisPlanner.Plan(input);
            var second = TarotThreeCardSynthesisPlanner.Plan(input);

            Assert.Equal(first, second);
            var profile = new TarotSynthesisResourceIdentity(TarotSynthesisResourceType.TrajectoryProfile, first.TrajectoryProfileId);
            var fragment = new TarotSynthesisResourceIdentity(TarotSynthesisResourceType.SynthesisFragment, first.SynthesisFragmentId);
            Assert.Contains(profile, inventory);
            Assert.Contains(fragment, inventory);
            reached.Add(profile);
            reached.Add(fragment);
            inputs++;
        }

        Assert.Equal(28_125, inputs);
        Assert.True(inventory.SetEquals(reached));
    }

    [Theory]
    [InlineData(-2, 0, 2, 1, 2, 1, 2, TarotThreeCardSynthesisContract.Improving, TarotThreeCardSynthesisContract.MutuallySupportive)]
    [InlineData(2, 0, -2, -1, 2, -1, 2, TarotThreeCardSynthesisContract.Deteriorating, TarotThreeCardSynthesisContract.MutuallyConflicted)]
    [InlineData(-1, 2, -1, -1, 2, 1, 2, TarotThreeCardSynthesisContract.TurningPoint, TarotThreeCardSynthesisContract.TensionEases)]
    [InlineData(1, 1, 1, 1, 1, -1, 3, TarotThreeCardSynthesisContract.ConstructiveContinuity, TarotThreeCardSynthesisContract.UnevenInfluence)]
    [InlineData(0, 0, 0, 0, 2, 0, 2, TarotThreeCardSynthesisContract.NeutralContinuity, TarotThreeCardSynthesisContract.MixedTransitions)]
    public void RepresentativePlansExposeInterpretableExplicitRules(
        int past, int present, int future,
        int pastPresent, int pastPresentIntensity,
        int presentFuture, int presentFutureIntensity,
        string expectedProfile, string expectedFragment)
    {
        var plan = TarotThreeCardSynthesisPlanner.Plan(new(
            past, present, future,
            pastPresent, pastPresentIntensity,
            presentFuture, presentFutureIntensity));

        Assert.Equal(expectedProfile, plan.TrajectoryProfileId.Value);
        Assert.Equal(expectedFragment, plan.SynthesisFragmentId.Value);
    }

    [Theory]
    [InlineData(-3, 0, 0, 0, 2, 0, 2)]
    [InlineData(0, 0, 3, 0, 2, 0, 2)]
    [InlineData(0, 0, 0, 0, 0, 0, 2)]
    [InlineData(0, 0, 0, 0, 2, 0, 4)]
    public void InvalidMetricInputIsRejected(
        int past, int present, int future,
        int pastPresent, int pastPresentIntensity,
        int presentFuture, int presentFutureIntensity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TarotThreeCardSynthesisPlanner.Plan(new(
            past, present, future,
            pastPresent, pastPresentIntensity,
            presentFuture, presentFutureIntensity)));
    }
}
