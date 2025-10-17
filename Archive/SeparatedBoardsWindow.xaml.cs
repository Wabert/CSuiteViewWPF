using System.Windows;

namespace CSuiteViewWPF
{
    public partial class SeparatedBoardsWindow : MahApps.Metro.Controls.MetroWindow
    {
        public SeparatedBoardsWindow()
        {
            InitializeComponent();
        }

        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Begin dragging the window when the header/footer is clicked and dragged
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
