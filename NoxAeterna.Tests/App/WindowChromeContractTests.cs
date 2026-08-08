using System.Globalization;
using System.Xml.Linq;
using NoxAeterna.App.Shell;

namespace NoxAeterna.Tests.App;

public sealed class WindowChromeContractTests
{
    [Theory]
    [InlineData(DesktopWindowPlatform.Windows, true)]
    [InlineData(DesktopWindowPlatform.MacOS, false)]
    [InlineData(DesktopWindowPlatform.Linux, false)]
    [InlineData(DesktopWindowPlatform.Other, false)]
    public void PlatformPolicy_UsesProjectChromeOnlyOnWindows(
        DesktopWindowPlatform platform,
        bool expected)
    {
        Assert.Equal(expected, WindowChromePolicy.UsesProjectOwnedChrome(platform));
    }

    [Fact]
    public void MainWindow_RetainsDesktopSizeContractAndReservesOnlyCompactWindowsCaptionRow()
    {
        var document = XDocument.Load(AppPath("MainWindow.axaml"));
        var window = Assert.IsType<XElement>(document.Root);
        var spacer = Assert.Single(window.Descendants(), element =>
            GetAttribute(element, "Name") == "WindowCaptionSpacer");

        Assert.Equal("1360", (string?)window.Attribute("Width"));
        Assert.Equal("860", (string?)window.Attribute("Height"));
        Assert.Equal("1180", (string?)window.Attribute("MinWidth"));
        Assert.Equal("760", (string?)window.Attribute("MinHeight"));
        Assert.Equal("{DynamicResource NoxAeternaWindowDecorationsTheme}",
            (string?)window.Attribute("WindowDecorationsTheme"));
        Assert.Equal("36", (string?)spacer.Attribute("Height"));
        Assert.Contains(
            "_windowCaptionSpacer.IsVisible = _usesProjectChrome",
            File.ReadAllText(AppPath("MainWindow.axaml.cs")),
            StringComparison.Ordinal);
    }

