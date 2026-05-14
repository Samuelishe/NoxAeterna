using NodaTime;
using NodaTime.TimeZones;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Astronomy.Time;

/// <summary>
/// Resolves birth data to a UTC instant using the TZDB timezone database.
/// </summary>
public sealed class TzdbBirthMomentResolver : IBirthMomentResolver
{
    private readonly IDateTimeZoneProvider _timeZoneProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TzdbBirthMomentResolver"/> class.
    /// </summary>
    /// <param name="timeZoneProvider">The timezone provider used to resolve IANA timezone identifiers.</param>
    public TzdbBirthMomentResolver(IDateTimeZoneProvider? timeZoneProvider = null)
    {
        _timeZoneProvider = timeZoneProvider ?? DateTimeZoneProviders.Tzdb;
    }

    /// <inheritdoc />
    public BirthMoment Resolve(BirthData birthData)
    {
        if (!birthData.LocalBirthDateTime.HasKnownTime)
        {
            throw new InvalidOperationException("Birth data with an unknown local time cannot be resolved to an instant.");
        }

        var localDateTime = birthData.LocalBirthDateTime.ToLocalDateTime();
        var timeZone = _timeZoneProvider[birthData.TimezoneId.Value];
        var mapping = timeZone.MapLocal(localDateTime);

        var resolutionStatus = mapping.Count switch
        {
            0 => TimeResolutionStatus.InvalidShiftedForward,
            1 => TimeResolutionStatus.Resolved,
            2 => TimeResolutionStatus.AmbiguousResolvedEarlier,
            _ => throw new InvalidOperationException("Unexpected local time mapping state.")
        };

        var zonedDateTime = timeZone.ResolveLocal(localDateTime, Resolvers.LenientResolver);

        return new BirthMoment(
            localDateTime,
            birthData.TimezoneId,
            zonedDateTime.ToInstant(),
            resolutionStatus,
            birthData.BirthTimeAccuracy,
            birthData.SourceNote);
    }
}
