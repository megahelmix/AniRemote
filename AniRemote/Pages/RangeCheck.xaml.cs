using AniRemote.Pages;
using AniRemote.ViewModels;
using Microsoft.Maui.Controls;
using System;
using static AniRemote.ViewModels.AnimViewModel;

namespace AniRemote.Pages;

public partial class RangeCheck : ContentPage
{
	public int RsLeftRight
	{
		get { return rsLeftRight.SliderValue; }
		set { rsLeftRight.SliderValue = value; }
	}
	public int RsUpDown
	{
		get { return rsUpDown.SliderValue; }
		set { rsUpDown.SliderValue = value; }
	}
	public int RsBottomLeft
	{
		get { return rsBottomLeft.SliderValue; }
		set { rsBottomLeft.SliderValue = value; }
	}
	public int RsTopLeft
	{
		get { return rsTopLeft.SliderValue; }
		set { rsTopLeft.SliderValue = value; }
	}
	public int RsBottomRight
	{
		get { return rsBottomRight.SliderValue; }
		set { rsBottomRight.SliderValue = value; }
	}
	public int RsTopRight
	{
		get { return rsTopRight.SliderValue; }
		set { rsTopRight.SliderValue = value; }
	}

	AnimViewModel ViewModel { get; set; }
	public RangeCheck(AnimViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		ViewModel = viewModel;
		viewModel.rcPage = this;

		rsLeftRight.PropertyChanged += RsLeftRight_PropertyChanged;
		rsUpDown.PropertyChanged += RsUpDown_PropertyChanged;
		rsBottomLeft.PropertyChanged += RsBottomLeft_PropertyChanged;
		rsTopLeft.PropertyChanged += RsTopLeft_PropertyChanged;
		rsBottomRight.PropertyChanged += RsBottomRight_PropertyChanged;
		rsTopRight.PropertyChanged += RsTopRight_PropertyChanged;
	}

	private void RsLeftRight_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		_ = ViewModel.UpdateServoValue(ServoIdx.LeftRight, rsLeftRight.SliderValue);
	}

	private void RsUpDown_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		_ = ViewModel.UpdateServoValue(ServoIdx.UpDown, rsUpDown.SliderValue);
	}

	private void RsBottomLeft_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		_ = ViewModel.UpdateServoValue(ServoIdx.LidBottomLeft, rsBottomLeft.SliderValue);
	}

	private void RsTopLeft_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		_ = ViewModel.UpdateServoValue(ServoIdx.LidTopLeft, rsTopLeft.SliderValue);
	}

	private void RsBottomRight_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		_ = ViewModel.UpdateServoValue(ServoIdx.LidBottomRight, rsBottomRight.SliderValue);
	}

	private void RsTopRight_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		_ = ViewModel.UpdateServoValue(ServoIdx.LidTopRight, rsTopRight.SliderValue);
	}

	private void ContentPage_Appearing(object sender, EventArgs e)
	{
		_ = ViewModel.ReadServoPositions();
	}
}