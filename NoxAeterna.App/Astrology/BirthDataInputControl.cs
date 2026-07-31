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
    private readonly Func<BirthDataInputViewModel, bool>? _buildChartFromInput;
    private DatePicker? _birthDatePicker;
    private TimePicker? _birthTimePicker;
    private ComboBox? _birthTimeAccuracyComboBox;
    private TextBox? _birthPlaceTextBox;
    private TextBox? _latitudeTextBox;
    private TextBox? _longitudeTextBox;
    private ComboBox? _timezoneComboBox;
    private TextBlock? _validationSummaryTextBlock;
    private TextBlock? _unknownTimeHelperTextBlock;
    private bool _isApplyingBirthTimeInputMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="BirthDataInputControl"/> class.
    /// </summary>
    public BirthDataInputControl(
        BirthDataInputViewModel viewModel,
        ILocalizationProvider localizationProvider,
        LanguageCode applicationLanguage,
        Func<BirthDataInputViewModel, bool>? buildChartFromInput = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _localizationProvider = localizationProvider ?? throw new ArgumentNullException(nameof(localizationProvider));
        _applicationLanguage = applicationLanguage;
        _buildChartFromInput = buildChartFromInput;

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
            SelectedTime = _viewModel.BirthTimeEditorValue
        };
        _birthTimePicker.PropertyChanged += (_, args) =>
        {
            if (!_isApplyingBirthTimeInputMode &&
                args.Property == TimePicker.SelectedTimeProperty)
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
            Margin = new Thickness(0, 4, 0, 0),
            Content = Localize(_viewModel.ValidateActionKey)
        };
        validateButton.Classes.Add("accent");
        validateButton.Classes.Add("primary-action");
        validateButton.Click += (_, _) =>
        {
            SyncStateFromInputs();
            if (_buildChartFromInput is not null)
            {
                _buildChartFromInput(_viewModel);
            }
            else
            {
                _viewModel.TryCreateBirthData(out _);
            }

            RefreshValidationSummary();
        };

        _validationSummaryTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap
        };
        _validationSummaryTextBlock.Classes.Add("validation-error");
        _unknownTimeHelperTextBlock = new TextBlock
        {
            Text = string.Empty,
            TextWrapping = TextWrapping.Wrap
        };
        _unknownTimeHelperTextBlock.Classes.Add("supporting");

        ApplyBirthTimeInputMode();

        return new StackPanel
        {
            Margin = new Thickness(4, 2, 4, 0),
            Spacing = 14,
            Children =
            {
                CreateSettingRow(Localize(_viewModel.BirthDateLabelKey), _birthDatePicker),
                CreateSettingRow(
                    Localize(_viewModel.BirthTimeLabelKey),
                    new StackPanel
                    {
                        Spacing = 5,
                        Children =
                        {
                            _birthTimePicker,
                            _unknownTimeHelperTextBlock
                        }
                    }),
                CreateSettingRow(Localize(_viewModel.BirthTimeAccuracyLabelKey), _birthTimeAccuracyComboBox),
                CreateSettingRow(Localize(_viewModel.BirthPlaceLabelKey), _birthPlaceTextBox),
                CreateTwoColumnGroup(
                    CreateSettingRow(Localize(_viewModel.LatitudeLabelKey), _latitudeTextBox),
                    CreateSettingRow(Localize(_viewModel.LongitudeLabelKey), _longitudeTextBox)),
                CreateSettingRow(Localize(_viewModel.TimezoneLabelKey), _timezoneComboBox),
                validateButton,
                _validationSummaryTextBlock
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
        var selectedTime = _birthTimePicker?.SelectedTime;

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
        _isApplyingBirthTimeInputMode = true;
        try
        {
            _birthTimePicker.SelectedTime = _viewModel.BirthTimeEditorValue;
            _birthTimePicker.IsEnabled = !isUnknownTime;
            _unknownTimeHelperTextBlock.Text = isUnknownTime
                ? Localize(_viewModel.UnknownTimeStatusKey)
                : string.Empty;
        }
        finally
        {
            _isApplyingBirthTimeInputMode = false;
        }
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
            SetValidationClass("validation-success");
            return;
        }

        _validationSummaryTextBlock.Text = string.Join(
            Environment.NewLine,
            _viewModel.ValidationResult.Errors.Select(error => Localize(error.MessageKey)));
        SetValidationClass("validation-error");
    }

    private void SetValidationClass(string className)
    {
        if (_validationSummaryTextBlock is null)
        {
            return;
        }

        _validationSummaryTextBlock.Classes.Remove("validation-success");
        _validationSummaryTextBlock.Classes.Remove("validation-error");
        _validationSummaryTextBlock.Classes.Remove("validation-warning");
        _validationSummaryTextBlock.Classes.Add(className);
    }

    private static Control CreateSettingRow(string labelText, Control editor)
    {
        var stackPanel = new StackPanel
        {
            Spacing = 6
        };

        stackPanel.Children.Add(
            new TextBlock
            {
                Text = labelText,
                FontSize = 14,
                FontWeight = FontWeight.Medium
            });
        stackPanel.Children.Add(editor);

        return stackPanel;
    }

    private static Control CreateTwoColumnGroup(Control left, Control right)
    {
        Grid.SetColumn(left, 0);
        Grid.SetColumn(right, 1);

        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 12,
            Children =
            {
                left,
                right
            }
        };
    }

    private static TextBox CreateTextBox(string initialText, string watermark) =>
        new()
        {
            Text = initialText,
            PlaceholderText = string.IsNullOrWhiteSpace(watermark) ? null : watermark,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _applicationLanguage, key).Text;

    private sealed record LocalizedBirthTimeAccuracyOption(BirthTimeAccuracyOption Option, string Label)
    {
        public override string ToString() => Label;
    }
}
