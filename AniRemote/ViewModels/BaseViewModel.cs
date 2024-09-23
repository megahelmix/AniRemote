using CommunityToolkit.Mvvm.ComponentModel;

namespace AniRemote.ViewModels
{
	public partial class BaseViewModel : ObservableObject
	{
		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(IsNotBusy))]
		bool isBusy;

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(IsNotScanning))]
		bool isScanning;

		[ObservableProperty]
		string title = string.Empty;

		public bool IsNotBusy => !IsBusy;
		public bool IsNotScanning => !IsScanning;
	}
}
