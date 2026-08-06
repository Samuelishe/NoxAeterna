using NodaTime;
using NoxAeterna.App.Preferences;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Tarot;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.Tests.App;

public sealed class UserPreferencesCoordinatorTests
{
    [Fact]
    public void Initialization_DoesNotWriteLoadedPreferences()
    {
        var store = new RecordingStore();
        var loaded = CreatePreferences();

        var coordinator = new UserPreferencesCoordinator(store, new UserPreferencesLoadResult(loaded, null));

        Assert.Same(loaded, coordinator.Current);
        Assert.Null(coordinator.LastDiagnostic);
        Assert.Empty(store.SavedPreferences);
    }

    [Fact]
    public void ApplicationPreferenceChange_MergesCurrentTarotPreferencesAndWritesOnce()
    {
        var store = new RecordingStore();
        var initial = CreatePreferences();
        var coordinator = CreateCoordinator(store, initial);
        var sourceWithStaleTarot = initial with
        {
            ApplicationLanguage = new ApplicationLanguagePreference(new LanguageCode("en")),
            InterpretationLanguage = new InterpretationLanguagePreference(new LanguageCode("en")),
            ThemeId = new ThemeId("light"),
            Tarot = TarotWorkspacePreferences.CreateDefault()
        };

        var changed = coordinator.ApplyApplicationPreferences(sourceWithStaleTarot);

        Assert.True(changed);
        var saved = Assert.Single(store.SavedPreferences);
        Assert.Equal(new LanguageCode("en"), saved.ApplicationLanguage.Language);
        Assert.Equal(new LanguageCode("en"), saved.InterpretationLanguage.Language);
        Assert.Equal(new ThemeId("light"), saved.ThemeId);
        Assert.Same(initial.Tarot, saved.Tarot);
        Assert.Equal(saved, coordinator.Current);
    }

    [Fact]
    public void TarotPreferenceChange_MergesCurrentApplicationPreferencesAndWritesOnce()
    {
        var store = new RecordingStore();
        var initial = CreatePreferences();
        var coordinator = CreateCoordinator(store, initial);
        var tarot = initial.Tarot with { AutoRevealCards = true, AllowReversed = false };

        var changed = coordinator.ApplyTarotPreferences(tarot);

        Assert.True(changed);
        var saved = Assert.Single(store.SavedPreferences);
        Assert.Equal(initial.ApplicationLanguage, saved.ApplicationLanguage);
        Assert.Equal(initial.InterpretationLanguage, saved.InterpretationLanguage);
        Assert.Equal(initial.ThemeId, saved.ThemeId);
        Assert.Same(tarot, saved.Tarot);
        Assert.Equal(saved, coordinator.Current);
    }

