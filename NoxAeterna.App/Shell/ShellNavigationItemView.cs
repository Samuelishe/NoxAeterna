using System.ComponentModel;
using System.Runtime.CompilerServices;
using NoxAeterna.Presentation.Shell;

namespace NoxAeterna.App.Shell;

public sealed class ShellNavigationItemView : INotifyPropertyChanged
{
    private bool _isLabelVisible;

    public ShellNavigationItemView(ShellNavigationItem item, string label, bool isLabelVisible)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Label = string.IsNullOrWhiteSpace(label)
            ? throw new ArgumentException("A localized navigation label is required.", nameof(label))
            : label;
        IconGeometry = ShellNavigationIconCatalog.CreateGeometry(item.IconId);
        _isLabelVisible = isLabelVisible;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ShellNavigationItem Item { get; }

    public string Label { get; }

    public Avalonia.Media.Geometry IconGeometry { get; }

    public bool IsLabelVisible
    {
        get => _isLabelVisible;
        set
        {
            if (_isLabelVisible == value)
            {
                return;
            }

            _isLabelVisible = value;
            OnPropertyChanged();
        }
    }

    public override string ToString() => Label;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
