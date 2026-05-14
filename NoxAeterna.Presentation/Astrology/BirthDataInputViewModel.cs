using NoxAeterna.Domain.Birth;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents the first structured birth-data input state in the astrology workspace.
/// </summary>
public sealed class BirthDataInputViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BirthDataInputViewModel"/> class.
    /// </summary>
    /// <param name="state">The editable input state.</param>
    /// <param name="availableTimeAccuracies">The available birth-time accuracy options.</param>
    public BirthDataInputViewModel(
        BirthDataInputState state,
        IEnumerable<BirthTimeAccuracyOption> availableTimeAccuracies)
    {
        var accuracies = (availableTimeAccuracies ?? throw new ArgumentNullException(nameof(availableTimeAccuracies))).ToArray();
        if (accuracies.Length == 0)
        {
            throw new ArgumentException("Birth data input must expose at least one time accuracy option.", nameof(availableTimeAccuracies));
        }

        State = state ?? throw new ArgumentNullException(nameof(state));
        AvailableTimeAccuracies = Array.AsReadOnly(accuracies);
        ValidationResult = BirthDataValidationResult.Success;
    }

    public LocalizationKey BirthDateLabelKey { get; } = new("ui.birth_data.birth_date");
    public LocalizationKey BirthTimeLabelKey { get; } = new("ui.birth_data.birth_time");
    public LocalizationKey BirthTimeAccuracyLabelKey { get; } = new("ui.birth_data.birth_time_accuracy");
    public LocalizationKey BirthPlaceLabelKey { get; } = new("ui.birth_data.birth_place");
    public LocalizationKey LatitudeLabelKey { get; } = new("ui.birth_data.latitude");
    public LocalizationKey LongitudeLabelKey { get; } = new("ui.birth_data.longitude");
    public LocalizationKey TimezoneLabelKey { get; } = new("ui.birth_data.timezone");
    public LocalizationKey ValidateActionKey { get; } = new("ui.birth_data.validate");
    public LocalizationKey ValidationSuccessKey { get; } = new("ui.birth_data.validation.valid");

    /// <summary>
    /// Gets the editable input state.
    /// </summary>
    public BirthDataInputState State { get; private set; }

    /// <summary>
    /// Gets the available birth-time accuracy options.
    /// </summary>
    public IReadOnlyList<BirthTimeAccuracyOption> AvailableTimeAccuracies { get; }

    /// <summary>
    /// Gets the latest validation result.
    /// </summary>
    public BirthDataValidationResult ValidationResult { get; private set; }

    /// <summary>
    /// Gets a value indicating whether validation has already been requested.
    /// </summary>
    public bool HasValidationAttempt { get; private set; }

    /// <summary>
    /// Replaces the editable input state.
    /// </summary>
    public void UpdateState(BirthDataInputState state)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        ValidationResult = BirthDataValidationResult.Success;
        HasValidationAttempt = false;
    }

    /// <summary>
    /// Validates the current input state.
    /// </summary>
    public void Validate()
    {
        ValidationResult = BirthDataInputValidator.Validate(State);
        HasValidationAttempt = true;
    }

    /// <summary>
    /// Attempts to map the current input state to domain birth data.
    /// </summary>
    public bool TryCreateBirthData(out BirthData birthData)
    {
        var mapped = BirthDataInputMapper.TryMap(State, out birthData, out var validationResult);
        ValidationResult = validationResult;
        HasValidationAttempt = true;
        return mapped;
    }

    /// <summary>
    /// Creates the current default birth-data input foundation.
    /// </summary>
    public static BirthDataInputViewModel CreateDefault() =>
        new(
            new BirthDataInputState(
                BirthDateText: string.Empty,
                BirthTimeText: string.Empty,
                BirthTimeAccuracy: BirthTimeAccuracy.ExactTime,
                BirthPlaceDisplayName: string.Empty,
                LatitudeText: string.Empty,
                LongitudeText: string.Empty,
                TimezoneIdText: string.Empty),
            new[]
            {
                new BirthTimeAccuracyOption(BirthTimeAccuracy.ExactTime, new LocalizationKey("ui.birth_data.time_accuracy.exact")),
                new BirthTimeAccuracyOption(BirthTimeAccuracy.ApproximateTime, new LocalizationKey("ui.birth_data.time_accuracy.approximate")),
                new BirthTimeAccuracyOption(BirthTimeAccuracy.UnknownTime, new LocalizationKey("ui.birth_data.time_accuracy.unknown"))
            });
}
