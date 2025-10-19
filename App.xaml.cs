using System.Windows;
using CSuiteViewWPF.Windows;

namespace CSuiteViewWPF
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// MVVM Pattern: Startup logic simplified - ViewModels handle all business logic
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			try
			{
				// Create and show the main window with MVVM architecture
				// ViewModel is set in MainWindow.xaml
				var mainWindow = new MainWindow();
				mainWindow.Show();
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