    [Fact]
    public void DecorationTheme_ProvidesNativeTitleAndCaptionButtonRolesWithoutManualClicks()
    {
        var document = XDocument.Load(AppPath("Themes", "SemanticControlStyles.axaml"));
        var roleOwners = document.Descendants()
            .Where(element => GetAttribute(element, "WindowDecorationProperties.ElementRole") is not null)
            .ToDictionary(
                element => GetAttribute(element, "Name") ?? element.Name.LocalName,
                element => GetAttribute(element, "WindowDecorationProperties.ElementRole")!);

        Assert.Equal("TitleBar", roleOwners["PART_TitleBar"]);
        Assert.Equal("MinimizeButton", roleOwners["PART_MinimizeButton"]);
        Assert.Equal("MaximizeButton", roleOwners["PART_MaximizeButton"]);
        Assert.Equal("CloseButton", roleOwners["PART_CloseButton"]);

        var source = File.ReadAllText(AppPath("Themes", "SemanticControlStyles.axaml"));
        Assert.Contains("WindowDrawnDecorationsTemplate", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Click=", source, StringComparison.Ordinal);
        Assert.Contains("DesignCanvasBrush", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MinimizeButton_UsesNonDegenerateCompactDashWithNativeRole()
    {
        var path = AppPath("Themes", "SemanticControlStyles.axaml");
        var document = XDocument.Load(path);
        var minimizeButton = Assert.Single(document.Descendants(), element =>
            GetAttribute(element, "Name") == "PART_MinimizeButton");
        var glyph = Assert.Single(minimizeButton.Descendants(), element =>
            GetAttribute(element, "Name") == "WindowCaptionMinimizeIcon");
        var width = double.Parse(Assert.IsType<XAttribute>(glyph.Attribute("Width")).Value, CultureInfo.InvariantCulture);
        var height = double.Parse(Assert.IsType<XAttribute>(glyph.Attribute("Height")).Value, CultureInfo.InvariantCulture);

        Assert.Equal("MinimizeButton", GetAttribute(minimizeButton, "WindowDecorationProperties.ElementRole"));
        Assert.Equal("Rectangle", glyph.Name.LocalName);
        Assert.InRange(width, 9, 12);
        Assert.InRange(height, 1, 2);
        Assert.True(width > height);
        Assert.Equal("Center", (string?)glyph.Attribute("HorizontalAlignment"));
        Assert.Equal("Center", (string?)glyph.Attribute("VerticalAlignment"));
        Assert.Equal("{DynamicResource DesignTextPrimaryBrush}", (string?)glyph.Attribute("Fill"));
        Assert.Equal("False", (string?)glyph.Attribute("IsHitTestVisible"));

        var source = File.ReadAllText(path);
        Assert.DoesNotContain("M3,12 L21,12", source, StringComparison.Ordinal);
        Assert.Null(glyph.Attribute("Stretch"));
    }

    [Fact]
    public void CaptionButtons_OwnHoverPressedFocusAndRestrainedCloseStates()
    {
        var source = File.ReadAllText(AppPath("Themes", "SemanticControlStyles.axaml"));

        Assert.Contains("Button.window-caption-button:pointerover", source, StringComparison.Ordinal);
        Assert.Contains("Button.window-caption-button:pressed", source, StringComparison.Ordinal);
        Assert.Contains("Button.window-caption-button:focus-visible", source, StringComparison.Ordinal);
        Assert.Contains("Button.window-caption-close:pointerover", source, StringComparison.Ordinal);
        Assert.Contains("DesignErrorBrush", source, StringComparison.Ordinal);
        Assert.Contains("DesignFocusRingBrush", source, StringComparison.Ordinal);
        Assert.DoesNotMatch("#[0-9A-Fa-f]{6,8}\\b", source);
    }

    [Fact]
    public void MaximizeButton_HasDistinctRestorePresentationForMaximizedState()
    {
        var source = File.ReadAllText(AppPath("Themes", "SemanticControlStyles.axaml"));

        Assert.Contains("WindowCaptionMaximizeIcon", source, StringComparison.Ordinal);
        Assert.Contains("WindowCaptionRestoreIcon", source, StringComparison.Ordinal);
        Assert.Contains("^:maximized /template/ Path#WindowCaptionMaximizeIcon", source, StringComparison.Ordinal);
        Assert.Contains("^:maximized /template/ Path#WindowCaptionRestoreIcon", source, StringComparison.Ordinal);
        Assert.Contains("ui.shell.window.restore", File.ReadAllText(AppPath("MainWindow.axaml.cs")), StringComparison.Ordinal);
    }

    [Fact]
    public void PlacementLifecycle_CapturesInMemoryAndPersistsOnlyFromSuccessfulClosing()
    {
        var coordinator = File.ReadAllText(AppPath("Shell", "WindowPlacementCoordinator.cs"));
        var window = File.ReadAllText(AppPath("MainWindow.axaml.cs"));

        Assert.Contains("PositionChanged += OnPositionChanged", coordinator, StringComparison.Ordinal);
        Assert.Contains("SizeChanged += OnSizeChanged", coordinator, StringComparison.Ordinal);
        Assert.DoesNotContain("ApplyWindowPlacement", coordinator[..coordinator.IndexOf("PersistAtClose", StringComparison.Ordinal)], StringComparison.Ordinal);
        Assert.Equal(1, Count(window, ".PersistAtClose()"));
        Assert.Contains("if (!e.Cancel)", window, StringComparison.Ordinal);
    }

    [Fact]
    public void WindowsSystemMenu_IsNarrowAppOwnedAltSpaceInteropAndIsDisposedWithWindow()
    {
        var coordinator = File.ReadAllText(AppPath("Shell", "WindowsSystemMenuCoordinator.cs"));
        var window = File.ReadAllText(AppPath("MainWindow.axaml.cs"));

        Assert.Contains("Win32Properties.AddWndProcHookCallback", coordinator, StringComparison.Ordinal);
        Assert.Contains("systemCommandKeyMenu", coordinator, StringComparison.Ordinal);
        Assert.Contains("GetSystemMenu", coordinator, StringComparison.Ordinal);
        Assert.Contains("TrackPopupMenuEx", coordinator, StringComparison.Ordinal);
        Assert.Contains("EnableMenuItem", coordinator, StringComparison.Ordinal);
        Assert.Contains("SystemCommandClose", coordinator, StringComparison.Ordinal);
        Assert.Contains("WindowMessageSystemCommand", coordinator, StringComparison.Ordinal);
        Assert.DoesNotContain("user32", File.ReadAllText(PresentationPath("Shell", "WindowPlacementRepairCalculator.cs")), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("_windowsSystemMenuCoordinator?.Dispose()", window, StringComparison.Ordinal);
    }

    private static int Count(string source, string value)
    {
        var count = 0;
        var start = 0;
        while ((start = source.IndexOf(value, start, StringComparison.Ordinal)) >= 0)
        {
            count++;
            start += value.Length;
        }

        return count;
    }

    private static string? GetAttribute(XElement element, string localName) =>
        element.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == localName)?.Value;

    private static string AppPath(params string[] segments)
    {
        var pathSegments = new[]
        {
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "NoxAeterna.App"
        }.Concat(segments).ToArray();
        return Path.GetFullPath(Path.Combine(pathSegments));
    }

    private static string PresentationPath(params string[] segments)
    {
        var pathSegments = new[]
        {
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "NoxAeterna.Presentation"
        }.Concat(segments).ToArray();
        return Path.GetFullPath(Path.Combine(pathSegments));
    }
}
