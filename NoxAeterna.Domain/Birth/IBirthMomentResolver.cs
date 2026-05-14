namespace NoxAeterna.Domain.Birth;

/// <summary>
/// Resolves user-entered birth data into a reproducible UTC birth moment.
/// </summary>
public interface IBirthMomentResolver
{
    /// <summary>
    /// Resolves the supplied <see cref="BirthData"/> into a calculation-ready <see cref="BirthMoment"/>.
    /// </summary>
    /// <param name="birthData">The unresolved user-entered birth data.</param>
    /// <returns>The resolved birth moment.</returns>
    BirthMoment Resolve(BirthData birthData);
}
