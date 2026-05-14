namespace NoxAeterna.Domain.Birth;

/// <summary>
/// Describes how reliable the provided birth time is.
/// </summary>
public enum BirthTimeAccuracy
{
    UnknownTime = 0,
    ApproximateTime = 1,
    ExactTime = 2
}
