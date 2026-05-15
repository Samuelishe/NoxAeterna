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
    private DatePicker? _birthDatePicker;
    private TimePicker? _birthTimePicker;
    private ComboBox? _birthTimeAccuracyComboBox;
    private TextBox? _birthPlaceTextBox;
    private TextBox? _latitudeTextBox;
    private TextBox? _longitudeTextBox;
    private ComboBox? _timezoneComboBox;
    private TextBlock? _validationSummaryTextBlock;
    private TextBlock? _unknownTimeHelperTextBlock;

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
        _birthDatePicker = new DatePicker
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DayFormat = "dd",
            MonthFormat = "MM",
            YearFormat = "yyyy",
            SelectedDate = _viewModel.State.BirthDate
        };
        _birthDatePicker.PropertyChanged += (_, args) =>
        {
            if (args.Property == DatePicker.SelectedDateProperty)
            {
                SyncStateFromInputs();
            }
        };

        _birthTimePicker = new TimePicker
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ClockIdentifier = "24HourClock",
            MinuteIncrement = 1,
            UseSeconds = false,
            SelectedTime = _viewModel.State.BirthTime
        };
        _birthTimePicker.PropertyChanged += (_, args) =>
        {
            if (args.Property == TimePicker.SelectedTimeProperty)
            {
                SyncStateFromInputs();
            }
        };

        _birthPlaceTextBox = CreateTextBox(
            _viewModel.State.BirthPlaceDisplayName,
            Localize(new LocalizationKey("ui.birth_data.birth_city_or_settlement_placeholder")));
        _latitudeTextBox = CreateTextBox(_viewModel.State.LatitudeText, string.Empty);
        _longitudeTextBox = CreateTextBox(_viewModel.State.LongitudeText, string.Empty);
        _timezoneComboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MaxDropDownHeight = 360,
            ItemsSource = _viewModel.AvailableTimezones.ToArray(),
            SelectedItem = _viewModel.AvailableTimezones.FirstOrDefault(option => option.TimezoneId == _viewModel.State.TimezoneId)
        };
        _timezoneComboBox.SelectionChanged += (_, _) => SyncStateFromInputs();

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

        AttachSync(_birthPlaceTextBox, _latitudeTextBox, _longitudeTextBox);

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
        _unknownTimeHelperTextBlock = new TextBlock
        {
            Text = string.Empty,
            TextWrapping = TextWrapping.Wrap,
            Foreground = ResolveBrush("WorkspacePanelSubtleForegroundBrush", new SolidColorBrush(Color.FromRgb(128, 128, 132)))
        };

        ApplyBirthTimeInputMode();

        return new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    CreateSettingRow(Localize(_viewModel.BirthDateLabelKey), _birthDatePicker),
                    CreateHelperRow(Localize(_viewModel.BirthDateHelperKey)),
                    CreateSettingRow(
                        Localize(_viewModel.BirthTimeLabelKey),
                        new StackPanel
                        {
                            Spacing = 6,
                            Children =
                            {
                                _birthTimePicker,
                                _unknownTimeHelperTextBlock
                            }
                        }),
                    CreateHelperRow(Localize(_viewModel.BirthTimeHelperKey)),
                    CreateSettingRow(Localize(_viewModel.BirthTimeAccuracyLabelKey), _birthTimeAccuracyComboBox),
                    CreateSettingRow(
                        Localize(_viewModel.BirthPlaceLabelKey),
                        _birthPlaceTextBox,
                        Localize(_viewModel.BirthPlaceHelperKey)),
                    CreateSettingRow(Localize(_viewModel.LatitudeLabelKey), _latitudeTextBox),
                    CreateSettingRow(Localize(_viewModel.LongitudeLabelKey), _longitudeTextBox),
                    CreateSettingRow(
                        Localize(_viewModel.TimezoneLabelKey),
                        _timezoneComboBox,
                        Localize(_viewModel.TimezoneHelperKey)),
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
        var selectedTimezone = _timezoneComboBox?.SelectedItem is TimezoneOption timezoneOption
            ? timezoneOption.TimezoneId
            : string.Empty;
        var selectedTime = selectedAccuracy == BirthTimeAccuracy.UnknownTime
            ? null
            : _birthTimePicker?.SelectedTime;

        _viewModel.UpdateState(
            new BirthDataInputState(
                _birthDatePicker?.SelectedDate,
                selectedTime,
                selectedAccuracy,
                _birthPlaceTextBox?.Text ?? string.Empty,
                _latitudeTextBox?.Text ?? string.Empty,
                _longitudeTextBox?.Text ?? string.Empty,
                selectedTimezone,
                _viewModel.State.LocationSource));
        ApplyBirthTimeInputMode();
        RefreshValidationSummary();
    }

    private void ApplyBirthTimeInputMode()
    {
        if (_birthTimePicker is null || _unknownTimeHelperTextBlock is null)
        {
            return;
        }

        var isUnknownTime = _viewModel.State.BirthTimeAccuracy == BirthTimeAccuracy.UnknownTime;
        _birthTimePicker.IsEnabled = !isUnknownTime;
        _birthTimePicker.Opacity = isUnknownTime ? 0.55 : 1.0;
        _unknownTimeHelperTextBlock.Text = isUnknownTime ? Localize(_viewModel.UnknownTimeHelperKey) : string.Empty;
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

    private static Control CreateSettingRow(string labelText, Control editor, string? helperText = null)
    {
        var stackPanel = new StackPanel
        {
            Spacing = 6
        };

        stackPanel.Children.Add(
            new TextBlock
            {
                Text = labelText,
                FontSize = 14
            });
        stackPanel.Children.Add(editor);

        if (!string.IsNullOrWhiteSpace(helperText))
        {
            stackPanel.Children.Add(
                new TextBlock
                {
                    Text = helperText,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.82
                });
        }

        return stackPanel;
    }

    private Control CreateHelperRow(string helperText) =>
        new TextBlock
        {
            Text = helperText,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.82
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
