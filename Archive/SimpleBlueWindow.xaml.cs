using System.Windows;
using System.Windows.Input;

namespace CSuiteViewWPF
{
    public partial class SimpleBlueWindow : Window
    {
        public SimpleBlueWindow()
        {
            InitializeComponent();
            // Allow dragging by mouse down anywhere on the window
            this.MouseLeftButtonDown += SimpleBlueWindow_MouseLeftButtonDown;
        }

        private void SimpleBlueWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
