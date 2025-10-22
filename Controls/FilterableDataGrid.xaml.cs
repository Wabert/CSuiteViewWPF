using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CSuiteViewWPF.Models;
using CSuiteViewWPF.ViewModels;
using CSuiteViewWPF.Windows;

namespace CSuiteViewWPF.Controls
{
    /// <summary>
    /// Generic filterable DataGrid control that dynamically generates columns based on ViewModel configuration.
    /// This control is now fully reusable with any data type that implements IFilterableDataGridViewModel.
    /// </summary>
    public partial class FilterableDataGrid : UserControl
    {
        public IFilterableDataGridViewModel ViewModel { get; private set; }
        private DataGridCell? _rightClickedCell;
    private ColumnFilterWindow? _currentFilterWindow = null;
        private ToggleButton? _currentToggleButton = null;

        public FilterableDataGrid()
        {
            InitializeComponent();
            // Use the high-performance ViewModel with instant filtering
            ViewModel = new FiteredDataGridViewModel();
            DataContext = ViewModel;
            
            // OLD CODE - Commented out (FilteredDataGridViewModel moved to Archive)
            // if (ViewModel is FilteredDataGridViewModel oldViewModel)
            // {
            //     oldViewModel.AttachFilterChangeHandlers();
            // }
            
            this.Loaded += FilteredDataGridControl_Loaded;
            this.DataContextChanged += FilteredDataGridControl_DataContextChanged;
        }

        private void FilteredDataGridControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If the DataContext is changed to a different IFilterableDataGridViewModel, regenerate columns
            if (e.NewValue is IFilterableDataGridViewModel viewModel)
            {
                GenerateColumns(viewModel);
            }
        }

        private void FilteredDataGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Generate columns based on ViewModel configuration
            if (DataContext is IFilterableDataGridViewModel viewModel)
            {
                GenerateColumns(viewModel);
            }
            
            // Delay attaching handlers until after the visual tree is fully built
            Dispatcher.BeginInvoke(new Action(() =>
            {
                AttachHeaderToggleHandlers();
                AttachHeaderClickHandlers();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Dynamically generates DataGrid columns based on ColumnDefinitions from the ViewModel.
        /// This replaces all the hard-coded column markup in XAML.
        /// </summary>
        private void GenerateColumns(IFilterableDataGridViewModel viewModel)
        {
            MainDataGrid.Columns.Clear();

            foreach (var colDef in viewModel.ColumnDefinitions)
            {
                var column = new DataGridTextColumn
                {
                    Header = colDef.Header,
                    Binding = new Binding(colDef.BindingPath)
                    {
                        StringFormat = colDef.StringFormat
                    },
                    Width = new DataGridLength(colDef.Width.Value, 
                        colDef.Width.GridUnitType == GridUnitType.Star ? DataGridLengthUnitType.Star : 
                        colDef.Width.GridUnitType == GridUnitType.Auto ? DataGridLengthUnitType.Auto : 
                        DataGridLengthUnitType.Pixel)
                };

                // Add filter header template if column is filterable
                if (colDef.IsFilterable)
                {
                    column.HeaderTemplate = CreateSimpleFilterHeaderTemplate(colDef);
                    column.CanUserSort = false; // Disable sorting on filterable columns
                }

                MainDataGrid.Columns.Add(column);
            }
        }

        /// <summary>
        /// Creates a clickable DataTemplate for column headers (no popup - handled by ShowSimpleFilterPopup)
        /// </summary>
    private DataTemplate CreateSimpleFilterHeaderTemplate(FilteredColumnDefinition colDef)
        {
            var template = new DataTemplate();

            // Create the root Grid
            var gridFactory = new FrameworkElementFactory(typeof(Grid));

            // Create header TextBlock
            var textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetValue(TextBlock.TextProperty, colDef.Header);
            textBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            textBlock.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlock.SetValue(TextBlock.ForegroundProperty, FindResource("DarkBlue"));
            textBlock.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            textBlock.SetValue(TextBlock.NameProperty, $"{colDef.ColumnKey}HeaderText");
            textBlock.SetValue(UIElement.IsHitTestVisibleProperty, false);
            gridFactory.AppendChild(textBlock);

            // Create invisible ToggleButton that covers the entire header
            var toggleButton = new FrameworkElementFactory(typeof(ToggleButton));
            toggleButton.SetValue(FrameworkElement.NameProperty, $"{colDef.ColumnKey}Toggle");
            toggleButton.SetValue(Control.BackgroundProperty, Brushes.Transparent);
            toggleButton.SetValue(Control.BorderThicknessProperty, new Thickness(0));
            toggleButton.SetValue(ContentControl.ContentProperty, "");
            toggleButton.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            toggleButton.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            toggleButton.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 8, 0));
            toggleButton.SetValue(UIElement.OpacityProperty, 0.0); // Transparent
            toggleButton.SetValue(UIElement.FocusableProperty, false);
            toggleButton.SetValue(ToolTipService.ToolTipProperty, $"Click to filter {colDef.Header}");
            toggleButton.SetValue(Control.CursorProperty, System.Windows.Input.Cursors.Hand);
            toggleButton.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(HeaderToggle_Checked));
            gridFactory.AppendChild(toggleButton);

            template.VisualTree = gridFactory;
            return template;
        }

