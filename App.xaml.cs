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
		public App()
		{
			// Catch all unhandled exceptions
			this.DispatcherUnhandledException += App_DispatcherUnhandledException;
			System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			try
			{
				var logPath = @"C:\Temp\CSuiteView_CRASH.log";
				var message = $"=== UNHANDLED EXCEPTION ===\n" +
					$"Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\n" +
					$"Exception Type: {e.Exception.GetType().FullName}\n" +
					$"Message: {e.Exception.Message}\n" +
					$"Stack Trace:\n{e.Exception.StackTrace}\n" +
					$"Inner Exception: {e.Exception.InnerException?.ToString() ?? "None"}\n" +
					$"==========================\n\n";

				System.IO.File.AppendAllText(logPath, message);

				MessageBox.Show($"Application crashed. Error logged to:\n{logPath}\n\nError: {e.Exception.Message}",
					"Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch { }

			e.Handled = false; // Let it crash
		}

		private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
		{
			try
			{
				var logPath = @"C:\Temp\CSuiteView_CRASH.log";
				var ex = e.ExceptionObject as System.Exception;
				var message = $"=== UNHANDLED DOMAIN EXCEPTION ===\n" +
					$"Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\n" +
					$"Exception: {ex?.ToString() ?? e.ExceptionObject.ToString()}\n" +
					$"===================================\n\n";

				System.IO.File.AppendAllText(logPath, message);
			}
			catch { }
		}

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

