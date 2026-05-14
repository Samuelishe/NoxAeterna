using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NoxAeterna.App.Debug;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Shell;

namespace NoxAeterna.App;

public partial class MainWindow : Window
{
    private readonly ILocalizationProvider _localizationProvider;
    private readonly LanguageCode _applicationLanguage;
    private readonly ShellViewModel _shellViewModel;

    public MainWindow()
    {
        InitializeComponent();

        _applicationLanguage = new LanguageCode("ru");
        _localizationProvider = DebugShellLocalizationFactory.Create();
        _shellViewModel = ShellViewModel.CreateDefault();

        Title = Localize(_shellViewModel.WindowTitleKey);
        NavigationTitleTextBlock.Text = Localize("ui.shell.navigation_title");

        var navigationItems = _shellViewModel.NavigationItems
            .Select(item => new LocalizedShellNavigationItem(item, Localize(item.LabelKey)))
            .ToArray();

        NavigationListBox.ItemsSource = navigationItems;
        NavigationListBox.SelectedItem = navigationItems.First(item => item.Item.Id == _shellViewModel.SelectedSectionId);

        UpdateShellSection();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (NavigationListBox.SelectedItem is not LocalizedShellNavigationItem selectedItem)
        {
            return;
        }

        _shellViewModel.SelectedSectionId = selectedItem.Item.Id;
        UpdateShellSection();
    }

    private void UpdateShellSection()
    {
        var currentItem = _shellViewModel.NavigationItems.First(item => item.Id == _shellViewModel.SelectedSectionId);

        SectionTitleTextBlock.Text = Localize(currentItem.LabelKey);

        if (currentItem.Id == ShellSectionId.DebugPreview)
        {
            SectionHintTextBlock.Text = $"{Localize("ui.shell.preview_caption")} • {Localize("ui.shell.preview_hint")}";
            SectionContentHost.Content = new DebugChartPreviewControl();
            return;
        }

        SectionHintTextBlock.Text = $"{Localize("ui.shell.placeholder.caption")} • {Localize("ui.shell.placeholder.hint")}";
        SectionContentHost.Content = new TextBlock
        {
            Text = Localize("ui.shell.placeholder.caption"),
            FontSize = 18
        };
    }

    private string Localize(string key) => Localize(new LocalizationKey(key));

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _applicationLanguage, key).Text;

    private sealed record LocalizedShellNavigationItem(ShellNavigationItem Item, string Label)
    {
        public override string ToString() => Item.IsTemporary ? $"{Label} [temp]" : Label;
    }
}
