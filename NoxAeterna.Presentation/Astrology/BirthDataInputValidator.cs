using System.Globalization;
using NodaTime;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Validates editable birth-data input state before domain mapping.
/// </summary>
public static class BirthDataInputValidator
{
    private static readonly HashSet<string> TimezoneIds = DateTimeZoneProviders.Tzdb.Ids.ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// Validates the provided birth-data input state.
    /// </summary>
    /// <param name="state">The input state.</param>
    /// <returns>A deterministic validation result.</returns>
    public static BirthDataValidationResult Validate(BirthDataInputState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var errors = new List<BirthDataInputError>();

        if (state.BirthDate is null)
        {
            errors.Add(new BirthDataInputError(BirthDataInputField.BirthDate, new LocalizationKey("ui.birth_data.validation.date_required")));
        }

        if (state.BirthTimeAccuracy == BirthTimeAccuracy.UnknownTime)
        {
        }
        else
        {
            if (state.BirthTime is null)
            {
                errors.Add(new BirthDataInputError(BirthDataInputField.BirthTime, new LocalizationKey("ui.birth_data.validation.time_required")));
            }
            else if (state.BirthTime < TimeSpan.Zero || state.BirthTime >= TimeSpan.FromDays(1))
            {
                errors.Add(new BirthDataInputError(BirthDataInputField.BirthTime, new LocalizationKey("ui.birth_data.validation.time_invalid")));
            }
        }

        if (string.IsNullOrWhiteSpace(state.BirthPlaceDisplayName))
        {
            errors.Add(new BirthDataInputError(BirthDataInputField.BirthPlace, new LocalizationKey("ui.birth_data.validation.place_required")));
        }

        ValidateCoordinate(
            state.LatitudeText,
            BirthDataInputField.Latitude,
            -90d,
            90d,
            new LocalizationKey("ui.birth_data.validation.latitude_required"),
            new LocalizationKey("ui.birth_data.validation.latitude_invalid"),
            errors);

        ValidateCoordinate(
            state.LongitudeText,
            BirthDataInputField.Longitude,
            -180d,
            180d,
            new LocalizationKey("ui.birth_data.validation.longitude_required"),
            new LocalizationKey("ui.birth_data.validation.longitude_invalid"),
            errors);

        if (string.IsNullOrWhiteSpace(state.TimezoneId))
        {
            errors.Add(new BirthDataInputError(BirthDataInputField.TimezoneId, new LocalizationKey("ui.birth_data.validation.timezone_required")));
        }
        else if (!TimezoneIds.Contains(state.TimezoneId.Trim()))
        {
            errors.Add(new BirthDataInputError(BirthDataInputField.TimezoneId, new LocalizationKey("ui.birth_data.validation.timezone_invalid")));
        }

        return errors.Count == 0 ? BirthDataValidationResult.Success : new BirthDataValidationResult(errors);
    }

    /// <summary>
    /// Attempts to parse a coordinate from input text.
    /// </summary>
    public static bool TryParseCoordinate(string text, out double value)
    {
        var normalized = text.Trim().Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static void ValidateCoordinate(
        string coordinateText,
        BirthDataInputField field,
        double min,
        double max,
        LocalizationKey requiredKey,
        LocalizationKey invalidKey,
        ICollection<BirthDataInputError> errors)
    {
        if (string.IsNullOrWhiteSpace(coordinateText))
        {
            errors.Add(new BirthDataInputError(field, requiredKey));
            return;
        }

        if (!TryParseCoordinate(coordinateText, out var coordinate) || double.IsNaN(coordinate) || double.IsInfinity(coordinate) || coordinate < min || coordinate > max)
        {
            errors.Add(new BirthDataInputError(field, invalidKey));
        }
    }
}
