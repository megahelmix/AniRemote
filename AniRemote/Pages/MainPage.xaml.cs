using AniRemote.Pages;
using AniRemote.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using static AniRemote.ViewModels.AnimViewModel;

namespace AniRemote
{
	public partial class MainPage : ContentPage
	{
		//int count = 0;
		AnimViewModel ViewModel { get; set; }

		public MainPage(AnimViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
			ViewModel = viewModel;
			btnWideOpen.Clicked += BtnWideOpen_Clicked;
			btnHalfOpen.Clicked += BtnHalfOpen_Clicked;
			btnEyesClosed.Clicked += BtnEyesClosed_Clicked;
		}

		private void BtnEyesClosed_Clicked(object? sender, EventArgs e)
		{
			sliderLids.Value = sliderLids.Minimum;
		}

		private void BtnHalfOpen_Clicked(object? sender, EventArgs e)
		{
			sliderLids.Value = 0;
		}

		private void BtnWideOpen_Clicked(object? sender, EventArgs e)
		{
			sliderLids.Value = sliderLids.Maximum;
		}

		Point ptZero;
		const double cDrawLimit = 80.0;
		const double cTransFactor = 1000.0 / cDrawLimit;
		private void DrawView_PointDrawn(object? sender, CommunityToolkit.Maui.Core.PointDrawnEventArgs e)
		{
			double x = e.Point.X - ptZero.X;
			double y = e.Point.Y - ptZero.Y;

			if (x > cDrawLimit) { x = cDrawLimit; }
			else if (x < -cDrawLimit) { x = -cDrawLimit; }
			if (y > cDrawLimit) { y = cDrawLimit; }
			else if (y < -cDrawLimit) { y = -cDrawLimit; }

			lblCoord.Text = $"X: {x.ToString("f1")}\nY: {y.ToString("f1")}";
			image1.TranslationX = x;
			image1.TranslationY = y;

			_ = ViewModel.EyeCoordinatesChanged((short)(x * cTransFactor), (short)(y * cTransFactor));
		}

		private void DrawView_DrawingLineStarted(object sender, CommunityToolkit.Maui.Core.DrawingLineStartedEventArgs e)
		{
			ptZero = e.Point;
		}

		private void DrawView_DrawingLineCompleted(object sender, CommunityToolkit.Maui.Core.DrawingLineCompletedEventArgs e)
		{
			image1.TranslationX = image1.TranslationY = 0;
			lblCoord.Text = $"X:0: Y:0";
			_ = ViewModel.EyeCoordinatesChanged(0, 0);
		}

		private blinkButton getBlinkButton(object btn)
		{
			if (btn != null)
			{
				if (btn == btnWinkLeft) return blinkButton.Left;
				if (btn == btnWinkRight) return blinkButton.Right;
			}
			return blinkButton.Both;
		}

		private void sliderLids_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			_ = ViewModel.LidsChanged((short)(e.NewValue * 10));
		}

		private void btnBlink_Pressed(object sender, EventArgs e)
		{
			_ = ViewModel.BlinkChanged(blinkButton.Both, true);
		}

		private void btnBlink_Released(object sender, EventArgs e)
		{
			_ = ViewModel.BlinkChanged(blinkButton.Both, false);
		}

		private void btnWinkLeft_Pressed(object sender, EventArgs e)
		{
			_ = ViewModel.BlinkChanged(blinkButton.Left, true);
		}

		private void btnWinkLeft_Released(object sender, EventArgs e)
		{
			_ = ViewModel.BlinkChanged(blinkButton.Left, false);
		}

		private void btnWinkRight_Pressed(object sender, EventArgs e)
		{
			_ = ViewModel.BlinkChanged(blinkButton.Right, true);
		}

		private void btnWinkRight_Released(object sender, EventArgs e)
		{
			_ = ViewModel.BlinkChanged(blinkButton.Right, false);
		}

		private void btnRangeCheck_Clicked(object sender, EventArgs e)
		{
			ViewModel.SwitchToRangeCheckAsyncCommand.Execute(this);
			Navigation.PushAsync(new RangeCheck(ViewModel));
		}

		private void ContentPage_Appearing(object sender, EventArgs e)
		{
			ViewModel.CheckBluetoothAvailabilityAsyncCommand.Execute(this);
		}
	}

}