    [Fact]
    public void SettingSamePreference_DoesNotWriteOrPublishChange()
    {
        var store = new RecordingStore();
        var initial = CreatePreferences();
        var coordinator = CreateCoordinator(store, initial);
        var eventCount = 0;
        coordinator.PreferencesChanged += (_, _) => eventCount++;

        var applicationChanged = coordinator.ApplyApplicationPreferences(initial);
        var tarotChanged = coordinator.ApplyTarotPreferences(initial.Tarot);

        Assert.False(applicationChanged);
        Assert.False(tarotChanged);
        Assert.Empty(store.SavedPreferences);
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void EachActualPreferenceChange_WritesExactlyOneCompleteSnapshot()
    {
        var store = new RecordingStore();
        var coordinator = CreateCoordinator(store, UserPreferencesDefaults.Create());
        var eventCount = 0;
        coordinator.PreferencesChanged += (_, _) => eventCount++;

        ApplyApplication(coordinator, preferences => preferences with
        {
            ApplicationLanguage = new ApplicationLanguagePreference(new LanguageCode("en"))
        });
        ApplyApplication(coordinator, preferences => preferences with
        {
            InterpretationLanguage = new InterpretationLanguagePreference(new LanguageCode("en"))
        });
        ApplyApplication(coordinator, preferences => preferences with { ThemeId = new ThemeId("light") });
        ApplyTarot(coordinator, tarot => tarot with { SpreadId = StandardTarotSpreads.ThreeCards.Id });
        ApplyTarot(coordinator, tarot => tarot with { ArtworkPackId = TarotPrototypeSelections.LupusNoctisArtworkPackId });
        ApplyTarot(coordinator, tarot => tarot with { BackVariantId = new TarotBackVariantId("lunar-seal") });
        ApplyTarot(coordinator, tarot => tarot with { AllowReversed = true });
        ApplyTarot(coordinator, tarot => tarot with { AutoRevealCards = false });

        // The sole artwork ID is already the persisted default, so its no-op is intentionally suppressed.
        Assert.Equal(7, store.SavedPreferences.Count);
        Assert.Equal(7, eventCount);
        Assert.Equal(coordinator.Current, store.SavedPreferences[^1]);
        Assert.Equal("three-cards", coordinator.Current.Tarot.SpreadId.Value);
        Assert.Equal("lunar-seal", coordinator.Current.Tarot.BackVariantId.Value);
        Assert.True(coordinator.Current.Tarot.AllowReversed);
        Assert.False(coordinator.Current.Tarot.AutoRevealCards);
    }

    [Fact]
    public void DrawRevealAndSelection_DoNotWriteSettings()
    {
        var initial = UserPreferencesDefaults.Create() with
        {
            Tarot = TarotWorkspacePreferences.CreateDefault() with
            {
                SpreadId = StandardTarotSpreads.ThreeCards.Id,
                AutoRevealCards = false
            }
        };
        var store = new RecordingStore();
        var coordinator = CreateCoordinator(store, initial);
        var viewModel = TarotWorkspaceViewModel.CreateFoundation(
            new TarotDrawEngine(new SequenceRandomSource(0, 0, 0)),
            initialPreferences: initial.Tarot);
        viewModel.PreferencesChanged += (_, preferences) => coordinator.ApplyTarotPreferences(preferences);

        viewModel.Draw(Instant.FromUnixTimeTicks(100));
        var assignment = Assert.IsType<TarotReading>(viewModel.CurrentReading).Cards[1];
        viewModel.RevealAndSelect(assignment.PositionId);
        viewModel.RevealAndSelect(assignment.PositionId);

        Assert.Equal(1, viewModel.RevealedCardCount);
        Assert.Same(assignment, viewModel.SelectedCard);
        Assert.Empty(store.SavedPreferences);
    }

    [Fact]
    public void SaveFailure_IsRetainedAsControlledDiagnosticWithoutThrowing()
    {
        var diagnostic = new UserPreferencesDiagnostic(
            UserPreferencesDiagnosticCode.SaveFailure,
            "controlled failure");
        var store = new RecordingStore(new UserPreferencesSaveResult(diagnostic));
        var coordinator = CreateCoordinator(store, UserPreferencesDefaults.Create());
        UserPreferences? published = null;
        coordinator.PreferencesChanged += (_, preferences) => published = preferences;

        var changed = coordinator.ApplyTarotPreferences(
            coordinator.Current.Tarot with { AutoRevealCards = false });

        Assert.True(changed);
        Assert.Equal(diagnostic, coordinator.LastDiagnostic);
        Assert.Equal(coordinator.Current, published);
        Assert.False(coordinator.Current.Tarot.AutoRevealCards);
        Assert.Single(store.SavedPreferences);
    }

    [Fact]
    public void FreshWorkspace_RestoresPreferencesButNotReadingRevealOrSelectionState()
    {
        var persisted = CreatePreferences();
        var coordinator = CreateCoordinator(new RecordingStore(), persisted);

        var viewModel = TarotWorkspaceViewModel.CreateFoundation(
            new TarotDrawEngine(new SequenceRandomSource()),
            initialPreferences: coordinator.Current.Tarot);

        Assert.Equal(persisted.Tarot, viewModel.Preferences);
        Assert.Null(viewModel.CurrentReading);
        Assert.Null(viewModel.SelectedCard);
        Assert.Equal(0, viewModel.RevealedCardCount);
        Assert.False(viewModel.HasRevealedCards);
        Assert.False(viewModel.AreAllCardsRevealed);
    }

    private static UserPreferencesCoordinator CreateCoordinator(
        IUserPreferencesStore store,
        UserPreferences preferences) => new(store, new UserPreferencesLoadResult(preferences, null));

    private static void ApplyApplication(
        UserPreferencesCoordinator coordinator,
        Func<UserPreferences, UserPreferences> change) =>
        Assert.True(coordinator.ApplyApplicationPreferences(change(coordinator.Current)));

    private static void ApplyTarot(
        UserPreferencesCoordinator coordinator,
        Func<TarotWorkspacePreferences, TarotWorkspacePreferences> change) =>
        coordinator.ApplyTarotPreferences(change(coordinator.Current.Tarot));

    private static UserPreferences CreatePreferences() => new(
        new ApplicationLanguagePreference(new LanguageCode("ru")),
        new InterpretationLanguagePreference(new LanguageCode("ru")),
        new ThemeId("dark"),
        new TarotWorkspacePreferences(
            StandardTarotSpreads.ThreeCards.Id,
            TarotPrototypeSelections.LupusNoctisArtworkPackId,
            new TarotBackVariantId("lunar-seal"),
            AllowReversed: true,
            AutoRevealCards: false));

    private sealed class RecordingStore(UserPreferencesSaveResult? saveResult = null) : IUserPreferencesStore
    {
        public string SettingsPath { get; } = "recording://settings.json";

        public List<UserPreferences> SavedPreferences { get; } = [];

        public UserPreferencesLoadResult Load() => new(UserPreferencesDefaults.Create(), null);

        public UserPreferencesSaveResult Save(UserPreferences preferences)
        {
            SavedPreferences.Add(preferences);
            return saveResult ?? UserPreferencesSaveResult.Success;
        }
    }

    private sealed class SequenceRandomSource(params int[] values) : ITarotRandomSource
    {
        private readonly Queue<int> values = new(values);

        public int NextIndex(int exclusiveUpperBound) => values.Dequeue();
    }
}
