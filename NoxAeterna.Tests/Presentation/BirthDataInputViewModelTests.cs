using NodaTime;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Presentation.Astrology;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Tests.Presentation;

public sealed class BirthDataInputViewModelTests
{
    [Fact]
    public void EmptyInputValidation_FailsForRequiredFields()
    {
        var viewModel = BirthDataInputViewModel.CreateDefault();

        viewModel.Validate();

        Assert.True(viewModel.HasValidationAttempt);
        Assert.False(viewModel.ValidationResult.IsValid);
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.BirthDate);
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.BirthPlace);
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.Latitude);
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.Longitude);
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.TimezoneId);
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.BirthTime);
    }

    [Fact]
    public void ValidInputValidation_Succeeds()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ExactTime,
                "Prague, Czechia",
                "50.0755",
                "14.4378",
                "Europe/Prague",
                LocationSource.ManualCoordinates));

        viewModel.Validate();

        Assert.True(viewModel.ValidationResult.IsValid);
        Assert.Empty(viewModel.ValidationResult.Errors);
    }

    [Fact]
    public void InvalidLatitudeValidation_Fails()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ExactTime,
                "Prague, Czechia",
                "95",
                "14.4378",
                "Europe/Prague",
                LocationSource.ManualCoordinates));

        viewModel.Validate();

        Assert.Contains(
            viewModel.ValidationResult.Errors,
            error => error.Field == BirthDataInputField.Latitude &&
                     error.MessageKey == new LocalizationKey("ui.birth_data.validation.latitude_invalid"));
    }

    [Fact]
    public void InvalidLongitudeValidation_Fails()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ExactTime,
                "Prague, Czechia",
                "50.0755",
                "-181",
                "Europe/Prague",
                LocationSource.ManualCoordinates));

        viewModel.Validate();

        Assert.Contains(
            viewModel.ValidationResult.Errors,
            error => error.Field == BirthDataInputField.Longitude &&
                     error.MessageKey == new LocalizationKey("ui.birth_data.validation.longitude_invalid"));
    }

    [Fact]
    public void MissingTimezoneValidation_Fails()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ExactTime,
                "Prague, Czechia",
                "50.0755",
                "14.4378",
                string.Empty,
                LocationSource.ManualCoordinates));

        viewModel.Validate();

        Assert.Contains(
            viewModel.ValidationResult.Errors,
            error => error.Field == BirthDataInputField.TimezoneId &&
                     error.MessageKey == new LocalizationKey("ui.birth_data.validation.timezone_required"));
    }

    [Fact]
    public void InvalidTimezone_IsRejected()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ExactTime,
                "Prague, Czechia",
                "50.0755",
                "14.4378",
                "Mars/Phobos",
                LocationSource.ManualCoordinates));

        viewModel.Validate();

        Assert.Contains(
            viewModel.ValidationResult.Errors,
            error => error.Field == BirthDataInputField.TimezoneId &&
                     error.MessageKey == new LocalizationKey("ui.birth_data.validation.timezone_invalid"));
    }

    [Fact]
    public void MissingPlaceNameValidation_Fails()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ExactTime,
                string.Empty,
                "50.0755",
                "14.4378",
                "Europe/Prague",
                LocationSource.ManualCoordinates));

        viewModel.Validate();

        Assert.Contains(
            viewModel.ValidationResult.Errors,
            error => error.Field == BirthDataInputField.BirthPlace &&
                     error.MessageKey == new LocalizationKey("ui.birth_data.validation.place_required"));
    }

    [Fact]
    public void UnknownBirthTime_AllowsEmptyTimeAndPreservesAccuracy()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                null,
                BirthTimeAccuracy.UnknownTime,
                "Prague, Czechia",
                "50.0755",
                "14.4378",
                "Europe/Prague",
                LocationSource.ManualCoordinates));

        viewModel.Validate();

        Assert.True(viewModel.ValidationResult.IsValid);
        Assert.Equal(BirthTimeAccuracy.UnknownTime, viewModel.State.BirthTimeAccuracy);
        Assert.Equal(TimeSpan.FromHours(12), viewModel.EffectiveTechnicalBirthTime);
    }

    [Fact]
    public void ExactTime_RequiresValidTime()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                null,
                BirthTimeAccuracy.ExactTime,
                "Prague, Czechia",
                "50.0755",
                "14.4378",
                "Europe/Prague",
                LocationSource.ManualCoordinates));

        viewModel.Validate();

        Assert.Contains(
            viewModel.ValidationResult.Errors,
            error => error.Field == BirthDataInputField.BirthTime &&
                     error.MessageKey == new LocalizationKey("ui.birth_data.validation.time_required"));
    }

    [Fact]
    public void TimezoneSelection_ExposesValidTzdbIds()
    {
        var viewModel = BirthDataInputViewModel.CreateDefault();

        Assert.Contains(viewModel.AvailableTimezones, option => option.TimezoneId == "Europe/Moscow");
        Assert.Contains(viewModel.AvailableTimezones, option => option.TimezoneId == "UTC");
    }

    [Fact]
    public void LocationSource_DefaultsAndUpdatesToManualCoordinates()
    {
        var viewModel = BirthDataInputViewModel.CreateDefault();

        Assert.Equal(LocationSource.NameOnly, viewModel.State.LocationSource);

        viewModel.UpdateState(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ExactTime,
                "Prague, Czechia",
                "50.0755",
                "14.4378",
                "Europe/Prague",
                LocationSource.NameOnly));

        Assert.Equal(LocationSource.ManualCoordinates, viewModel.State.LocationSource);
    }

    [Fact]
    public void TryCreateBirthData_MapsValidInputToDomainModel()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState(
                new DateTimeOffset(1990, 7, 14, 0, 0, 0, TimeSpan.Zero),
                new TimeSpan(13, 45, 0),
                BirthTimeAccuracy.ApproximateTime,
                "Prague, Czechia",
                "50.0755",
                "14.4378",
                "Europe/Prague",
                LocationSource.ManualCoordinates));

        var mapped = viewModel.TryCreateBirthData(out var birthData);

        Assert.True(mapped);
        Assert.Equal(new LocalDate(1990, 7, 14), birthData.LocalBirthDateTime.Date);
        Assert.Equal(new LocalTime(13, 45), birthData.LocalBirthDateTime.Time);
        Assert.Equal(BirthTimeAccuracy.ApproximateTime, birthData.BirthTimeAccuracy);
        Assert.Equal("Prague, Czechia", birthData.BirthLocation.DisplayName);
        Assert.Equal(50.0755d, birthData.BirthLocation.Latitude);
        Assert.Equal(14.4378d, birthData.BirthLocation.Longitude);
        Assert.Equal(new TimezoneId("Europe/Prague"), birthData.TimezoneId);
    }

    [Fact]
    public void LabelsAndErrors_AreLocalizationKeyBased()
    {
        var viewModel = BirthDataInputViewModel.CreateDefault();

        Assert.Equal(new LocalizationKey("ui.birth_data.birth_date"), viewModel.BirthDateLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.birth_time"), viewModel.BirthTimeLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.birth_time_accuracy"), viewModel.BirthTimeAccuracyLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.birth_city_or_settlement"), viewModel.BirthPlaceLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.birth_date_helper"), viewModel.BirthDateHelperKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.birth_time_helper"), viewModel.BirthTimeHelperKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.latitude"), viewModel.LatitudeLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.longitude"), viewModel.LongitudeLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.timezone"), viewModel.TimezoneLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.timezone_helper"), viewModel.TimezoneHelperKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.validate"), viewModel.ValidateActionKey);
        Assert.All(viewModel.AvailableTimeAccuracies, option => Assert.False(string.IsNullOrWhiteSpace(option.LabelKey.Value)));
    }

    private static BirthDataInputViewModel CreateViewModel(BirthDataInputState state) =>
        new(
            state,
            new[]
            {
                new BirthTimeAccuracyOption(BirthTimeAccuracy.ExactTime, new LocalizationKey("ui.birth_data.time_accuracy.exact")),
                new BirthTimeAccuracyOption(BirthTimeAccuracy.ApproximateTime, new LocalizationKey("ui.birth_data.time_accuracy.approximate")),
                new BirthTimeAccuracyOption(BirthTimeAccuracy.UnknownTime, new LocalizationKey("ui.birth_data.time_accuracy.unknown"))
            });
}
