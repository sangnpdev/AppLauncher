using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AppLauncher;

public partial class MainWindow : Window
{
    private readonly ConfigStore _configStore = new();
    private LauncherConfig _config = new();
    private int _sharedAppCount;

    public MainWindow()
    {
        InitializeComponent();
        LoadConfig();
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveConfig();
    }

    private void LoadConfig()
    {
        _config = _configStore.Load();

        ProfilesListBox.ItemsSource = _config.Profiles;
        RefreshAppLibrary();
        ProfilesListBox.SelectedIndex = _config.Profiles.Count > 0 ? 0 : -1;
        RefreshProfileDetails();
    }

    private void SaveConfig()
    {
        _configStore.Save(_config);
    }

    private ProfileConfig? GetSelectedProfile()
    {
        return ProfilesListBox.SelectedItem as ProfileConfig;
    }

    private void RefreshProfileDetails()
    {
        var profile = GetSelectedProfile();
        var hasProfile = profile is not null;

        ProfileNameTextBox.IsEnabled = hasProfile;
        ProfileAppsListBox.IsEnabled = hasProfile;
        ExistingAppsComboBox.IsEnabled = hasProfile;
        NewAppNameTextBox.IsEnabled = hasProfile;
        NewAppPathTextBox.IsEnabled = hasProfile;
        BackendFolderTextBox.IsEnabled = hasProfile;
        BackendCommandTextBox.IsEnabled = hasProfile;
        DeleteProfileButton.IsEnabled = hasProfile;
        SaveProfileNameButton.IsEnabled = hasProfile;
        BrowseAppPathButton.IsEnabled = hasProfile;
        AddNewAppButton.IsEnabled = hasProfile;
        BrowseBackendFolderButton.IsEnabled = hasProfile;
        SaveBackendButton.IsEnabled = hasProfile;
        StartSelectedProfileButton.IsEnabled = hasProfile;

        if (!hasProfile)
        {
            ProfileNameTextBox.Text = string.Empty;
            ProfileAppsListBox.ItemsSource = null;
            NewAppNameTextBox.Text = string.Empty;
            NewAppPathTextBox.Text = string.Empty;
            BackendFolderTextBox.Text = string.Empty;
            BackendCommandTextBox.Text = string.Empty;
            UpdateSummary();
            RefreshActionStates();
            return;
        }

        ProfileNameTextBox.Text = profile!.Name;
        ProfileAppsListBox.ItemsSource = profile.Apps;
        ProfileAppsListBox.Items.Refresh();

        BackendFolderTextBox.Text = profile.Backend.FolderPath;
        BackendCommandTextBox.Text = profile.Backend.Command;
        UpdateSummary();
        RefreshActionStates();
    }

    private void RefreshAppLibrary()
    {
        var allApps = _config.Profiles
            .SelectMany(p => p.Apps)
            .GroupBy(a => a.Path, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(a => a.Name)
            .ToList();

        _sharedAppCount = allApps.Count;
        ExistingAppsComboBox.ItemsSource = allApps;
        ExistingAppsComboBox.SelectedIndex = allApps.Count > 0 ? 0 : -1;
        UpdateSummary();
        RefreshActionStates();
    }

    private void UpdateSummary()
    {
        var profile = GetSelectedProfile();

        ProfilesCountTextBlock.Text = _config.Profiles.Count.ToString();
        LibraryCountTextBlock.Text = _sharedAppCount.ToString();

        if (profile is null)
        {
            SelectedProfileHeaderTextBlock.Text = "No profile selected";
            SelectedProfileSubtitleTextBlock.Text = "Create or select a profile to manage apps and backend tasks.";
            AppCountTextBlock.Text = "0 apps ready";
            BackendStatusTextBlock.Text = "No backend task";
            StartSelectedProfileButton.Content = "Start Selected Profile";
            return;
        }

        var appCount = profile.Apps.Count;
        var hasBackend = !string.IsNullOrWhiteSpace(profile.Backend.FolderPath)
            && !string.IsNullOrWhiteSpace(profile.Backend.Command);

        SelectedProfileHeaderTextBlock.Text = profile.Name;
        SelectedProfileSubtitleTextBlock.Text = appCount == 0
            ? "Add desktop apps below, then launch everything in one click."
            : "Launch the full workspace, including optional backend tasks, from one place.";
        AppCountTextBlock.Text = appCount == 1 ? "1 app ready" : $"{appCount} apps ready";
        BackendStatusTextBlock.Text = hasBackend ? "Backend task ready" : "No backend task";
        StartSelectedProfileButton.Content = $"Start {profile.Name}";
    }

    private void RefreshActionStates()
    {
        var hasProfile = GetSelectedProfile() is not null;
        AddExistingAppButton.IsEnabled = hasProfile && ExistingAppsComboBox.SelectedItem is AppEntry;
        RemoveSelectedAppButton.IsEnabled = hasProfile && ProfileAppsListBox.SelectedItem is AppEntry;
    }

    private void ProfilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshProfileDetails();
    }

