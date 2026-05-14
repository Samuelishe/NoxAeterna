namespace NoxAeterna.Domain.Birth;

/// <summary>
/// Describes how a local birth time was resolved when converting it to an instant.
/// </summary>
public enum TimeResolutionStatus
{
    Resolved = 0,
    AmbiguousResolvedEarlier = 1,
    AmbiguousResolvedLater = 2,
    InvalidShiftedForward = 3,
    InvalidRejected = 4
}
