﻿using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE;
using AniRemote.Models;
using AniRemote.ViewModels;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using System;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;

namespace AniRemote.Services
{
	public class BluetoothLEService
	{
		public DeviceCandidate SelectedDeviceCandidate
		{
			get
			{
				if ((DeviceCandidateList != null) && (DeviceCandidateList.Count > 0))
				{
					return DeviceCandidateList[0];
				}
				return new DeviceCandidate();
			}
		}
		public List<DeviceCandidate>? DeviceCandidateList { get; private set; }
		public IBluetoothLE BluetoothLE { get; private set; }
		public IAdapter Adapter { get; private set; }
		public IDevice? Device { get; set; }

		public BluetoothLEService()
		{
			BluetoothLE = CrossBluetoothLE.Current;
			Adapter = CrossBluetoothLE.Current.Adapter;
			Adapter.ScanTimeout = 4000;

#pragma warning disable CS8622 // Die NULL-Zulässigkeit von Verweistypen im Typ des Parameters entspricht (möglicherweise aufgrund von Attributen für die NULL-Zulässigkeit) nicht dem Zieldelegaten.
			Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
			Adapter.DeviceConnected += Adapter_DeviceConnected;
			Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
			Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
			BluetoothLE.StateChanged += BluetoothLE_StateChanged;
#pragma warning restore CS8622 // Die NULL-Zulässigkeit von Verweistypen im Typ des Parameters entspricht (möglicherweise aufgrund von Attributen für die NULL-Zulässigkeit) nicht dem Zieldelegaten.
		}

		public async Task<List<DeviceCandidate>> ScanForDevicesAsync()
		{
			DeviceCandidateList = new List<DeviceCandidate>();

			try
			{
				IReadOnlyList<IDevice> systemDevices = Adapter.GetSystemConnectedOrPairedDevices(AnimViewModel.serviceUuids);
				foreach (var systemDevice in systemDevices)
				{
					DeviceCandidate? deviceCandidate = DeviceCandidateList.FirstOrDefault(d => d.Id == systemDevice.Id);
					if (deviceCandidate == null)
					{
						DeviceCandidateList.Add(new DeviceCandidate
						{
							Id = systemDevice.Id,
							Name = systemDevice.Name,
						});
						await ShowToastAsync($"Found {systemDevice.State.ToString().ToLower()} device {systemDevice.Name}.");
					}
				}
				await Adapter.StartScanningForDevicesAsync(AnimViewModel.serviceUuids);
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert($"Unable to scan nearby Bluetooth LE devices", $"{ex.Message}.", "OK");
			}

			return DeviceCandidateList;
		}

		#region DeviceEventArgs
		private async void Adapter_DeviceDiscovered(object sender, DeviceEventArgs e)
		{
			DeviceCandidate? deviceCandidate = DeviceCandidateList?.FirstOrDefault(d => d.Id == e.Device.Id);
			if (deviceCandidate == null)
			{
				DeviceCandidateList?.Add(new DeviceCandidate
				{
					Id = e.Device.Id,
					Name = e.Device.Name,
				});
				await ShowToastAsync($"Found {e.Device.State.ToString().ToLower()} {e.Device.Name}.");
			}
		}

		private void Adapter_DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
		{
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				try
				{
					await ShowToastAsync($"{e.Device.Name} connection is lost.");
				}
				catch
				{
					await ShowToastAsync($"Device connection is lost.");
				}
			});
		}

		private void Adapter_DeviceConnected(object sender, DeviceEventArgs e)
		{
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				try
				{
					await ShowToastAsync($"{e.Device.Name} is connected.");
				}
				catch
				{
					await ShowToastAsync($"Device is connected.");
				}
			});
		}

		private void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
		{
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				try
				{
					await ShowToastAsync($"{e.Device.Name} is disconnected.");
				}
				catch
				{
					await ShowToastAsync($"Device is disconnected.");
				}
			});
		}
		#endregion DeviceEventArgs

		#region BluetoothStateChangedArgs
		private void BluetoothLE_StateChanged(object sender, BluetoothStateChangedArgs e)
		{
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				try
				{
					await ShowToastAsync($"Bluetooth state is {e.NewState}.");
				}
				catch
				{
					await ShowToastAsync($"Bluetooth state has changed.");
				}
			});
		}
		#endregion BluetoothStateChangedArgs

#if ANDROID
		#region BluetoothPermissions
		public async Task<PermissionStatus> CheckBluetoothPermissions()
		{
			PermissionStatus status = PermissionStatus.Unknown;
			try
			{
				status = await Permissions.CheckStatusAsync<BluetoothLEPermissions>();
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert($"Unable to check Bluetooth LE permissions", $"{ex.Message}.", "OK");
			}
			return status;
		}

		public async Task<PermissionStatus> RequestBluetoothPermissions()
		{
			PermissionStatus status = PermissionStatus.Unknown;
			try
			{
				status = await Permissions.RequestAsync<BluetoothLEPermissions>();
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert($"Unable to request Bluetooth LE permissions", $"{ex.Message}.", "OK");
			}
			return status;
		}
		#endregion BluetoothPermissions
#elif IOS
#elif WINDOWS
#endif

		public async Task ShowToastAsync(string message)
		{
			ToastDuration toastDuration = ToastDuration.Long;
			IToast toast = Toast.Make(message, toastDuration);
			await toast.Show();
		}
	}
}
