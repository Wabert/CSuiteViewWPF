using System;

namespace CSuiteViewWPF.Services
{
    public enum AppTheme { Light, Dark, Royal }

    public interface IThemeService
    {
        AppTheme GetCurrentTheme();
        void SetTheme(AppTheme theme);
        event EventHandler<AppTheme>? ThemeChanged;
    }
}
