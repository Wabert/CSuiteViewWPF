using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CSuiteViewWPF.Services;
using CSuiteViewWPF.Views;

namespace CSuiteViewWPF.ViewModels
{
    /// <summary>
    /// ViewModel for the main startup window with three action buttons
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            // Initialize commands
            DirectoryScanCommand = new RelayCommand(ExecuteDirectoryScan);
        }

        #region Commands

        public ICommand DirectoryScanCommand { get; }

        #endregion

        #region Command Implementations

        private void ExecuteDirectoryScan()
        {
            // Use the new base window with chrome and theming
            var window = new ThemedWindow
            {
                Title = "File System Scanner",
                Width = 1000,
                Height = 600,
                Content = new CSuiteViewWPF.Views.DirectoryScanView()
            };

            var owner = Application.Current?.MainWindow as Window;
            if (owner != null)
            {
                window.Owner = owner;
                // Offset from owner so we don't cover the top-left controls
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = owner.Left + 40;
                window.Top = owner.Top + 60;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            window.Show();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
