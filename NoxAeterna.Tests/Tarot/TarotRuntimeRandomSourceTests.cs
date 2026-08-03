using NoxAeterna.Infrastructure.Tarot;

namespace NoxAeterna.Tests.Tarot;

public sealed class TarotRuntimeRandomSourceTests
{
    [Fact]
    public void SystemAdapter_ProducesBoundedRuntimeValuesWithoutStaticMutableFields()
    {
        var source = new SystemTarotRandomSource();
        var values = Enumerable.Range(0, 128).Select(_ => source.NextIndex(78)).ToArray();
        var staticFields = typeof(SystemTarotRandomSource)
            .GetFields(System.Reflection.BindingFlags.Static |
                       System.Reflection.BindingFlags.Public |
                       System.Reflection.BindingFlags.NonPublic);

        Assert.All(values, value => Assert.InRange(value, 0, 77));
        Assert.True(values.Distinct().Count() > 1);
        Assert.Empty(staticFields);
        Assert.Throws<ArgumentOutOfRangeException>(() => source.NextIndex(0));
    }
}
