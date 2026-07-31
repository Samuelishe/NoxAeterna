using System.Text;

namespace NoxAeterna.Tools.Repository.Analysis;

public sealed record RepositoryTextMeasurement(bool IsText, int? Lines, int? Characters);

public static class RepositoryTextMetrics
{
    public static RepositoryTextMeasurement Measure(byte[] bytes)
    {
        if (bytes.AsSpan().Contains((byte)0))
        {
            return new RepositoryTextMeasurement(false, null, null);
        }

        string content;
        try
        {
            using var stream = new MemoryStream(bytes, writable: false);
            using var reader = new StreamReader(
                stream,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                detectEncodingFromByteOrderMarks: true);
            content = reader.ReadToEnd();
        }
        catch (DecoderFallbackException)
        {
            return new RepositoryTextMeasurement(false, null, null);
        }

        if (content.Length == 0)
        {
            return new RepositoryTextMeasurement(true, 0, 0);
        }

        var lines = content.Count(static character => character == '\n');
        if (content[^1] != '\n')
        {
            lines++;
        }

        return new RepositoryTextMeasurement(true, lines, content.Length);
    }
}
