using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSuiteViewWPF.Controls
{
    public partial class FilterPopupWindow : Window
    {
        public FilterPopupWindow()
        {
            InitializeComponent();
            
            // Handle clicking outside the window to close it
            this.Deactivated += FilterPopupWindow_Deactivated;
        }

        private void FilterPopupWindow_Deactivated(object? sender, EventArgs e)
        {
            try
            {
                // Close the window when it loses focus (user clicked outside)
                // Use Dispatcher to ensure we're on the UI thread
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (this.IsLoaded && !this.IsMouseOver)
                    {
                        this.Close();
                    }
                }));
            }
            catch
            {
                // Ignore any errors during close
            }
        }

        /// <summary>
        /// Sets the filter content to display in the window
        /// </summary>
        public void SetFilterContent(UIElement content)
        {
            if (ContentContainer != null)
            {
                ContentContainer.Child = content;
            }
        }

        /// <summary>
        /// Sets the title text in the custom title bar
        /// </summary>
        public void SetTitle(string title)
        {
            if (TitleText != null)
            {
                TitleText.Text = title;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
