using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Maps validated presentation birth-data input state to domain birth-data values.
/// </summary>
public static class BirthDataInputMapper
{
    /// <summary>
    /// Attempts to map editable presentation state to domain birth data.
    /// </summary>
    /// <param name="state">The source input state.</param>
    /// <param name="birthData">The resulting domain birth data when mapping succeeds.</param>
    /// <param name="validationResult">The validation result.</param>
    /// <returns><see langword="true"/> when mapping succeeds; otherwise <see langword="false"/>.</returns>
    public static bool TryMap(BirthDataInputState state, out BirthData birthData, out BirthDataValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(state);

        validationResult = BirthDataInputValidator.Validate(state);
        if (!validationResult.IsValid)
        {
            birthData = default;
            return false;
        }

        var selectedDate = state.BirthDate!.Value;
        var date = new NodaTime.LocalDate(selectedDate.Year, selectedDate.Month, selectedDate.Day);
        var time = default(NodaTime.LocalTime?);
        if (state.BirthTimeAccuracy != BirthTimeAccuracy.UnknownTime)
        {
            var selectedTime = state.BirthTime!.Value;
            time = new NodaTime.LocalTime(selectedTime.Hours, selectedTime.Minutes);
        }

        BirthDataInputValidator.TryParseCoordinate(state.LatitudeText, out var latitude);
        BirthDataInputValidator.TryParseCoordinate(state.LongitudeText, out var longitude);

        birthData = new BirthData(
            new LocalBirthDateTime(date, time),
            state.BirthTimeAccuracy,
            new BirthLocation(state.BirthPlaceDisplayName, latitude, longitude),
            new TimezoneId(state.TimezoneId));
        return true;
    }
}
