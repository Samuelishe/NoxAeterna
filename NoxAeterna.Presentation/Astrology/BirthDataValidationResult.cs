namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents the validation result for birth-data input state.
/// </summary>
public sealed class BirthDataValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BirthDataValidationResult"/> class.
    /// </summary>
    /// <param name="errors">The collected validation errors.</param>
    public BirthDataValidationResult(IEnumerable<BirthDataInputError> errors)
    {
        Errors = Array.AsReadOnly((errors ?? throw new ArgumentNullException(nameof(errors))).ToArray());
    }

    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyList<BirthDataInputError> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether the validation succeeded.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static BirthDataValidationResult Success { get; } = new(Array.Empty<BirthDataInputError>());
}
