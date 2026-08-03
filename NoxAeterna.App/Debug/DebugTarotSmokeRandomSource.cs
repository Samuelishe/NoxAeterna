#if DEBUG
using System.Globalization;
using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.App.Debug;

/// <summary>Provides an opt-in, process-local deterministic sequence for real-control Tarot smoke.</summary>
internal sealed class DebugTarotSmokeRandomSource : ITarotRandomSource
{
    internal const string EnvironmentVariableName = "NOX_AETERNA_TAROT_SMOKE_SEQUENCE";
    private readonly Queue<int> indices;

    private DebugTarotSmokeRandomSource(IEnumerable<int> indices)
    {
        this.indices = new Queue<int>(indices);
    }

    internal static ITarotRandomSource? CreateFromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var indices = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => int.Parse(segment, NumberStyles.None, CultureInfo.InvariantCulture))
            .ToArray();
        if (indices.Length == 0 || indices.Any(static index => index < 0))
        {
            throw new InvalidOperationException(
                $"{EnvironmentVariableName} must contain non-negative comma-separated indices.");
        }

        return new DebugTarotSmokeRandomSource(indices);
    }

    public int NextIndex(int exclusiveUpperBound)
    {
        if (indices.Count == 0)
        {
            throw new InvalidOperationException("The debug Tarot smoke sequence was exhausted.");
        }

        var index = indices.Dequeue();
        if (index >= exclusiveUpperBound)
        {
            throw new InvalidOperationException(
                $"The debug Tarot smoke index {index} is outside the bound {exclusiveUpperBound}.");
        }

        return index;
    }
}
#endif
