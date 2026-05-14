using System.Reflection;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Tests.Birth;

public sealed class PublicApiTimeTypeTests
{
    [Fact]
    public void BirthDomainPublicApi_DoesNotExposeSystemDateTime()
    {
        var birthTypes = typeof(BirthData).Assembly
            .GetTypes()
            .Where(type => type.IsPublic && type.Namespace == "NoxAeterna.Domain.Birth");

        foreach (var type in birthTypes)
        {
            AssertNoSystemDateTime(type, type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(property => property.PropertyType));

            AssertNoSystemDateTime(type, type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .SelectMany(constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType)));

            AssertNoSystemDateTime(type, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName)
                .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType)));
        }
    }

    private static void AssertNoSystemDateTime(Type owner, IEnumerable<Type> exposedTypes)
    {
        foreach (var exposedType in exposedTypes)
        {
            var unwrappedType = Nullable.GetUnderlyingType(exposedType) ?? exposedType;

            Assert.False(
                unwrappedType == typeof(DateTime) || unwrappedType == typeof(DateTimeOffset),
                $"{owner.FullName} leaks {unwrappedType.FullName} through its public API.");
        }
    }
}
