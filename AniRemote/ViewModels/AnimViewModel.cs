using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AniRemote.Services;
using AniRemote.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AniRemote.Pages;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;

namespace AniRemote.ViewModels
{
	public partial class AnimViewModel : BaseViewModel
	{
		private static Guid serviceUuid = new Guid("59d2855d-f4d3-4da7-8da1-697d057d25d1");
		public static Guid[] serviceUuids = { serviceUuid };

		private static Guid chara_EyeMovement_Uuid = new Guid("7e2bd36e-fb6e-4ce8-93ab-4410194bd15e");

		public BluetoothLEService BluetoothLEService { get; private set; }
		public ObservableCollection<DeviceCandidate> DeviceCandidates { get; } = new();

		public IAsyncRelayCommand CheckBluetoothAvailabilityAsyncCommand { get; }
		public IAsyncRelayCommand ScanNearbyDevicesAsyncCommand { get; }
		public IAsyncRelayCommand ConnectToDeviceCandidateAsyncCommand { get; }
		public IAsyncRelayCommand DisconnectFromDeviceAsyncCommand { get; }
		public IAsyncRelayCommand SwitchToRangeCheckAsyncCommand { get; }
		//public IAsyncRelayCommand TestClickAsyncCommand { get; }

		public IService? AnimatronicsService { get; private set; }
		public ICharacteristic? AnimatronicsCharacteristic { get; private set; }


		[ObservableProperty]
		private bool isBluetoothOn = false;
		[ObservableProperty]
		private bool scanDevicesSuccess = false;

		private bool isDeviceConnected = false;

		public AnimViewModel(BluetoothLEService bluetoothLEService)
		{
			BluetoothLEService = bluetoothLEService;

			ScanNearbyDevicesAsyncCommand = new AsyncRelayCommand(ScanDevicesAsync);
			CheckBluetoothAvailabilityAsyncCommand = new AsyncRelayCommand(CheckBluetoothAvailabilityAsync);
			ConnectToDeviceCandidateAsyncCommand = new AsyncRelayCommand(ConnectToDeviceCandidateAsync);

			DisconnectFromDeviceAsyncCommand = new AsyncRelayCommand(DisconnectFromDeviceAsync);

			SwitchToRangeCheckAsyncCommand = new AsyncRelayCommand(SwitchToRangeCheck);

			//TestClickAsyncCommand = new AsyncRelayCommand(TestClick);
		}

		partial void OnIsBluetoothOnChanged(bool value)
		{
			ScanNearbyDevicesAsyncCommand.Execute(value);
		}

		partial void OnScanDevicesSuccessChanged(bool value)
		{
			ConnectToDeviceCandidateAsyncCommand.Execute(value);
		}