    private void ExistingAppsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshActionStates();
    }

    private void ProfileAppsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshActionStates();
    }

    private void AddProfile_Click(object sender, RoutedEventArgs e)
    {
        var index = _config.Profiles.Count + 1;
        var profileName = $"Profile {index}";
        while (_config.Profiles.Any(p => string.Equals(p.Name, profileName, StringComparison.OrdinalIgnoreCase)))
        {
            index++;
            profileName = $"Profile {index}";
        }

        var profile = new ProfileConfig { Name = profileName };
        _config.Profiles.Add(profile);
        ProfilesListBox.Items.Refresh();
        ProfilesListBox.SelectedItem = profile;

        RefreshAppLibrary();
        RefreshProfileDetails();
        SaveConfig();
    }

    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            return;
        }

        var result = MessageBox.Show(
            $"Delete profile '{profile.Name}'?",
            "Confirm",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var index = ProfilesListBox.SelectedIndex;
        _config.Profiles.Remove(profile);
        ProfilesListBox.Items.Refresh();

        if (_config.Profiles.Count == 0)
        {
            ProfilesListBox.SelectedIndex = -1;
        }
        else
        {
            ProfilesListBox.SelectedIndex = Math.Clamp(index, 0, _config.Profiles.Count - 1);
        }

        RefreshAppLibrary();
        RefreshProfileDetails();
        SaveConfig();
    }

    private void SaveProfileName_Click(object sender, RoutedEventArgs e)
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            return;
        }

        var newName = ProfileNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            MessageBox.Show("Profile name cannot be empty.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        profile.Name = newName;
        ProfilesListBox.Items.Refresh();
        UpdateSummary();
        SaveConfig();
    }

    private void BrowseAppPath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select app executable",
            Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == true)
        {
            NewAppPathTextBox.Text = dialog.FileName;
            if (string.IsNullOrWhiteSpace(NewAppNameTextBox.Text))
            {
                NewAppNameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }
    }

    private void AddNewApp_Click(object sender, RoutedEventArgs e)
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            MessageBox.Show("Please select a profile first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var appPath = NewAppPathTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(appPath))
        {
            MessageBox.Show("Please enter app path.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var appName = NewAppNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(appName))
        {
            appName = Path.GetFileNameWithoutExtension(appPath);
        }

        if (profile.Apps.Any(a => string.Equals(a.Path, appPath, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("This app path already exists in the selected profile.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        profile.Apps.Add(new AppEntry
        {
            Name = appName,
            Path = appPath
        });

        ProfilesListBox.Items.Refresh();
        ProfileAppsListBox.Items.Refresh();
        RefreshAppLibrary();
        UpdateSummary();
        SaveConfig();

        NewAppNameTextBox.Text = string.Empty;
        NewAppPathTextBox.Text = string.Empty;
    }

    private void AddExistingApp_Click(object sender, RoutedEventArgs e)
    {
        var profile = GetSelectedProfile();
        var selectedApp = ExistingAppsComboBox.SelectedItem as AppEntry;

        if (profile is null || selectedApp is null)
        {
            return;
        }

        if (profile.Apps.Any(a => string.Equals(a.Path, selectedApp.Path, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Selected app already exists in this profile.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        profile.Apps.Add(new AppEntry
        {
            Name = selectedApp.Name,
            Path = selectedApp.Path
        });

        ProfilesListBox.Items.Refresh();
        ProfileAppsListBox.Items.Refresh();
        UpdateSummary();
        SaveConfig();
    }

    private void RemoveSelectedApp_Click(object sender, RoutedEventArgs e)
    {
        var profile = GetSelectedProfile();
        var selectedApp = ProfileAppsListBox.SelectedItem as AppEntry;
        if (profile is null || selectedApp is null)
        {
            return;
        }

        profile.Apps.Remove(selectedApp);
        ProfilesListBox.Items.Refresh();
        ProfileAppsListBox.Items.Refresh();
        RefreshAppLibrary();
        UpdateSummary();
        SaveConfig();
    }

    private void BrowseBackendFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select backend folder"
        };

        if (dialog.ShowDialog() == true)
        {
            BackendFolderTextBox.Text = dialog.FolderName;
        }
    }

    private void SaveBackend_Click(object sender, RoutedEventArgs e)
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            return;
        }

        profile.Backend.FolderPath = BackendFolderTextBox.Text.Trim();
        profile.Backend.Command = BackendCommandTextBox.Text.Trim();
        ProfilesListBox.Items.Refresh();
        UpdateSummary();
        SaveConfig();
    }

    private void StartSelectedProfile_Click(object sender, RoutedEventArgs e)
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            MessageBox.Show("Please select a profile.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveBackend_Click(sender, e);

        var startedCount = 0;
        var errors = new StringBuilder();

        foreach (var app in profile.Apps)
        {
            if (string.IsNullOrWhiteSpace(app.Path) || !File.Exists(app.Path))
            {
                errors.AppendLine($"- App not found: {app.Name} ({app.Path})");
                continue;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = app.Path,
                    WorkingDirectory = Path.GetDirectoryName(app.Path) ?? Environment.CurrentDirectory,
                    UseShellExecute = true
                });
                startedCount++;
            }
            catch (Exception ex)
            {
                errors.AppendLine($"- Cannot start app '{app.Name}': {ex.Message}");
            }
        }

        if (!string.IsNullOrWhiteSpace(profile.Backend.FolderPath) && !string.IsNullOrWhiteSpace(profile.Backend.Command))
        {
            if (!Directory.Exists(profile.Backend.FolderPath))
            {
                errors.AppendLine($"- Backend folder not found: {profile.Backend.FolderPath}");
            }
            else
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/k {profile.Backend.Command}",
                        WorkingDirectory = profile.Backend.FolderPath,
                        UseShellExecute = true
                    });
                    startedCount++;
                }
                catch (Exception ex)
                {
                    errors.AppendLine($"- Cannot run backend command: {ex.Message}");
                }
            }
        }

        if (errors.Length == 0)
        {
            MessageBox.Show($"Started {startedCount} item(s).", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var message = $"Started {startedCount} item(s) with issues:\n\n{errors}";
        MessageBox.Show(message, "Completed With Warnings", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}

public sealed class LauncherConfig
{
    public List<ProfileConfig> Profiles { get; set; } = [];
}

public sealed class ProfileConfig
{
    public string Name { get; set; } = "Profile";
    public List<AppEntry> Apps { get; set; } = [];
    public BackendConfig Backend { get; set; } = new();

    public string AppCountLabel => Apps.Count == 1 ? "1 app" : $"{Apps.Count} apps";
    public string BackendStatusLabel => string.IsNullOrWhiteSpace(Backend.Command) ? "No backend" : "Backend ready";
}

public sealed class AppEntry
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Name} - {Path}";
    }
}

public sealed class BackendConfig
{
    public string FolderPath { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}
