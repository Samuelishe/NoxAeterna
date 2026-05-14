using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Presentation.Astrology;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Hosts the first structured birth-data input UI inside the astrology workspace.
/// </summary>
public sealed class BirthDataInputControl : UserControl
{
    private readonly BirthDataInputViewModel _viewModel;
    private readonly ILocalizationProvider _localizationProvider;
    private readonly LanguageCode _applicationLanguage;
    private TextBox? _birthDateTextBox;
    private TextBox? _birthTimeTextBox;
    private ComboBox? _birthTimeAccuracyComboBox;
    private TextBox? _birthPlaceTextBox;
    private TextBox? _latitudeTextBox;
    private TextBox? _longitudeTextBox;
    private TextBox? _timezoneTextBox;
    private TextBlock? _validationSummaryTextBlock;

    /// <summary>
    /// Initializes a new instance of the <see cref="BirthDataInputControl"/> class.
    /// </summary>
    public BirthDataInputControl(
        BirthDataInputViewModel viewModel,
        ILocalizationProvider localizationProvider,
        LanguageCode applicationLanguage)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _localizationProvider = localizationProvider ?? throw new ArgumentNullException(nameof(localizationProvider));
        _applicationLanguage = applicationLanguage;

        Content = BuildContent();
        RefreshValidationSummary();
    }

    private Control BuildContent()
    {
        _birthDateTextBox = CreateTextBox(_viewModel.State.BirthDateText, "YYYY-MM-DD");
        _birthTimeTextBox = CreateTextBox(_viewModel.State.BirthTimeText, "HH:mm");
        _birthPlaceTextBox = CreateTextBox(_viewModel.State.BirthPlaceDisplayName, string.Empty);
        _latitudeTextBox = CreateTextBox(_viewModel.State.LatitudeText, string.Empty);
        _longitudeTextBox = CreateTextBox(_viewModel.State.LongitudeText, string.Empty);
        _timezoneTextBox = CreateTextBox(_viewModel.State.TimezoneIdText, "Europe/Moscow");

        _birthTimeAccuracyComboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemsSource = _viewModel.AvailableTimeAccuracies
                .Select(option => new LocalizedBirthTimeAccuracyOption(option, Localize(option.LabelKey)))
                .ToArray(),
            SelectedItem = _viewModel.AvailableTimeAccuracies
                .Select(option => new LocalizedBirthTimeAccuracyOption(option, Localize(option.LabelKey)))
                .First(option => option.Option.Accuracy == _viewModel.State.BirthTimeAccuracy)
        };
        _birthTimeAccuracyComboBox.SelectionChanged += (_, _) => SyncStateFromInputs();

        AttachSync(_birthDateTextBox, _birthTimeTextBox, _birthPlaceTextBox, _latitudeTextBox, _longitudeTextBox, _timezoneTextBox);

        var validateButton = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Content = Localize(_viewModel.ValidateActionKey)
        };
        validateButton.Click += (_, _) =>
        {
            SyncStateFromInputs();
            _viewModel.TryCreateBirthData(out _);
            RefreshValidationSummary();
        };

        _validationSummaryTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Foreground = ResolveBrush("WorkspaceValidationErrorBrush", new SolidColorBrush(Color.FromRgb(190, 110, 110)))
        };

        return new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    CreateSettingRow(Localize(_viewModel.BirthDateLabelKey), _birthDateTextBox),
                    CreateSettingRow(Localize(_viewModel.BirthTimeLabelKey), _birthTimeTextBox),
                    CreateSettingRow(Localize(_viewModel.BirthTimeAccuracyLabelKey), _birthTimeAccuracyComboBox),
                    CreateSettingRow(Localize(_viewModel.BirthPlaceLabelKey), _birthPlaceTextBox),
                    CreateSettingRow(Localize(_viewModel.LatitudeLabelKey), _latitudeTextBox),
                    CreateSettingRow(Localize(_viewModel.LongitudeLabelKey), _longitudeTextBox),
                    CreateSettingRow(Localize(_viewModel.TimezoneLabelKey), _timezoneTextBox),
                    validateButton,
                    _validationSummaryTextBlock
                }
            }
        };
    }

    private void AttachSync(params TextBox[] textBoxes)
    {
        foreach (var textBox in textBoxes)
        {
            textBox.PropertyChanged += (_, args) =>
            {
                if (args.Property == TextBox.TextProperty)
                {
                    SyncStateFromInputs();
                }
            };
        }
    }

    private void SyncStateFromInputs()
    {
        var selectedAccuracy = _birthTimeAccuracyComboBox?.SelectedItem is LocalizedBirthTimeAccuracyOption selected
            ? selected.Option.Accuracy
            : _viewModel.State.BirthTimeAccuracy;

        _viewModel.UpdateState(
            new BirthDataInputState(
                _birthDateTextBox?.Text ?? string.Empty,
                _birthTimeTextBox?.Text ?? string.Empty,
                selectedAccuracy,
                _birthPlaceTextBox?.Text ?? string.Empty,
                _latitudeTextBox?.Text ?? string.Empty,
                _longitudeTextBox?.Text ?? string.Empty,
                _timezoneTextBox?.Text ?? string.Empty));
        RefreshValidationSummary();
    }

    private void RefreshValidationSummary()
    {
        if (_validationSummaryTextBlock is null)
        {
            return;
        }

        if (!_viewModel.HasValidationAttempt)
        {
            _validationSummaryTextBlock.Text = string.Empty;
            return;
        }

        if (_viewModel.ValidationResult.IsValid)
        {
            _validationSummaryTextBlock.Text = Localize(_viewModel.ValidationSuccessKey);
            _validationSummaryTextBlock.Foreground = ResolveBrush("WorkspaceValidationSuccessBrush", new SolidColorBrush(Color.FromRgb(120, 150, 120)));
            return;
        }

        _validationSummaryTextBlock.Text = string.Join(
            Environment.NewLine,
            _viewModel.ValidationResult.Errors.Select(error => Localize(error.MessageKey)));
        _validationSummaryTextBlock.Foreground = ResolveBrush("WorkspaceValidationErrorBrush", new SolidColorBrush(Color.FromRgb(190, 110, 110)));
    }

    private static Control CreateSettingRow(string labelText, Control editor) =>
        new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock
                {
                    Text = labelText,
                    FontSize = 14
                },
                editor
            }
        };

    private static TextBox CreateTextBox(string initialText, string watermark) =>
        new()
        {
            Text = initialText,
            PlaceholderText = string.IsNullOrWhiteSpace(watermark) ? null : watermark,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _applicationLanguage, key).Text;

    private IBrush ResolveBrush(string resourceKey, IBrush fallbackBrush) =>
        Application.Current is { } application &&
        application.TryGetResource(resourceKey, ActualThemeVariant, out var resource) &&
        resource is IBrush brush
            ? brush
            : fallbackBrush;

    private sealed record LocalizedBirthTimeAccuracyOption(BirthTimeAccuracyOption Option, string Label)
    {
        public override string ToString() => Label;
    }
}