		private async Task ConnectToDeviceCandidateAsync()
		{
			if (IsBusy)
			{
				return;
			}

			await BluetoothLEService.ShowToastAsync($"Connecting to device.");
			if (BluetoothLEService.SelectedDeviceCandidate.Id.Equals(Guid.Empty))
			{
				#region read device id from storage
				var device_name = await SecureStorage.Default.GetAsync("device_name");
				var device_id = await SecureStorage.Default.GetAsync("device_id");
				if (!string.IsNullOrEmpty(device_id))
				{
					BluetoothLEService.SelectedDeviceCandidate.Name = device_name ?? string.Empty;
					BluetoothLEService.SelectedDeviceCandidate.Id = Guid.Parse(device_id);
				}
				#endregion read device id from storage
				else
				{
					await BluetoothLEService.ShowToastAsync($"Select a Bluetooth LE device first. Try again.");
					return;
				}
			}

			if (!BluetoothLEService.BluetoothLE.IsOn)
			{
				await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
				return;
			}

			if (BluetoothLEService.Adapter.IsScanning)
			{
				await BluetoothLEService.ShowToastAsync($"Bluetooth adapter is scanning. Try again.");
				return;
			}

			try
			{
				IsBusy = true;

				if (BluetoothLEService.Device != null)
				{
					if (BluetoothLEService.Device.State == DeviceState.Connected)
					{
						if (BluetoothLEService.Device.Id.Equals(BluetoothLEService.SelectedDeviceCandidate.Id))
						{
							await BluetoothLEService.ShowToastAsync($"{BluetoothLEService.Device.Name} is already connected.");
							return;
						}

						if (BluetoothLEService.SelectedDeviceCandidate != null)
						{
							#region another device
							if (!BluetoothLEService.Device.Id.Equals(BluetoothLEService.SelectedDeviceCandidate.Id))
							{
								Title = $"{BluetoothLEService.SelectedDeviceCandidate.Name}";
								await DisconnectFromDeviceAsync();
								await BluetoothLEService.ShowToastAsync($"{BluetoothLEService.Device.Name} has been disconnected.");
							}
							#endregion another device
						}
					}
				}

				if (BluetoothLEService.SelectedDeviceCandidate == null)
				{
					throw new Exception("BluetoothLEService.SelectedDeviceCandidate is null!");
				}
				BluetoothLEService.Device = await BluetoothLEService.Adapter.ConnectToKnownDeviceAsync(BluetoothLEService.SelectedDeviceCandidate.Id);

				if (BluetoothLEService.Device.State == DeviceState.Connected)
				{
					AnimatronicsService = await BluetoothLEService.Device.GetServiceAsync(serviceUuid);
					if (AnimatronicsService != null)
					{
						AnimatronicsCharacteristic = await AnimatronicsService.GetCharacteristicAsync(chara_EyeMovement_Uuid);
						if (AnimatronicsCharacteristic != null)
						{
							await BluetoothLEService.ShowToastAsync($"{BluetoothLEService.Device.Name} connected.");
							isDeviceConnected = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (BluetoothLEService.SelectedDeviceCandidate != null)
				{
					await Shell.Current.DisplayAlert($"{BluetoothLEService.SelectedDeviceCandidate.Name}", $"Unable to connect to {BluetoothLEService.SelectedDeviceCandidate.Name}. {ex.Message}", "OK");
				}
				else
				{
					await Shell.Current.DisplayAlert($"No Device", $"Unable to connect to Unknown Device. {ex.Message}", "OK");
				}
			}
			finally
			{
				IsBusy = false;
			}
		}

		private async Task DisconnectFromDeviceAsync()
		{
			if (IsBusy)
			{
				return;
			}

			if (BluetoothLEService.Device == null)
			{
				await BluetoothLEService.ShowToastAsync($"Nothing to do.");
				return;
			}

			if (!BluetoothLEService.BluetoothLE.IsOn)
			{
				await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
				return;
			}

			if (BluetoothLEService.Adapter.IsScanning)
			{
				await BluetoothLEService.ShowToastAsync($"Bluetooth adapter is scanning. Try again.");
				return;
			}

			if (BluetoothLEService.Device.State == DeviceState.Disconnected)
			{
				await BluetoothLEService.ShowToastAsync($"{BluetoothLEService.Device.Name} is already disconnected.");
				return;
			}

			try
			{
				IsBusy = true;

				//await AnimatronicsCharacteristic.StopUpdatesAsync();

				await BluetoothLEService.Adapter.DisconnectDeviceAsync(BluetoothLEService.Device);

				//AnimatronicsCharacteristic.ValueUpdated -= HeartRateMeasurementCharacteristic_ValueUpdated;
			}
			catch (Exception)
			{
				await Shell.Current.DisplayAlert($"{BluetoothLEService.Device.Name}", $"Unable to disconnect from {BluetoothLEService.Device.Name}.", "OK");
			}
			finally
			{
				IsBusy = false;
				BluetoothLEService.Device?.Dispose();
				isDeviceConnected = false;
			}
		}

		async Task ScanDevicesAsync()
		{
			if (IsScanning)
			{
				return;
			}

			if (!BluetoothLEService.BluetoothLE.IsAvailable)
			{
				await Shell.Current.DisplayAlert($"Bluetooth", $"Bluetooth is missing.", "OK");
				return;
			}
#if ANDROID
			PermissionStatus permissionStatus = await BluetoothLEService.CheckBluetoothPermissions();
			if (permissionStatus != PermissionStatus.Granted)
			{
				permissionStatus = await BluetoothLEService.RequestBluetoothPermissions();
				if (permissionStatus != PermissionStatus.Granted)
				{
					await Shell.Current.DisplayAlert($"Bluetooth LE permissions", $"Bluetooth LE permissions are not granted.", "OK");
					return;
				}
			}
#elif IOS
#elif WINDOWS
#endif

			try
			{
				if (!BluetoothLEService.BluetoothLE.IsOn)
				{
					await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
					return;
				}

				IsScanning = true;

				List<DeviceCandidate> deviceCandidates = await BluetoothLEService.ScanForDevicesAsync();

				if (deviceCandidates.Count == 0)
				{
					await BluetoothLEService.ShowToastAsync($"Unable to find nearby Bluetooth LE devices. Try again.");
				}

				if (DeviceCandidates.Count > 0)
				{
					DeviceCandidates.Clear();
				}

				foreach (var deviceCandidate in deviceCandidates)
				{
					DeviceCandidates.Add(deviceCandidate);
				}
				ScanDevicesSuccess = true;
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert($"Unable to get nearby Bluetooth LE devices", $"{ex.Message}.", "OK");
			}
			finally
			{
				IsScanning = false;
			}
		}

		async Task CheckBluetoothAvailabilityAsync()
		{
			isRangeCheckActive = false;
			if (IsScanning)
			{
				return;
			}

			try
			{
				if (!BluetoothLEService.BluetoothLE.IsAvailable)
				{
					await Shell.Current.DisplayAlert($"Bluetooth", $"Bluetooth is missing.", "OK");
					return;
				}

				if (!IsBluetoothOn)
				{

					if (BluetoothLEService.BluetoothLE.IsOn)
					{
						IsBluetoothOn = true;
					}
					else
					{
						await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
					}
				}
				// On returning to MainPage set normal Mode
				if (blink != 0)
				{
					x = y = blink = 0;
					await UpdateBlinkState();
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert($"Unable to check Bluetooth availability", $"{ex.Message}.", "OK");
			}
		}

		// Characteristics Sent to Device
		short x = 0;
		short y = 0;
		short lidPos = 0;
		short blink = 0;
		short blnkState = 0;

		// Flags for Safe-Update of Characteristics to Device
		bool dirty = false;
		bool writeActive = false;

		public enum blinkButton
		{
			Left = 0b0001,
			Right = 0b0010,
			Both = 0b0011,
			RangeCheck = 0b10000000
		}
		public async Task EyeCoordinatesChanged(short x, short y)
		{
			this.x = x;
			this.y = y;
			dirty = true;
			await WriteCharacteristics();
		}
		public async Task BlinkChanged(blinkButton blinkButton, bool pressed)
		{
			if (pressed)
			{
				blink |= (short)blinkButton;
			}
			else
			{
				blink &= (short)~blinkButton;
			}
			await UpdateBlinkState();
		}

		private async Task UpdateBlinkState()
		{
			if (blink != blnkState)
			{
				blnkState = blink;
				dirty = true;
				await WriteCharacteristics();
			}
		}

		private async Task WriteCharacteristics()
		{
			if (writeActive)
			{
				return;
			}
			writeActive = true;
			while (dirty)
			{
				await UpdateCharaEyemovement();
			}

			writeActive = false;
		}

		private async Task UpdateCharaEyemovement()
		{
			await UpdateBleValues(BitConverter.GetBytes(x)
						.Concat(BitConverter.GetBytes(y))
						.Concat(BitConverter.GetBytes(lidPos))
						.Concat(BitConverter.GetBytes(blink))
						.ToArray());
		}

		private async Task UpdateBleValues(byte[] bytes)
		{
			if (!isDeviceConnected)
			{
				dirty = false;
				return;
			}
			if (IsBusy)
			{
				return;
			}
			try
			{
				IsBusy = true;
				if (AnimatronicsCharacteristic != null)
				{
					dirty = false;
					await AnimatronicsCharacteristic.WriteAsync(bytes);
				}
			}
			finally
			{
				IsBusy = false;
			}
		}

		public async Task LidsChanged(short val)
		{
			lidPos = val;
			dirty = true;
			await WriteCharacteristics();
		}

		#region RangeCheckMode
		const int cIntRawLength = 8;
		public enum ServoIdx
		{
			LeftRight = 0,
			UpDown,
			LidBottomLeft,
			LidTopLeft,
			LidBottomRight,
			LidTopRight
		}
		byte[] rawBleBytes = new byte[cIntRawLength];

		public RangeCheck? rcPage { get; set; }

		bool bWantReadServoPos = false;
		public async Task SwitchToRangeCheck()
		{
			blink = (short)blinkButton.RangeCheck;
			await UpdateBlinkState();
		}

		protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (bWantReadServoPos && (e.PropertyName == "IsBusy") && !IsBusy)
			{
				await ReadServoPositions();
			}
		}

		bool isRangeCheckActive = false;
		public async Task ReadServoPositions()
		{
			if (!isDeviceConnected)
			{
				return;
			}
			if (IsBusy)
			{
				bWantReadServoPos = true;
				return;
			}
			try
			{
				IsBusy = true;
				if ((AnimatronicsCharacteristic != null) && (rcPage != null))
				{
					var result = await AnimatronicsCharacteristic.ReadAsync();
					Array.Copy(result.data, 0, rawBleBytes, 0, cIntRawLength);
					rcPage.RsLeftRight = rawBleBytes[(int)ServoIdx.LeftRight];
					rcPage.RsUpDown = rawBleBytes[(int)ServoIdx.UpDown];
					rcPage.RsBottomLeft = rawBleBytes[(int)ServoIdx.LidBottomLeft];
					rcPage.RsTopLeft = rawBleBytes[(int)ServoIdx.LidTopLeft];
					rcPage.RsBottomRight = rawBleBytes[(int)ServoIdx.LidBottomRight];
					rcPage.RsTopRight = rawBleBytes[(int)ServoIdx.LidTopRight];
				}
			}
			finally
			{
				bWantReadServoPos = false;
				IsBusy = false;
				isRangeCheckActive = true;
			}
		}

		public async Task UpdateServoValue(ServoIdx servoIdx, int value)
		{
			if (isRangeCheckActive)
			{
				rawBleBytes[(int)servoIdx] = (byte)value;
				await UpdateBleValues(rawBleBytes);
			}
		}
		#endregion
	}
}
