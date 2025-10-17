using CSuiteViewWPF.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CSuiteViewWPF
{
    public partial class TemplateMainWindow : Window
    {
        private int panelCount = 3;
        private int treePanelIndex = -1; // -1 = none

        // Expose properties so the creator can set panel counts in code
        public int PanelCount
        {
            get => panelCount;
            set
            {
                // allow 0 panels to completely hide the panel layer
                panelCount = Math.Max(0, Math.Min(6, value));
                if (treePanelIndex >= panelCount) treePanelIndex = -1;
                BuildPanels();
            }
        }

        public int TreePanelIndex
        {
            get => treePanelIndex;
            set
            {
                if (value < 0) treePanelIndex = -1;
                else if (panelCount == 0) treePanelIndex = -1;
                else treePanelIndex = Math.Max(0, Math.Min(panelCount - 1, value));
                BuildPanels();
            }
        }

        public TemplateMainWindow()
        {
            InitializeComponent();

            // default
            panelCount = 3;
            treePanelIndex = -1;
            // default layout sizes
            HeaderHeight = 48;
            FooterHeight = 36;
            SpaceAbove = 12;
            SpaceBelow = 18;
            ApplyLayoutSizes();
            BuildPanels();
            // Allow dragging by mouse down on the header area only
            HeaderBar.MouseLeftButtonDown += HeaderBar_MouseLeftButtonDown;
            // Allow dragging by mouse down on the footer bar as well
            FooterBar.MouseLeftButtonDown += FooterBar_MouseLeftButtonDown;
        }

        // Allow external code to set the header title text
        public string HeaderTitle
        {
            get => HeaderTitleTextBlock?.Text ?? string.Empty;
            set
            {
                if (HeaderTitleTextBlock != null)
                    HeaderTitleTextBlock.Text = value ?? string.Empty;
            }
        }

        private void HeaderBar_MouseLeftButtonDown(object? sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void FooterBar_MouseLeftButtonDown(object? sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void BuildPanels()
        {
            PanelsGrid.Children.Clear();
            PanelsGrid.ColumnDefinitions.Clear();

            // If no panels are requested, collapse the PanelsGrid area and return
            if (panelCount == 0)
            {
                if (MiddleBorder != null) MiddleBorder.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                if (MiddleBorder != null) MiddleBorder.Visibility = Visibility.Visible;
            }

            // Build columns with splitters between them
            for (int i = 0; i < panelCount; i++)
            {
                PanelsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // for all but the last column, add a narrow splitter column
                if (i < panelCount - 1)
                {
                    PanelsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
                }
            }

            for (int i = 0; i < panelCount; i++)
            {
                int colIndex = i * 2; // because we added splitter columns between

                var border = new Border
                {
                    Background = (System.Windows.Media.Brush)FindResource("PanelLight"),
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(0),
                    Padding = new Thickness(8),
                    BorderBrush = (System.Windows.Media.Brush)FindResource("MediumBlue"),
                    BorderThickness = new Thickness(2)
                };

                Grid.SetColumn(border, colIndex);
                PanelsGrid.Children.Add(border);

                var content = new DockPanel();
                border.Child = content;

                // Place a header area at top of panel
                var hdr = new TextBlock { Text = $"Panel {i + 1}", FontWeight = FontWeights.Bold, Margin = new Thickness(0,0,0,8) };
                DockPanel.SetDock(hdr, Dock.Top);
                content.Children.Add(hdr);

                // If this is the chosen tree panel, add the specialized NavigationTreeControl
                if (treePanelIndex == i)
                {
                    // For leftmost panel use the NavigationTreeControl; for others, you could reuse it as well
                    var nav = new NavigationTreeControl();
                    DockPanel.SetDock(nav, Dock.Top);
                    content.Children.Add(nav);
                }
                else
                {
                    // placeholder content
                    content.Children.Add(new TextBlock { Text = "Content area", Opacity = 0.9 });
                }

                // add splitter column after this panel (except last)
                if (i < panelCount - 1)
                {
                    var splitter = new GridSplitter
                    {
                        Width = 6,
                        Background = (System.Windows.Media.Brush)FindResource("MediumBlue"),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        ShowsPreview = true
                    };
                    Grid.SetColumn(splitter, colIndex + 1);
                    PanelsGrid.Children.Add(splitter);
                }
            }
        }

        // Layout properties
        private int headerHeight;
        private int footerHeight;
        private int spaceAbove;
        private int spaceBelow;
    private bool footerVisible = true;

        public int HeaderHeight
        {
            get => headerHeight;
            set
            {
                headerHeight = Math.Max(0, value);
                ApplyLayoutSizes();
            }
        }

        public int FooterHeight
        {
            get => footerHeight;
            set
            {
                footerHeight = Math.Max(0, value);
                ApplyLayoutSizes();
            }
        }

        public int SpaceAbove
        {
            get => spaceAbove;
            set
            {
                spaceAbove = Math.Max(0, value);
                ApplyLayoutSizes();
            }
        }

        public int SpaceBelow
        {
            get => spaceBelow;
            set
            {
                spaceBelow = Math.Max(0, value);
                ApplyLayoutSizes();
            }
        }

        // If false, the footer and its thin gold separator line are removed from layout
        public bool FooterVisible
        {
            get => footerVisible;
            set
            {
                footerVisible = value;
                ApplyLayoutSizes();
            }
        }

        private void ApplyLayoutSizes()
        {
            // Update grid row heights
            if (MainContentGrid != null && MainContentGrid.RowDefinitions.Count >= 5)
            {
                MainContentGrid.RowDefinitions[0].Height = new GridLength(HeaderHeight, GridUnitType.Pixel);

                // Always keep the thin gold line under the header (row 1). Only collapse the footer and
                // the fine gold line above it (rows 3 and 4) when FooterVisible is false.
                MainContentGrid.RowDefinitions[1].Height = new GridLength(2, GridUnitType.Pixel); // fine gold line under header
                MainContentGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);

                if (FooterVisible)
                {
                    MainContentGrid.RowDefinitions[3].Height = new GridLength(2, GridUnitType.Pixel); // fine gold line above footer
                    MainContentGrid.RowDefinitions[4].Height = new GridLength(FooterHeight, GridUnitType.Pixel);
                    if (FooterBar != null) FooterBar.Visibility = Visibility.Visible;
                }
                else
                {
                    // collapse footer-related rows (preserve header's thin line)
                    MainContentGrid.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Pixel);
                    MainContentGrid.RowDefinitions[4].Height = new GridLength(0, GridUnitType.Pixel);
                    if (FooterBar != null) FooterBar.Visibility = Visibility.Collapsed;
                }

                // Apply space above/below to middle Border padding
                var middleBorder = MainContentGrid.Children.OfType<Border>().FirstOrDefault(b => Grid.GetRow(b) == 2);
                if (middleBorder != null)
                {
                    middleBorder.Padding = new Thickness(12, SpaceAbove, 12, SpaceBelow);
                }
            }
        }

        private TreeViewItem BuildTreeItem(Models.NavigationNode node)
        {
            var tvi = new TreeViewItem { Header = node.Name, IsExpanded = true };
            if (node.Children != null && node.Children.Any())
            {
                foreach (var c in node.Children)
                {
                    tvi.Items.Add(BuildTreeItem(c));
                }
            }
            return tvi;
        }

        // Toolbar removed — set PanelCount and TreePanelIndex from code

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
