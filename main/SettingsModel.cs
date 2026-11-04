using System;
using System.IO;
using System.Text.Json;

namespace Ketch
{
    public class SettingsModel
    {
        private const string SettingsFileName = "settings.json";
        private const string LegacySettingsFileName = "ketch.settings.json";

        public string Background { get; set; } = "Green";
        public string BackgroundHex { get; set; } = "#70B070";

        public string Font { get; set; } = "Green";
        public string FontHex { get; set; } = "#385030";

        public int Opacity { get; set; } = 100;
        public string DisplayLocation { get; set; } = "Bottom Right";
        public string Size { get; set; } = "Medium";
        public string TimeFormat { get; set; } = "24 hs.";
        public bool ShowSeconds { get; set; } = false;

        public static (SettingsModel settings, string path) LoadWithAutoPath()
        {
            string baseDirectory = AppContext.BaseDirectory;
            string localDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ketch");

            string exePath = Path.Combine(baseDirectory, SettingsFileName);
            string localPath = Path.Combine(localDirectory, SettingsFileName);

            // Prefer the renamed file. Keep reading the old name to preserve existing user settings.
            if (TryLoad(exePath, out var settings)) return (settings, exePath);
            if (TryLoad(localPath, out settings)) return (settings, localPath);

            string legacyExePath = Path.Combine(baseDirectory, LegacySettingsFileName);
            string legacyLocalPath = Path.Combine(localDirectory, LegacySettingsFileName);
            if (TryLoad(legacyExePath, out settings)) return (settings, exePath);
            if (TryLoad(legacyLocalPath, out settings)) return (settings, localPath);

            // Do not create a settings file on startup; it will be written on the first change.
            return (new SettingsModel(), exePath);
        }

        private static bool TryLoad(string path, out SettingsModel settings)
        {
            settings = new SettingsModel();
            if (!File.Exists(path)) return false;

            try
            {
                var loaded = JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(path));
                if (loaded == null) return false;
                settings = loaded;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string Save(string path)
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                string? directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(path, json);
                return path;
            }
            catch
            {
                try
                {
                    string localDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ketch");
                    Directory.CreateDirectory(localDirectory);
                    string localPath = Path.Combine(localDirectory, SettingsFileName);
                    File.WriteAllText(localPath, json);
                    return localPath;
                }
                catch
                {
                    // Preserve the intended path if both locations are unavailable.
                    return path;
                }
            }
        }
    }
}
