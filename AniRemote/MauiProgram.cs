using AniRemote.Pages;
using AniRemote.Services;
using AniRemote.ViewModels;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace AniRemote
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.UseMauiCommunityToolkitCore();
#if DEBUG
			builder.Logging.AddDebug();
#endif
			builder.Services.AddSingleton<BluetoothLEService>();
			builder.Services.AddSingleton<AnimViewModel>();
			builder.Services.AddSingleton<MainPage>();
			builder.Services.AddSingleton<RangeCheck>();

			return builder.Build();
		}
	}
}
