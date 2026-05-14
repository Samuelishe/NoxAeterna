using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.App.Debug;

/// <summary>
/// Provides a temporary in-memory localization catalog for the thin shell and debug preview host.
/// This is bootstrap-only infrastructure until real localization loading exists.
/// </summary>
public static class DebugShellLocalizationFactory
{
    /// <summary>
    /// Creates the temporary localization provider for the shell host.
    /// </summary>
    /// <returns>An in-memory localization provider.</returns>
    public static ILocalizationProvider Create() =>
        new FallbackLocalizationProvider(
        [
            new LocalizationCatalog(
                LocalizationScope.Ui,
                new LanguageCode("ru"),
                [
                    new LocalizationEntry(new LocalizationKey("ui.shell.window_title"), "Нокс Этерна"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.navigation_title"), "Разделы"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.section.astrology"), "Астрология"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.section.tarot"), "Таро"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.section.archive"), "Архив"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.section.settings"), "Настройки"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.section.debug_preview"), "Debug Preview"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.preview_caption"), "Временная визуальная проверка pipeline"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.preview_hint"), "Не продуктовый экран"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.placeholder.caption"), "Раздел пока не реализован"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.placeholder.hint"), "Сейчас доступен только временный preview pipeline")
                ]),
            new LocalizationCatalog(
                LocalizationScope.Ui,
                new LanguageCode("en"),
                [
                    new LocalizationEntry(new LocalizationKey("ui.shell.window_title"), "Nox Aeterna"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.navigation_title"), "Sections"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.section.astrology"), "Astrology"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.section.tarot"), "Tarot"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.section.archive"), "Archive"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.section.settings"), "Settings"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.section.debug_preview"), "Debug Preview"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.preview_caption"), "Temporary visual pipeline verification"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.preview_hint"), "Not product UI"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.placeholder.caption"), "Section scaffold only"),
                    new LocalizationEntry(new LocalizationKey("ui.shell.placeholder.hint"), "Only the temporary preview pipeline is available now")
                ])
        ]);
}
