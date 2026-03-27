using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AppLauncher;

public partial class AppDialogWindow : Window
{
    private readonly AppDialogOptions _options;

    public AppDialogWindow(AppDialogOptions options)
    {
        _options = options;
        InitializeComponent();
        ApplyOptions();
    }

    private void ApplyOptions()
    {
        var palette = _options.Tone switch
        {
            AppDialogTone.Success => new DialogPalette("Completed", "OK", "#E8F7EF", "#067647", "#CFF3DD", "#067647"),
            AppDialogTone.Warning => new DialogPalette("Attention", "!", "#FFF3E5", "#B54708", "#FFE2C0", "#F7A55A"),
            AppDialogTone.Danger => new DialogPalette("Sensitive Action", "!", "#FEE4E2", "#B42318", "#FCD5D2", "#F97066"),
            AppDialogTone.Question => new DialogPalette("Confirm Action", "?", "#E8F3FF", "#0F6CBD", "#D7E9FD", "#5AA7E8"),
            _ => new DialogPalette("Information", "i", "#E8F3FF", "#0F6CBD", "#D7E9FD", "#5AA7E8")
        };

        Title = _options.Title;
        ToneBadgeTextBlock.Text = palette.BadgeLabel;
        ToneBadgeTextBlock.Foreground = CreateBrush(palette.ForegroundHex);
        ToneBadgeBorder.Background = CreateBrush(palette.BadgeBackgroundHex);
        IconTextBlock.Text = palette.Icon;
        IconTextBlock.FontSize = palette.Icon.Length > 1 ? 22 : 34;
        IconTextBlock.Foreground = CreateBrush(palette.ForegroundHex);
        IconHostBorder.Background = CreateBrush(palette.IconBackgroundHex);
        TitleTextBlock.Text = _options.Title;
        MessageTextBlock.Text = _options.Message;
        AccentStripBorder.Background = new LinearGradientBrush(
            (Color)ColorConverter.ConvertFromString(palette.ForegroundHex),
            (Color)ColorConverter.ConvertFromString(palette.AccentHex),
            new Point(0, 0),
            new Point(1, 0));

        PrimaryActionButton.Content = _options.PrimaryButtonText;
        PrimaryActionButton.IsDefault = true;

        if (string.IsNullOrWhiteSpace(_options.SecondaryButtonText))
        {
            SecondaryActionButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            SecondaryActionButton.Content = _options.SecondaryButtonText;
            SecondaryActionButton.Visibility = Visibility.Visible;
            SecondaryActionButton.IsCancel = true;
        }

        if (_options.IsPrimaryDestructive)
        {
            PrimaryActionButton.Background = CreateBrush("#C9382B");
            PrimaryActionButton.BorderBrush = CreateBrush("#C9382B");
            PrimaryActionButton.Foreground = Brushes.White;
        }
        else
        {
            PrimaryActionButton.Background = CreateBrush("#0F6CBD");
            PrimaryActionButton.BorderBrush = CreateBrush("#0F6CBD");
            PrimaryActionButton.Foreground = Brushes.White;
        }
    }

    private static SolidColorBrush CreateBrush(string hex)
    {
        return (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
    }

    private void PrimaryActionButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void SecondaryActionButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
        }
    }

    private sealed record DialogPalette(
        string BadgeLabel,
        string Icon,
        string BadgeBackgroundHex,
        string ForegroundHex,
        string IconBackgroundHex,
        string AccentHex);
}
