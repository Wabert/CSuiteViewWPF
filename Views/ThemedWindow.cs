using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using CSuiteViewWPF.Services;

namespace CSuiteViewWPF.Views
{
    // Base window class that applies chrome, template, routed commands, and theme handling
    public class StyleWindow : Window
    {
        public static readonly RoutedUICommand MinimizeCommand = new RoutedUICommand("Minimize", nameof(MinimizeCommand), typeof(StyleWindow));

        // Allows windows to opt-out of header buttons when placing controls in the body instead
        public static readonly DependencyProperty ShowHeaderButtonsProperty = DependencyProperty.Register(
            nameof(ShowHeaderButtons), typeof(bool), typeof(StyleWindow), new PropertyMetadata(true));

        public bool ShowHeaderButtons
        {
            get => (bool)GetValue(ShowHeaderButtonsProperty);
            set => SetValue(ShowHeaderButtonsProperty, value);
        }

        public StyleWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = false; // keep OS rendering for perf; we draw our own header
            ResizeMode = ResizeMode.CanResizeWithGrip;
            Background = Brushes.Transparent; // outer background; template provides actual visuals

            // Apply WindowChrome for resizable border without default title bar
            WindowChrome.SetWindowChrome(this, new WindowChrome
            {
                CaptionHeight = 0, // pure client area; we'll handle dragging manually
                ResizeBorderThickness = new Thickness(6),
                GlassFrameThickness = new Thickness(0),
                UseAeroCaptionButtons = false
            });

            // Apply our base style template (from themes)
            Loaded += (_, __) => ApplyTemplateStyle();

            // Routed command bindings
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (_, __) => Close()));
            CommandBindings.Add(new CommandBinding(MinimizeCommand, (_, __) => WindowState = WindowState.Minimized));

            // Theme change re-applies template to reflect dynamic resources
            ThemeService.Instance.ThemeChanged += (_, __) => ApplyTemplateStyle();
        }

        private void ApplyTemplateStyle()
        {
            try
            {
                var style = TryFindResource("StyleWindow.BaseStyle") as Style;
                if (style != null) Style = style;
            }
            catch { /* ignore */ }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Hook header drag behavior
            if (GetTemplateChild("PART_Header") is System.Windows.FrameworkElement header)
            {
                header.MouseLeftButtonDown -= Header_MouseLeftButtonDown;
                header.MouseLeftButtonDown += Header_MouseLeftButtonDown;
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Toggle maximize on double-click
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                return;
            }

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }
    }
}
