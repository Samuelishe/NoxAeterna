using NoxAeterna.Domain.Birth;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents one selectable birth-time accuracy option.
/// </summary>
public sealed record BirthTimeAccuracyOption(BirthTimeAccuracy Accuracy, LocalizationKey LabelKey);
