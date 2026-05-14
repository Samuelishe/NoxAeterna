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
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.BirthTime);
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.BirthPlace);
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.Latitude);
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.Longitude);
        Assert.Contains(viewModel.ValidationResult.Errors, error => error.Field == BirthDataInputField.TimezoneId);
    }

    [Fact]
    public void ValidInputValidation_Succeeds()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState(
                "1990-07-14",
                "13:45",
                BirthTimeAccuracy.ExactTime,
                "Moscow",
                "55.7558",
                "37.6176",
                "Europe/Moscow"));

        viewModel.Validate();

        Assert.True(viewModel.ValidationResult.IsValid);
        Assert.Empty(viewModel.ValidationResult.Errors);
    }

    [Fact]
    public void InvalidLatitudeValidation_Fails()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState("1990-07-14", "13:45", BirthTimeAccuracy.ExactTime, "Moscow", "95", "37.6176", "Europe/Moscow"));

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
            new BirthDataInputState("1990-07-14", "13:45", BirthTimeAccuracy.ExactTime, "Moscow", "55.7558", "-181", "Europe/Moscow"));

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
            new BirthDataInputState("1990-07-14", "13:45", BirthTimeAccuracy.ExactTime, "Moscow", "55.7558", "37.6176", string.Empty));

        viewModel.Validate();

        Assert.Contains(
            viewModel.ValidationResult.Errors,
            error => error.Field == BirthDataInputField.TimezoneId &&
                     error.MessageKey == new LocalizationKey("ui.birth_data.validation.timezone_required"));
    }

    [Fact]
    public void MissingPlaceNameValidation_Fails()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState("1990-07-14", "13:45", BirthTimeAccuracy.ExactTime, string.Empty, "55.7558", "37.6176", "Europe/Moscow"));

        viewModel.Validate();

        Assert.Contains(
            viewModel.ValidationResult.Errors,
            error => error.Field == BirthDataInputField.BirthPlace &&
                     error.MessageKey == new LocalizationKey("ui.birth_data.validation.place_required"));
    }

    [Fact]
    public void UnknownBirthTime_AllowsEmptyTime()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState("1990-07-14", string.Empty, BirthTimeAccuracy.UnknownTime, "Moscow", "55.7558", "37.6176", "Europe/Moscow"));

        viewModel.Validate();

        Assert.True(viewModel.ValidationResult.IsValid);
    }

    [Fact]
    public void TryCreateBirthData_MapsValidInputToDomainModel()
    {
        var viewModel = CreateViewModel(
            new BirthDataInputState("1990-07-14", "13:45", BirthTimeAccuracy.ApproximateTime, "Moscow", "55.7558", "37.6176", "Europe/Moscow"));

        var mapped = viewModel.TryCreateBirthData(out var birthData);

        Assert.True(mapped);
        Assert.Equal(new LocalDate(1990, 7, 14), birthData.LocalBirthDateTime.Date);
        Assert.Equal(new LocalTime(13, 45), birthData.LocalBirthDateTime.Time);
        Assert.Equal(BirthTimeAccuracy.ApproximateTime, birthData.BirthTimeAccuracy);
        Assert.Equal("Moscow", birthData.BirthLocation.DisplayName);
        Assert.Equal(55.7558d, birthData.BirthLocation.Latitude);
        Assert.Equal(37.6176d, birthData.BirthLocation.Longitude);
        Assert.Equal(new TimezoneId("Europe/Moscow"), birthData.TimezoneId);
    }

    [Fact]
    public void LabelsAndErrors_AreLocalizationKeyBased()
    {
        var viewModel = BirthDataInputViewModel.CreateDefault();

        Assert.Equal(new LocalizationKey("ui.birth_data.birth_date"), viewModel.BirthDateLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.birth_time"), viewModel.BirthTimeLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.birth_time_accuracy"), viewModel.BirthTimeAccuracyLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.birth_place"), viewModel.BirthPlaceLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.latitude"), viewModel.LatitudeLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.longitude"), viewModel.LongitudeLabelKey);
        Assert.Equal(new LocalizationKey("ui.birth_data.timezone"), viewModel.TimezoneLabelKey);
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
