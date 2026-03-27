using System.Windows;

namespace AppLauncher;

public enum AppDialogTone
{
    Info,
    Success,
    Warning,
    Danger,
    Question
}

public sealed class AppDialogOptions
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string PrimaryButtonText { get; init; } = "OK";
    public string? SecondaryButtonText { get; init; }
    public AppDialogTone Tone { get; init; } = AppDialogTone.Info;
    public bool IsPrimaryDestructive { get; init; }
}

public static class AppDialog
{
    public static void ShowInfo(Window? owner, string title, string message)
    {
        Show(owner, new AppDialogOptions
        {
            Title = title,
            Message = message,
            Tone = AppDialogTone.Info,
            PrimaryButtonText = "OK"
        });
    }

    public static void ShowSuccess(Window? owner, string title, string message)
    {
        Show(owner, new AppDialogOptions
        {
            Title = title,
            Message = message,
            Tone = AppDialogTone.Success,
            PrimaryButtonText = "Done"
        });
    }

    public static void ShowWarning(Window? owner, string title, string message)
    {
        Show(owner, new AppDialogOptions
        {
            Title = title,
            Message = message,
            Tone = AppDialogTone.Warning,
            PrimaryButtonText = "Understood"
        });
    }

    public static void ShowMessage(Window? owner, AppDialogOptions options)
    {
        Show(owner, options);
    }

    public static bool ShowConfirm(
        Window? owner,
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string secondaryButtonText = "Cancel",
        AppDialogTone tone = AppDialogTone.Question,
        bool isPrimaryDestructive = false)
    {
        return Show(owner, new AppDialogOptions
        {
            Title = title,
            Message = message,
            Tone = tone,
            PrimaryButtonText = primaryButtonText,
            SecondaryButtonText = secondaryButtonText,
            IsPrimaryDestructive = isPrimaryDestructive
        });
    }

    private static bool Show(Window? owner, AppDialogOptions options)
    {
        var dialog = new AppDialogWindow(options);
        if (owner is not null)
        {
            dialog.Owner = owner;
        }

        return dialog.ShowDialog() == true;
    }
}
