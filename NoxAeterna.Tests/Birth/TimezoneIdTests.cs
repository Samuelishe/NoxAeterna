using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Tests.Birth;

public sealed class TimezoneIdTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_RejectsEmptyValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new TimezoneId(value));
    }
}
