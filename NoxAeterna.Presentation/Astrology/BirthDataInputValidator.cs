using System.Globalization;
using NodaTime.Text;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Validates editable birth-data input state before domain mapping.
/// </summary>
public static class BirthDataInputValidator
{
    private static readonly LocalDatePattern DatePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");
    private static readonly LocalTimePattern TimePattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm");

    /// <summary>
    /// Validates the provided birth-data input state.
    /// </summary>
    /// <param name="state">The input state.</param>
    /// <returns>A deterministic validation result.</returns>
    public static BirthDataValidationResult Validate(BirthDataInputState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var errors = new List<BirthDataInputError>();

        if (string.IsNullOrWhiteSpace(state.BirthDateText))
        {
            errors.Add(new BirthDataInputError(BirthDataInputField.BirthDate, new LocalizationKey("ui.birth_data.validation.date_required")));
        }
        else if (!DatePattern.Parse(state.BirthDateText.Trim()).Success)
        {
            errors.Add(new BirthDataInputError(BirthDataInputField.BirthDate, new LocalizationKey("ui.birth_data.validation.date_invalid")));
        }

        if (state.BirthTimeAccuracy == BirthTimeAccuracy.UnknownTime)
        {
            if (!string.IsNullOrWhiteSpace(state.BirthTimeText))
            {
                errors.Add(new BirthDataInputError(BirthDataInputField.BirthTime, new LocalizationKey("ui.birth_data.validation.time_must_be_empty_for_unknown")));
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(state.BirthTimeText))
            {
                errors.Add(new BirthDataInputError(BirthDataInputField.BirthTime, new LocalizationKey("ui.birth_data.validation.time_required")));
            }
            else if (!TimePattern.Parse(state.BirthTimeText.Trim()).Success)
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

        if (string.IsNullOrWhiteSpace(state.TimezoneIdText))
        {
            errors.Add(new BirthDataInputError(BirthDataInputField.TimezoneId, new LocalizationKey("ui.birth_data.validation.timezone_required")));
        }

        return errors.Count == 0 ? BirthDataValidationResult.Success : new BirthDataValidationResult(errors);
    }

    /// <summary>
    /// Attempts to parse a local date from input text.
    /// </summary>
    public static bool TryParseDate(string text, out NodaTime.LocalDate date)
    {
        var parseResult = DatePattern.Parse(text.Trim());
        date = parseResult.Success ? parseResult.Value : default;
        return parseResult.Success;
    }

    /// <summary>
    /// Attempts to parse a local time from input text.
    /// </summary>
    public static bool TryParseTime(string text, out NodaTime.LocalTime time)
    {
        var parseResult = TimePattern.Parse(text.Trim());
        time = parseResult.Success ? parseResult.Value : default;
        return parseResult.Success;
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
