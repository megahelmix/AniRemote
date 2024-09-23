using Microsoft.Maui.Controls;

namespace AniRemote
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			MainPage = new AppShell();
		}
	}
}
