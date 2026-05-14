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

        BirthDataInputValidator.TryParseDate(state.BirthDateText, out var date);
        var time = default(NodaTime.LocalTime?);
        if (state.BirthTimeAccuracy != BirthTimeAccuracy.UnknownTime)
        {
            BirthDataInputValidator.TryParseTime(state.BirthTimeText, out var parsedTime);
            time = parsedTime;
        }

        BirthDataInputValidator.TryParseCoordinate(state.LatitudeText, out var latitude);
        BirthDataInputValidator.TryParseCoordinate(state.LongitudeText, out var longitude);

        birthData = new BirthData(
            new LocalBirthDateTime(date, time),
            state.BirthTimeAccuracy,
            new BirthLocation(state.BirthPlaceDisplayName, latitude, longitude),
            new TimezoneId(state.TimezoneIdText));
        return true;
    }
}
