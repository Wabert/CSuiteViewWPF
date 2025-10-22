using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace CSuiteViewWPF.Services
{
    public class ThemeService : IThemeService
    {
        private const string SettingsFileName = "settings.json";
        private const string AppFolderName = "CSuiteViewWPF";
        private readonly string _settingsPath;
        private AppTheme _current;

        public static ThemeService Instance { get; } = new ThemeService();

        private ThemeService()
        {
            _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppFolderName);
            Directory.CreateDirectory(_settingsPath);
            _current = LoadPersistedTheme();
            ApplyTheme(_current, raise:false);
        }

        public event EventHandler<AppTheme>? ThemeChanged;

        public AppTheme GetCurrentTheme() => _current;

        public void SetTheme(AppTheme theme)
        {
            if (theme == _current) return;
            _current = theme;
            ApplyTheme(theme, raise:true);
            PersistTheme(theme);
        }

        private void ApplyTheme(AppTheme theme, bool raise)
        {
            var app = Application.Current;
            if (app == null) return;

            // Find existing theme dictionary (Light.xaml or Dark.xaml) to replace
            var themeIndex = -1;
            for (int i = 0; i < app.Resources.MergedDictionaries.Count; i++)
            {
                var src = app.Resources.MergedDictionaries[i].Source?.ToString() ?? string.Empty;
                if (src.Contains("/Themes/Light.xaml") || src.Contains("/Themes/Dark.xaml") || src.Contains("/Themes/Royal.xaml"))
                {
                    themeIndex = i;
                    break;
                }
            }

            var uri = theme switch
            {
                AppTheme.Light => new Uri("/Themes/Light.xaml", UriKind.Relative),
                AppTheme.Dark => new Uri("/Themes/Dark.xaml", UriKind.Relative),
                AppTheme.Royal => new Uri("/Themes/Royal.xaml", UriKind.Relative),
                _ => new Uri("/Themes/Royal.xaml", UriKind.Relative)
            };
            var newDict = new ResourceDictionary { Source = uri };

            if (themeIndex >= 0)
                app.Resources.MergedDictionaries[themeIndex] = newDict;
            else
                app.Resources.MergedDictionaries.Insert(0, newDict);

            if (raise) ThemeChanged?.Invoke(this, _current);
        }

        private void PersistTheme(AppTheme theme)
        {
            var file = Path.Combine(_settingsPath, SettingsFileName);
            var json = JsonSerializer.Serialize(new { theme = theme.ToString() });
            File.WriteAllText(file, json);
        }

        private AppTheme LoadPersistedTheme()
        {
            try
            {
                var file = Path.Combine(_settingsPath, SettingsFileName);
                if (!File.Exists(file)) return AppTheme.Royal;
                var json = File.ReadAllText(file);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("theme", out var prop))
                {
                    var s = prop.GetString();
                    if (Enum.TryParse<AppTheme>(s, out var t)) return t;
                }
            }
            catch { }
            return AppTheme.Royal;
        }
    }
}
