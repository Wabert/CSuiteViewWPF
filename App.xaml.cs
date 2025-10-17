using System.Windows;

namespace CSuiteViewWPF
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			try
			{
				// Create a TemplateMainWindow configured with no panels and no footer.
				// Put three buttons into the middle area: Screen shot, Directory Scan, Test Windows.
				var tmpl = new TemplateMainWindow();
				tmpl.PanelCount = 0;
				tmpl.FooterVisible = false;

				var middleBorder = tmpl.FindName("MiddleBorder") as System.Windows.Controls.Border;
				if (middleBorder != null)
				{
					var stack = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Vertical, Margin = new Thickness(12,12,0,0), VerticalAlignment = System.Windows.VerticalAlignment.Top, HorizontalAlignment = System.Windows.HorizontalAlignment.Left };

					// Screen shot (brass button)
					var screenshotBtn = new System.Windows.Controls.Button { Content = "Screen shot", Width = 180, Height = 30, Margin = new Thickness(0, 6, 0, 6), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
					try { screenshotBtn.Style = (System.Windows.Style)Current.FindResource("RoundedBrassButtonStyle"); } catch { }
				screenshotBtn.Click += (s2, e2) => { /* wiring later */ };
					stack.Children.Add(screenshotBtn);

					// Directory Scan (dark blue / gold button)
					var dirScanBtn = new System.Windows.Controls.Button { Content = "Directory Scan", Width = 180, Height = 30, Margin = new Thickness(0, 6, 0, 6), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
					try { dirScanBtn.Style = (System.Windows.Style)Current.FindResource("DarkBlueGoldButton"); } catch { }
				dirScanBtn.Click += (s3, e3) => { /* wiring later */ };
					stack.Children.Add(dirScanBtn);

					// Test Windows (open a TemplateMainWindow with TemplateCreatorControl)
					var testWinBtn = new System.Windows.Controls.Button { Content = "Test Windows", Width = 180, Height = 30, Margin = new Thickness(0, 6, 0, 6), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
					try { testWinBtn.Style = (System.Windows.Style)Current.FindResource("RoundedSilverButtonStyle"); } catch { }
				testWinBtn.Click += (s4, e4) =>
					{
						var w = new TemplateMainWindow();
						w.PanelCount = 0;
						w.FooterVisible = false;
						var creator = new TemplateCreatorControl();
						var mb = w.FindName("MiddleBorder") as System.Windows.Controls.Border;
						if (mb != null)
						{
							mb.Child = creator;
							mb.Visibility = System.Windows.Visibility.Visible;
						}
						w.Show();
					};
					stack.Children.Add(testWinBtn);

					middleBorder.Child = stack;
					middleBorder.Visibility = System.Windows.Visibility.Visible;
				}

				tmpl.Show();
			}
			catch (System.Exception ex)
			{
				try
				{
					var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CSuiteViewWPF_startup_error.txt");
					System.IO.File.WriteAllText(path, ex.ToString());
				}
				catch { }
				// Re-throw so the process still fails visibly if running interactively
				throw;
			}
		}
	}
}

