using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CSuiteViewWPF.Models;

namespace CSuiteViewWPF.ViewModels
{
    /// <summary>
    /// ViewModel for StyledContentWindow - manages window state and data
    /// </summary>
    public class StyledContentWindowViewModel : INotifyPropertyChanged
    {
        private string _headerTitle = "ANICO DATABASE MANAGER";
        private int _panelCount = 3;
        private int _treePanelIndex = -1;
        private int _headerHeight = 48;
        private int _footerHeight = 36;
        private int _spaceAbove = 12;
        private int _spaceBelow = 18;
        private bool _footerVisible = true;
        private ObservableCollection<FileSystemItem> _items = new ObservableCollection<FileSystemItem>();

        #region Properties

        /// <summary>
        /// Title displayed in the window header
        /// </summary>
        public string HeaderTitle
        {
            get => _headerTitle;
            set
            {
                if (_headerTitle != value)
                {
                    _headerTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of panels to display (0-3)
        /// </summary>
        public int PanelCount
        {
            get => _panelCount;
            set
            {
                if (_panelCount != value)
                {
                    _panelCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Index of the tree panel (-1 for none, 0 for left, 2 for right)
        /// </summary>
        public int TreePanelIndex
        {
            get => _treePanelIndex;
            set
            {
                if (_treePanelIndex != value)
                {
                    _treePanelIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Height of the header bar in pixels
        /// </summary>
        public int HeaderHeight
        {
            get => _headerHeight;
            set
            {
                if (_headerHeight != value)
                {
                    _headerHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Height of the footer bar in pixels
        /// </summary>
        public int FooterHeight
        {
            get => _footerHeight;
            set
            {
                if (_footerHeight != value)
                {
                    _footerHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Space above panels in pixels
        /// </summary>
        public int SpaceAbove
        {
            get => _spaceAbove;
            set
            {
                if (_spaceAbove != value)
                {
                    _spaceAbove = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Space below panels in pixels
        /// </summary>
        public int SpaceBelow
        {
            get => _spaceBelow;
            set
            {
                if (_spaceBelow != value)
                {
                    _spaceBelow = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the footer bar is visible
        /// </summary>
        public bool FooterVisible
        {
            get => _footerVisible;
            set
            {
                if (_footerVisible != value)
                {
                    _footerVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Collection of items to display in the FilteredDataGrid
        /// </summary>
        public ObservableCollection<FileSystemItem> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged();
                }
            }
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
