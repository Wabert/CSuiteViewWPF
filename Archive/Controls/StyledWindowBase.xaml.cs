using System.Windows;
using System.Windows.Controls;

namespace CSuiteViewWPF.Controls
{
    /// <summary>
    /// Archived legacy control; prefer Views.StyleWindow
    /// </summary>
    public partial class StyledWindowBase : UserControl
    {
        public StyledWindowBase()
        {
            InitializeComponent();
        }

        // Event handlers removed in archive version to avoid behavioral drift
    }
}
