using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents one validation error in the birth-data input flow.
/// </summary>
public sealed record BirthDataInputError(BirthDataInputField Field, LocalizationKey MessageKey);