        /// <summary>
        /// OLD CODE - Creates a DataTemplate for a column header with filter popup.
        /// COMMENTED OUT - Uses archived FilterContent/FilterContentViewModel
        /// Now using ShowSimpleFilterPopup instead
        /// </summary>
        /* ARCHIVED CODE - REFERENCES MOVED CLASSES
        private DataTemplate CreateFilterHeaderTemplate(FilterableColumnDefinition colDef, IFilterableDataGridViewModel viewModel)
        {
            var template = new DataTemplate();

            // Create the root Grid
            var gridFactory = new FrameworkElementFactory(typeof(Grid));

            // Add column definitions
            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));
            gridFactory.AppendChild(col1);
            gridFactory.AppendChild(col2);

            // Create header TextBlock
            var textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetValue(TextBlock.TextProperty, colDef.Header);
            textBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            textBlock.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlock.SetValue(TextBlock.ForegroundProperty, FindResource("DarkBlue"));
            textBlock.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            textBlock.SetValue(TextBlock.NameProperty, $"{colDef.ColumnKey}HeaderText"); // Add name for later access
            textBlock.SetValue(Grid.ColumnSpanProperty, 2);
            textBlock.SetValue(UIElement.IsHitTestVisibleProperty, false); // Make text not block clicks
            gridFactory.AppendChild(textBlock);

            // Create invisible ToggleButton that covers the entire header (clickable area)
            var toggleButton = new FrameworkElementFactory(typeof(ToggleButton));
            toggleButton.SetValue(FrameworkElement.NameProperty, $"{colDef.ColumnKey}Toggle");
            toggleButton.SetValue(Grid.ColumnSpanProperty, 2);
            toggleButton.SetValue(Control.BackgroundProperty, Brushes.Transparent);
            toggleButton.SetValue(Control.BorderThicknessProperty, new Thickness(0));
            toggleButton.SetValue(ContentControl.ContentProperty, "");
            toggleButton.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            toggleButton.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            toggleButton.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 8, 0)); // Leave room for resize gripper on both left and right
            toggleButton.SetValue(UIElement.OpacityProperty, 0.0); // Completely transparent
            toggleButton.SetValue(UIElement.FocusableProperty, false);
            toggleButton.SetValue(ToolTipService.ToolTipProperty, $"Click to filter {colDef.Header}");
            toggleButton.SetValue(Control.CursorProperty, System.Windows.Input.Cursors.Hand);
            toggleButton.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(HeaderToggle_Checked));
            gridFactory.AppendChild(toggleButton);

            // Create Popup
            var popup = new FrameworkElementFactory(typeof(Popup));
            popup.SetBinding(Popup.IsOpenProperty, new Binding("IsChecked")
            {
                ElementName = $"{colDef.ColumnKey}Toggle"
            });
            popup.SetValue(Popup.PlacementTargetProperty, new Binding
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridColumnHeader), 1)
            });
            popup.SetValue(Popup.PlacementProperty, PlacementMode.Custom);
            popup.SetValue(Popup.CustomPopupPlacementCallbackProperty, new CustomPopupPlacementCallback(OnFilterPopupPlacement));
            popup.SetValue(Popup.StaysOpenProperty, false);
            popup.SetValue(Popup.AllowsTransparencyProperty, true);
            popup.SetValue(Popup.PopupAnimationProperty, PopupAnimation.Fade);

            // Create Popup content - Border
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(230, 242, 255))); // Light blue
            border.SetValue(Border.BorderBrushProperty, Brushes.Gold);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(2));
            border.SetValue(Border.PaddingProperty, new Thickness(8));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            border.SetValue(FrameworkElement.MinWidthProperty, 200.0);
            border.SetValue(FrameworkElement.MinHeightProperty, 250.0);

            // Create Grid inside Border
            var popupGrid = new FrameworkElementFactory(typeof(Grid));
            var row1 = new FrameworkElementFactory(typeof(RowDefinition));
            row1.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Auto));
            var row2 = new FrameworkElementFactory(typeof(RowDefinition));
            row2.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
            popupGrid.AppendChild(row1);
            popupGrid.AppendChild(row2);

            // Create close button
            var closeButton = new FrameworkElementFactory(typeof(Button));
            closeButton.SetValue(ContentControl.ContentProperty, "✕");
            closeButton.SetValue(FrameworkElement.WidthProperty, 22.0);
            closeButton.SetValue(FrameworkElement.HeightProperty, 22.0);
            closeButton.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            closeButton.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top);
            closeButton.SetValue(FrameworkElement.MarginProperty, new Thickness(0));
            closeButton.SetValue(Control.PaddingProperty, new Thickness(0));
            closeButton.SetValue(Control.BackgroundProperty, Brushes.Transparent);
            closeButton.SetValue(Control.BorderThicknessProperty, new Thickness(0));
            closeButton.SetValue(Control.ForegroundProperty, FindResource("DarkBlue"));
            closeButton.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnFilterPopupCloseClick));
            popupGrid.AppendChild(closeButton);

            // Create FilterContent UserControl
            var filterContent = new FrameworkElementFactory(typeof(FilterContent));
            filterContent.SetValue(Grid.RowProperty, 1);
            
            // Create a ViewModel for the FilterContent
            var filterItems = viewModel.GetFiltersForColumn(colDef.ColumnKey);
            System.Diagnostics.Debug.WriteLine($"Creating filter for column '{colDef.ColumnKey}' with {filterItems.Count} items");
            
            var filterContentVM = new FilterContentViewModel
            {
                FilterTitle = $"Filter {colDef.Header}",
                FilterItems = filterItems,
                SelectAllCommand = viewModel.GetSelectAllCommand(colDef.ColumnKey),
                DeselectAllCommand = viewModel.GetDeselectAllCommand(colDef.ColumnKey)
            };

            if (viewModel is FilteredDataGridViewModel concreteViewModel)
            {
                filterContentVM.VisibleNormalizedValues = concreteViewModel.GetVisibleNormalizedValues(colDef.ColumnKey);
            }
            filterContent.SetValue(FrameworkElement.DataContextProperty, filterContentVM);
            
            popupGrid.AppendChild(filterContent);
            border.AppendChild(popupGrid);
            popup.AppendChild(border);
            gridFactory.AppendChild(popup);

            template.VisualTree = gridFactory;
            return template;
        }
        */ // END ARCHIVED CODE

