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
				// Create a TemplateMainWindow configured with no panels and no footer,
				// then host the TemplateCreatorControl inside its middle area so the
				// creator UI runs inside the template.
				var tmpl = new TemplateMainWindow();
				tmpl.PanelCount = 0;
				tmpl.FooterVisible = false;

				var creator = new TemplateCreatorControl();

				// Find the MiddleBorder in the template and host the creator control
				var middleBorder = tmpl.FindName("MiddleBorder") as System.Windows.Controls.Border;
				if (middleBorder != null)
				{
					middleBorder.Child = creator;
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

