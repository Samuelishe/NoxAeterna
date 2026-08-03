using System.Security.Cryptography;
using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Infrastructure.Tarot;

/// <summary>Provides runtime Tarot indices through the operating system random-number generator.</summary>
public sealed class SystemTarotRandomSource : ITarotRandomSource
{
    /// <inheritdoc />
    public int NextIndex(int exclusiveUpperBound)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exclusiveUpperBound);
        return RandomNumberGenerator.GetInt32(exclusiveUpperBound);
    }
}
