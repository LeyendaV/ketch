using System;
using System.IO;
using System.Text.Json;

namespace Ketch
{
    public class SettingsModel
    {
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
            string exePath = Path.Combine(AppContext.BaseDirectory, "ketch.settings.json");
            string localDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ketch");
            string localPath = Path.Combine(localDir, "ketch.settings.json");

            if (File.Exists(exePath))
            {
                try
                {
                    var text = File.ReadAllText(exePath);
                    var result = JsonSerializer.Deserialize<SettingsModel>(text);
                    if (result != null) return (result, exePath);
                }
                catch { }
            }

            if (File.Exists(localPath))
            {
                try
                {
                    var text = File.ReadAllText(localPath);
                    var result = JsonSerializer.Deserialize<SettingsModel>(text);
                    if (result != null) return (result, localPath);
                }
                catch { }
            }

            try
            {
                File.WriteAllText(exePath + ".tmp", "{}");
                File.Delete(exePath + ".tmp");
                return (new SettingsModel(), exePath);
            }
            catch
            {
                try
                {
                    if (!Directory.Exists(localDir)) Directory.CreateDirectory(localDir);
                    return (new SettingsModel(), localPath);
                }
                catch
                {
                    return (new SettingsModel(), exePath);
                }
            }
        }

        public string Save(string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
                return path;
            }
            catch
            {
                try
                {
                    string localDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ketch");
                    if (!Directory.Exists(localDir)) Directory.CreateDirectory(localDir);
                    string localPath = Path.Combine(localDir, "ketch.settings.json");
                    File.WriteAllText(localPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
                    return localPath;
                }
                catch
                {
                    return path;
                }
            }
        }
    }
}