        private void AttachHeaderToggleHandlers()
        {
            // Find all ToggleButtons in the DataGrid headers and wire Checked event
            var toggles = FindVisualChildren<ToggleButton>(this);
            foreach (var toggle in toggles)
            {
                if (toggle.Name.EndsWith("Toggle"))
                {
                    toggle.Checked -= HeaderToggle_Checked;
                    toggle.Checked += HeaderToggle_Checked;
                    toggle.Unchecked -= HeaderToggle_Unchecked;
                    toggle.Unchecked += HeaderToggle_Unchecked;
                }
            }
        }

        private void HeaderToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            // This handler exists to prevent the Checked event from being called when toggling
            // Just do nothing when unchecked
        }

        private void AttachHeaderClickHandlers()
        {
            // Find all DataGridColumnHeaders and attach PreviewMouseDown handler
            var headers = FindVisualChildren<DataGridColumnHeader>(this);
            foreach (var header in headers)
            {
                header.PreviewMouseDown -= Header_PreviewMouseDown;
                header.PreviewMouseDown += Header_PreviewMouseDown;
            }
        }

        private void Header_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridColumnHeader header)
            {
                // Check if we're clicking on the resize gripper (right OR left edge of header)
                var clickPoint = e.GetPosition(header);
                var gripperWidth = 8; // Width of the resize gripper area (increased for easier resizing)
                
                // If clicking in the gripper area (right edge OR left edge), don't trigger filter
                if (clickPoint.X >= header.ActualWidth - gripperWidth || clickPoint.X <= gripperWidth)
                {
                    return; // Let the resize operation happen
                }
                
                // Find the ToggleButton in this header
                var toggle = FindVisualChildren<ToggleButton>(header).FirstOrDefault(t => t.Name.EndsWith("Toggle"));
                if (toggle != null)
                {
                    // Check if this toggle has the current open window
                    if (_currentToggleButton == toggle && _currentFilterWindow != null)
                    {
                        // Clicking the same header that has window open - close it
                        CloseFilterPopup();
                    }
                    else
                    {
                        // Open window for this toggle (or switch to this one)
                        // Set to checked to trigger the Checked event
                        toggle.IsChecked = true;
                    }
                    
                    e.Handled = true; // Prevent other handlers from firing
                }
            }
        }

        private void HeaderToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton checkedToggle)
            {
                // Check if this is already being handled (to prevent double popup)
                if (e.Handled)
                    return;
                    
                e.Handled = true;
                
                // Extract column key from toggle button name (e.g., "ObjectTypeToggle" -> "ObjectType")
                var columnKey = checkedToggle.Name.Replace("Toggle", "");
                
                // Find the column definition
                var viewModel = DataContext as IFilterableDataGridViewModel;
                if (viewModel != null)
                {
                    var columnDef = viewModel.ColumnDefinitions.FirstOrDefault(c => c.ColumnKey == columnKey);
                    if (columnDef != null)
                    {
                        // === NEW SIMPLIFIED FILTERING ===
                        ShowSimpleFilterPopup(columnKey, columnDef.Header.ToString(), checkedToggle);
                    }
                }
            }
        }

        /// <summary>
        /// Shows the new simplified filter popup using a lightweight Window instead of Popup.
        /// This solves all the mouse capture and StaysOpen issues inherent to WPF Popup.
        /// </summary>
        private void ShowSimpleFilterPopup(string columnKey, string columnName, ToggleButton toggleButton)
        {
            var viewModel = DataContext as IFilterableDataGridViewModel;
            if (viewModel == null) return;

            // If clicking the same toggle that's already open, close it
            if (_currentFilterWindow != null && _currentToggleButton == toggleButton)
            {
                CloseFilterPopup();
                return;
            }

            // Close any existing window and uncheck its toggle
            CloseFilterPopup();

            // Store reference to current toggle
            _currentToggleButton = toggleButton;

            // Create the filter content (FilterColumnPanel UserControl)
            var filterContent = new FilterColumnPanel
            {
                ColumnName = columnName,
                ColumnKey = columnKey
            };

            // Load distinct values
            var distinctValues = viewModel.GetDistinctValuesForColumn(columnKey);
            filterContent.LoadValues(distinctValues);

            // Set currently selected values (if any)
            var activeValues = viewModel.GetActiveFilterValues(columnKey);
            filterContent.SetSelectedValues(activeValues);

            // Handle filter changes (instant filtering)
            filterContent.FilterChanged += (s, e) =>
            {
                viewModel.ApplyColumnFilter(e.ColumnKey, e.SelectedValues);
                UpdateFilterIndicators();
            };

            // Handle close button
            filterContent.CloseRequested += (s, e) =>
            {
                CloseFilterPopup();
            };

            // Create the filter window
            _currentFilterWindow = new ColumnFilterWindow
            {
                Owner = Window.GetWindow(this) // Important: keeps window on top of parent
            };
            
            // Set the title and content using the existing methods
            _currentFilterWindow.SetTitle($"Filter: {columnName}");
            _currentFilterWindow.SetFilterContent(filterContent);

            // Position the window below the toggle button
            var point = toggleButton.PointToScreen(new Point(0, toggleButton.ActualHeight));
            _currentFilterWindow.Left = point.X;
            _currentFilterWindow.Top = point.Y;

            // When window closes (for any reason), clean up
            _currentFilterWindow.Closed += (s, e) =>
            {
                if (_currentToggleButton != null)
                {
                    _currentToggleButton.IsChecked = false;
                    _currentToggleButton = null;
                }
                _currentFilterWindow = null;
            };

            // Show the window (non-modal)
            _currentFilterWindow.Show();
        }

        /// <summary>
        /// Closes the current filter window if one is open
        /// </summary>
        private void CloseFilterPopup()
        {
            if (_currentFilterWindow != null)
            {
                _currentFilterWindow.Close();
                _currentFilterWindow = null;
            }
            
            if (_currentToggleButton != null)
            {
                _currentToggleButton.IsChecked = false;
                _currentToggleButton = null;
            }
        }

        // === OLD COMPLEX FILTERING CODE - ARCHIVED (references FilterContent/FilterContentViewModel) ===
        /* ARCHIVED CODE
        private void HeaderToggle_Checked_OLD_UNUSED(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton checkedToggle)
            {
                var columnKey = checkedToggle.Name.Replace("Toggle", "");
                var viewModel = DataContext as IFilterableDataGridViewModel;
                if (viewModel != null)
                {
                    var columnDef = viewModel.ColumnDefinitions.FirstOrDefault(c => c.ColumnKey == columnKey);
                    if (columnDef != null)
                    {
                        // Get filter items for this column
                        var filterItems = viewModel.GetFiltersForColumn(columnKey);
                        
                        // Create FilterContentViewModel with OK and Cancel commands
                        var filterContentVM = new FilterContentViewModel
                        {
                            FilterTitle = $"Filter {columnDef.Header}",
                            FilterItems = filterItems,
                            SelectAllCommand = viewModel.GetSelectAllCommand(columnKey),
                            DeselectAllCommand = viewModel.GetDeselectAllCommand(columnKey)
                        };

                        if (viewModel is FilteredDataGridViewModel concreteViewModel)
                        {
                            filterContentVM.VisibleNormalizedValues = concreteViewModel.GetVisibleNormalizedValues(columnKey);
                        }
                        
                        // Create FilterContent control
                        var filterContent = new FilterContent
                        {
                            DataContext = filterContentVM
                        };
                        
                        // Create and show the filter popup window
                        var filterWindow = new FilterPopupWindow
                        {
                            Owner = Window.GetWindow(this),
                            WindowStartupLocation = WindowStartupLocation.Manual
                        };
                        
                        // Set the window title
                        filterWindow.SetTitle($"Filter: {columnDef.Header}");
                        
                        // Set up OK command - apply filters and close window
                        filterContentVM.OkCommand = new RelayCommand(() =>
                        {
                            // Apply the filter by triggering the filter refresh
                            ((FilteredDataGridViewModel)viewModel).ApplyFilters();
                            // Update filter indicators (underline headers with active filters)
                            UpdateFilterIndicators();
                            filterWindow.Close();
                        });
                        
                        // Set up Cancel command - restore original selections and close window
                        filterContentVM.CancelCommand = new RelayCommand(() =>
                        {
                            // Restore original selections without applying
                            filterContentVM.RestoreOriginalSelections();
                            filterWindow.Close();
                        });
                        
                        // Set up Clear Filter command - select all items, apply immediately, and close window
                        filterContentVM.ClearFilterCommand = new RelayCommand(() =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Clear Filter clicked for column: {columnKey}");
                            
                            // Clear the filter for this column (rebuilds from full dataset with all selected)
                            ((FilteredDataGridViewModel)viewModel).ClearColumnFilter(columnKey);
                            
                            System.Diagnostics.Debug.WriteLine($"Filter collection count after clear: {filterItems.Count}");
                            System.Diagnostics.Debug.WriteLine($"All selected: {filterItems.All(f => f.IsSelected)}");
                            
                            // Apply the filter immediately to refresh the view
                            ((FilteredDataGridViewModel)viewModel).ApplyFilters();
                            // Update filter indicators (remove underline since filter is cleared)
                            UpdateFilterIndicators();
                            // Close the window
                            filterWindow.Close();
                            
                            // Uncheck the toggle button
                            if (checkedToggle != null)
                            {
                                checkedToggle.IsChecked = false;
                            }
                        });
                        
                        // Set up Sort Ascending command - sort the column and close window
                        filterContentVM.SortAscendingCommand = new RelayCommand(() =>
                        {
                            SortColumn(columnDef.ColumnKey, System.ComponentModel.ListSortDirection.Ascending);
                            filterWindow.Close();
                        });
                        
                        // Set up Sort Descending command - sort the column and close window
                        filterContentVM.SortDescendingCommand = new RelayCommand(() =>
                        {
                            SortColumn(columnDef.ColumnKey, System.ComponentModel.ListSortDirection.Descending);
                            filterWindow.Close();
                        });
                        
                        // Set the filter content
                        filterWindow.SetFilterContent(filterContent);
                        
                        // Calculate position: align top of popup with bottom of header, right edge with right edge of header
                        var header = FindParentOfType<DataGridColumnHeader>(checkedToggle);
                        if (header != null)
                        {
                            // Get DPI scaling factor
                            var source = PresentationSource.FromVisual(header);
                            double dpiX = 1.0;
                            double dpiY = 1.0;
                            if (source != null)
                            {
                                dpiX = source.CompositionTarget.TransformToDevice.M11;
                                dpiY = source.CompositionTarget.TransformToDevice.M22;
                            }
                            
                            // Get the position of the header relative to the screen
                            var headerPoint = header.PointToScreen(new Point(0, 0));
                            
                            // Convert from physical pixels to DIPs
                            var left = headerPoint.X / dpiX;
                            var top = (headerPoint.Y + header.ActualHeight + 3) / dpiY; // Add 3 pixels gap below header
                            
                            // Debug output
                            System.Diagnostics.Debug.WriteLine($"DPI Scale: X={dpiX}, Y={dpiY}");
                            System.Diagnostics.Debug.WriteLine($"Header screen position (physical): X={headerPoint.X}, Y={headerPoint.Y}");
                            System.Diagnostics.Debug.WriteLine($"Header ActualWidth={header.ActualWidth}, ActualHeight={header.ActualHeight}");
                            System.Diagnostics.Debug.WriteLine($"Setting popup position (DIPs): Left={left}, Top={top}");
                            
                            // Position popup: align left edge with column left edge, just below the header
                            filterWindow.Left = left;
                            filterWindow.Top = top;
                        }
                        
                        // Uncheck the toggle when window closes
                        filterWindow.Closed += (s, args) =>
                        {
                            checkedToggle.IsChecked = false;
                        };
                        
                        // Uncheck all other header toggles (only one popup open at a time)
                        var toggles = FindVisualChildren<ToggleButton>(this);
                        foreach (var toggle in toggles)
                        {
                            if (toggle != checkedToggle && toggle.Name.EndsWith("Toggle"))
                            {
                                toggle.IsChecked = false;
                            }
                        }
                        
                        filterWindow.Show(); // Use Show() instead of ShowDialog() to allow Deactivated event to work
                    }
                }
            }
        }
        */ // END ARCHIVED CODE

        /// <summary>
        /// Helper method to find parent element of a specific type
        /// </summary>
        private static T? FindParentOfType<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject? child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }
                    if (child != null)
                    {
                        foreach (T childOfChild in FindVisualChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a visual element is inside another element's visual tree
        /// </summary>
        private static bool IsElementInsidePopup(DependencyObject element, DependencyObject container)
        {
            var current = element;
            while (current != null)
            {
                if (current == container)
                    return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        /// <summary>
        /// Handler for toggle button clicks (for debugging)
        /// </summary>
        private void OnToggleButtonClick(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            System.Diagnostics.Debug.WriteLine($"Toggle button clicked: {toggle?.Name}, IsChecked: {toggle?.IsChecked}");
            System.Windows.MessageBox.Show($"Toggle button clicked: {toggle?.Name}\nIsChecked: {toggle?.IsChecked}");
        }

        /// <summary>
        /// Handler for the X button in filter popups
        /// </summary>
        public void OnFilterPopupCloseClick(object sender, RoutedEventArgs e)
        {
            // Find the parent DataGridColumnHeader, then uncheck the ToggleButton in that header
            var btn = sender as Button;
            if (btn == null) return;
            
            DependencyObject? parent = btn;
            while (parent != null && !(parent is DataGridColumnHeader))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            
            if (parent is DataGridColumnHeader header)
            {
                // Find the ToggleButton in the header
                var toggle = FindVisualChildren<ToggleButton>(header).FirstOrDefault();
                if (toggle != null)
                {
                    toggle.IsChecked = false;
                }
            }
        }

        /// <summary>
        /// CustomPopupPlacementCallback for filter popups: align right edge of popup to right edge of header
        /// </summary>
        private CustomPopupPlacement[] OnFilterPopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            // Place the popup so its right edge aligns with the header's right edge, a few pixels below
            var point = new Point(targetSize.Width - popupSize.Width, targetSize.Height + 4);
            return new[] { new CustomPopupPlacement(point, PopupPrimaryAxis.None) };
        }

        /// <summary>
        /// Sort the DataGrid by a specific column using the high-performance sorting engine
        /// ARCHIVED - References FilteredDataGridViewModel
        /// </summary>
        /* ARCHIVED CODE
        private void SortColumn(string columnKey, System.ComponentModel.ListSortDirection direction)
        {
            if (DataContext is not FilteredDataGridViewModel viewModel) return;

            // Use the high-performance sorting method from the ViewModel
            bool ascending = direction == System.ComponentModel.ListSortDirection.Ascending;
            viewModel.SortByColumn(columnKey, ascending);
        }
        */ // END ARCHIVED CODE

        /// <summary>
        /// Update the visual indicator (underline) for columns with active filters
        /// </summary>
        public void UpdateFilterIndicators()
        {
            if (DataContext is not IFilterableDataGridViewModel viewModel) return;

            // Go through all column headers and update their text decoration
            var headers = FindVisualChildren<DataGridColumnHeader>(this);
            foreach (var header in headers)
            {
                // Find the TextBlock in the header
                var textBlocks = FindVisualChildren<TextBlock>(header);
                foreach (var textBlock in textBlocks)
                {
                    // Check if this TextBlock is a header text (has the HeaderText suffix in name)
                    if (textBlock.Name != null && textBlock.Name.EndsWith("HeaderText"))
                    {
                        // Extract column key from the name
                        string columnKey = textBlock.Name.Replace("HeaderText", "");
                        
                        // Check if this column has any active filters
                        var activeValues = viewModel.GetActiveFilterValues(columnKey);
                        bool hasActiveFilter = activeValues.Count > 0;
                        
                        // Apply underline if filter is active
                        if (hasActiveFilter)
                        {
                            textBlock.TextDecorations = TextDecorations.Underline;
                        }
                        else
                        {
                            textBlock.TextDecorations = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handler for when the context menu is opened - captures the right-clicked cell
        /// </summary>
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ContextMenu_Opened called");
            
            if (sender is ContextMenu contextMenu && contextMenu.PlacementTarget is DataGrid dataGrid)
            {
                // Get the mouse position relative to the DataGrid
                var mousePos = System.Windows.Input.Mouse.GetPosition(dataGrid);
                System.Diagnostics.Debug.WriteLine($"Mouse position when context menu opened: {mousePos}");
                
                // Hit test to find what's under the mouse
                var hitTestResult = VisualTreeHelper.HitTest(dataGrid, mousePos);
                if (hitTestResult != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Hit test result type: {hitTestResult.VisualHit?.GetType().Name}");
                    
                    // Find the DataGridCell that was clicked
                    if (hitTestResult.VisualHit is DependencyObject visualHit)
                    {
                        _rightClickedCell = FindParentOfType<DataGridCell>(visualHit);
                    }
                    else
                    {
                        _rightClickedCell = null;
                    }
                    
                    if (_rightClickedCell != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Cell captured successfully");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No cell found at mouse position");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Hit test returned null");
                    _rightClickedCell = null;
                }
            }
        }

        /// <summary>
        /// Handler for "Copy Cell" context menu item
        /// </summary>
        private void CopyCell_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CopyCell_Click called");
            
            if (_rightClickedCell != null)
            {
                System.Diagnostics.Debug.WriteLine($"Using stored cell");
                var cellContent = GetCellContent(_rightClickedCell);
                System.Diagnostics.Debug.WriteLine($"Cell content: '{cellContent}'");
                
                if (!string.IsNullOrEmpty(cellContent))
                {
                    Clipboard.SetText(cellContent);
                    System.Diagnostics.Debug.WriteLine("Content copied to clipboard");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Cell content is empty");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No cell was stored");
            }
        }

        /// <summary>
        /// Handler for "Copy Table" context menu item
        /// Copies the currently visible (filtered) data to clipboard in tab-delimited format
        /// </summary>
        private void CopyTable_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not IFilterableDataGridViewModel viewModel) return;
            if (viewModel.Items == null || viewModel.Items.Count == 0) return;

            var sb = new StringBuilder();

            // Add header row
            var headerValues = new List<string>();
            foreach (var column in MainDataGrid.Columns)
            {
                var header = column.Header?.ToString() ?? "";
                headerValues.Add(header);
            }
            sb.AppendLine(string.Join("\t", headerValues));

            // Add data rows - use the DataGrid's ItemsSource which contains only visible items
            var visibleItems = MainDataGrid.ItemsSource;
            if (visibleItems != null)
            {
                foreach (var item in visibleItems)
                {
                    var rowValues = new List<string>();
                    foreach (var column in MainDataGrid.Columns)
                    {
                        var cellValue = GetCellValue(item, column);
                        rowValues.Add(cellValue);
                    }
                    sb.AppendLine(string.Join("\t", rowValues));
                }
            }

            Clipboard.SetText(sb.ToString());
            System.Diagnostics.Debug.WriteLine($"Copied {sb.Length} characters to clipboard");
        }

        /// <summary>
        /// Get the DataGridCell that was right-clicked
        /// </summary>
        private DataGridCell? GetDataGridCellFromContextMenu(object sender)
        {
            if (sender is MenuItem menuItem)
            {
                var contextMenu = menuItem.Parent as ContextMenu;
                if (contextMenu?.PlacementTarget is DataGrid dataGrid)
                {
                    // Get the cell that was right-clicked
                    var cellInfo = dataGrid.CurrentCell;
                    if (cellInfo.Column != null && cellInfo.Item != null)
                    {
                        // Find the DataGridCell for the current cell
                        var row = dataGrid.ItemContainerGenerator.ContainerFromItem(cellInfo.Item) as DataGridRow;
                        if (row != null)
                        {
                            var cellContent = cellInfo.Column.GetCellContent(row);
                            if (cellContent != null)
                            {
                                return FindParentOfType<DataGridCell>(cellContent);
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get the text content of a DataGridCell
        /// </summary>
        private string GetCellContent(DataGridCell cell)
        {
            if (cell.Content is TextBlock textBlock)
            {
                return textBlock.Text;
            }
            else if (cell.Content is ContentPresenter presenter)
            {
                var textBlock2 = FindVisualChildren<TextBlock>(presenter).FirstOrDefault();
                if (textBlock2 != null)
                {
                    return textBlock2.Text;
                }
            }
            return cell.Content?.ToString() ?? "";
        }

        /// <summary>
        /// Get the value of a cell for a given item and column
        /// </summary>
        private string GetCellValue(object item, DataGridColumn column)
        {
            if (column is DataGridBoundColumn boundColumn)
            {
                var binding = boundColumn.Binding as Binding;
                if (binding != null && !string.IsNullOrEmpty(binding.Path.Path))
                {
                    var propertyInfo = item.GetType().GetProperty(binding.Path.Path);
                    if (propertyInfo != null)
                    {
                        var value = propertyInfo.GetValue(item);
                        
                        // Apply StringFormat if specified
                        if (value != null && !string.IsNullOrEmpty(binding.StringFormat))
                        {
                            return string.Format(binding.StringFormat, value);
                        }
                        
                        return value?.ToString() ?? "";
                    }
                }
            }
            return "";
        }
    }
}