using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TanScreenshot
{
    public static class ConfigManager
    {
        private static readonly string ConfigPath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "config.json");

        public static AppConfig LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<AppConfig>(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load config: {ex.Message}",
                                "Config Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            return new AppConfig();
        }

        public static void SaveConfig(AppConfig config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save config: {ex.Message}",
                                "Config Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        public static Keys[] ConvertToKeys(List<string> keyNames)
        {
            if (keyNames == null)
            {
                return new[] { Keys.PrintScreen };
            }

            var keys = keyNames
                .Select(name => Enum.TryParse<Keys>(name, out var key) ? key : (Keys?)null)
                .Where(key => key.HasValue)
                .Select(key => key.Value)
                .ToArray();

            return keys.Length > 0 ? keys : new[] { Keys.PrintScreen };
        }
    }
}