using System.IO;
using System.Text.Json;

namespace AppLauncher;

public sealed class ConfigStore
{
    private readonly string _configFilePath;

    public ConfigStore()
    {
        var configDirectory = ResolveConfigDirectory();
        Directory.CreateDirectory(configDirectory);
        _configFilePath = Path.Combine(configDirectory, "profiles.json");
    }

    public LauncherConfig Load()
    {
        if (!File.Exists(_configFilePath))
        {
            return new LauncherConfig();
        }

        try
        {
            var json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<LauncherConfig>(json) ?? new LauncherConfig();
        }
        catch
        {
            return new LauncherConfig();
        }
    }

    public void Save(LauncherConfig config)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(config, options);
        File.WriteAllText(_configFilePath, json);
    }

    private static string ResolveConfigDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var csprojPath = Path.Combine(directory.FullName, "AppLauncher.csproj");
            if (File.Exists(csprojPath))
            {
                return Path.Combine(directory.FullName, "config");
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "config");
    }
}
