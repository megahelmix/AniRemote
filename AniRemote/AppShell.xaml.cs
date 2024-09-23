using AniRemote.Pages;
using Microsoft.Maui.Controls;

namespace AniRemote
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
			Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
			Routing.RegisterRoute(nameof(RangeCheck), typeof(RangeCheck));
		}
	}
}
