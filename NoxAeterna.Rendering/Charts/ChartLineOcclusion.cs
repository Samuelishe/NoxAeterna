using Avalonia;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Removes protected rectangular intervals from straight chart line segments.
/// </summary>
public static class ChartLineOcclusion
{
    private const double Epsilon = 1e-9;

    /// <summary>
    /// Returns the portions of a segment that remain visible outside protected bounds.
    /// </summary>
    /// <param name="source">The original segment source.</param>
    /// <param name="target">The original segment target.</param>
    /// <param name="protectedBounds">The annotation bounds that occlude the line.</param>
    /// <param name="margin">An additional DIP margin around every protected rectangle.</param>
    public static IReadOnlyList<ChartLineSegment> GetVisibleSegments(
        Point source,
        Point target,
        IEnumerable<Rect> protectedBounds,
        double margin = 0d)
    {
        ArgumentNullException.ThrowIfNull(protectedBounds);
        ValidatePoint(source, nameof(source));
        ValidatePoint(target, nameof(target));

        if (!double.IsFinite(margin) || margin < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(margin),
                "Occlusion margin must be a finite non-negative number.");
        }

        var bounds = protectedBounds
            .Select(item =>
            {
                ValidateRect(item);
                return Inflate(item, margin);
            })
            .ToArray();
        var delta = target - source;
        var lengthSquared = (delta.X * delta.X) + (delta.Y * delta.Y);

        if (lengthSquared <= Epsilon * Epsilon)
        {
            return bounds.Any(item => ContainsInclusive(item, source))
                ? Array.Empty<ChartLineSegment>()
                : new[] { new ChartLineSegment(source, target) };
        }

        var occludedIntervals = bounds
            .Select(item => GetIntersectionInterval(source, delta, item))
            .Where(static interval => interval is not null)
            .Select(static interval => interval!.Value)
            .Where(static interval => interval.End - interval.Start > Epsilon)
            .OrderBy(static interval => interval.Start)
            .ThenBy(static interval => interval.End)
            .ToArray();

        if (occludedIntervals.Length == 0)
        {
            return new[] { new ChartLineSegment(source, target) };
        }

        var merged = Merge(occludedIntervals);
        var visible = new List<ChartLineSegment>(merged.Count + 1);
        var cursor = 0d;

        foreach (var interval in merged)
        {
            if (interval.Start - cursor > Epsilon)
            {
                visible.Add(CreateSegment(source, delta, cursor, interval.Start));
            }

            cursor = Math.Max(cursor, interval.End);
        }

        if (1d - cursor > Epsilon)
        {
            visible.Add(CreateSegment(source, delta, cursor, 1d));
        }

        return visible.AsReadOnly();
    }

    private static ParameterInterval? GetIntersectionInterval(
        Point source,
        Vector delta,
        Rect bounds)
    {
        var start = 0d;
        var end = 1d;

        if (!ClipAxis(source.X, delta.X, bounds.Left, bounds.Right, ref start, ref end) ||
            !ClipAxis(source.Y, delta.Y, bounds.Top, bounds.Bottom, ref start, ref end))
        {
            return null;
        }

        return new ParameterInterval(
            Math.Clamp(start, 0d, 1d),
            Math.Clamp(end, 0d, 1d));
    }

    private static bool ClipAxis(
        double source,
        double delta,
        double minimum,
        double maximum,
        ref double start,
        ref double end)
    {
        if (Math.Abs(delta) <= Epsilon)
        {
            return source >= minimum - Epsilon && source <= maximum + Epsilon;
        }

        var first = (minimum - source) / delta;
        var second = (maximum - source) / delta;
        if (first > second)
        {
            (first, second) = (second, first);
        }

        start = Math.Max(start, first);
        end = Math.Min(end, second);
        return start <= end + Epsilon;
    }

    private static IReadOnlyList<ParameterInterval> Merge(
        IReadOnlyList<ParameterInterval> ordered)
    {
        var merged = new List<ParameterInterval>(ordered.Count);
        var current = ordered[0];

        for (var index = 1; index < ordered.Count; index++)
        {
            var next = ordered[index];
            if (next.Start <= current.End + Epsilon)
            {
                current = current with { End = Math.Max(current.End, next.End) };
                continue;
            }

            merged.Add(current);
            current = next;
        }

        merged.Add(current);
        return merged;
    }

    private static ChartLineSegment CreateSegment(
        Point source,
        Vector delta,
        double start,
        double end) =>
        new(
            source + (delta * start),
            source + (delta * end));

    private static Rect Inflate(Rect bounds, double margin) =>
        new(
            bounds.X - margin,
            bounds.Y - margin,
            bounds.Width + (margin * 2d),
            bounds.Height + (margin * 2d));

    private static bool ContainsInclusive(Rect bounds, Point point) =>
        point.X >= bounds.Left - Epsilon &&
        point.X <= bounds.Right + Epsilon &&
        point.Y >= bounds.Top - Epsilon &&
        point.Y <= bounds.Bottom + Epsilon;

    private static void ValidatePoint(Point point, string parameterName)
    {
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y))
        {
            throw new ArgumentException("Line-segment points must be finite.", parameterName);
        }
    }

    private static void ValidateRect(Rect bounds)
    {
        if (!double.IsFinite(bounds.X) ||
            !double.IsFinite(bounds.Y) ||
            !double.IsFinite(bounds.Width) ||
            !double.IsFinite(bounds.Height) ||
            bounds.Width < 0d ||
            bounds.Height < 0d)
        {
            throw new ArgumentException(
                "Protected bounds must contain finite, non-negative dimensions.",
                nameof(bounds));
        }
    }

    private readonly record struct ParameterInterval(double Start, double End);
}
