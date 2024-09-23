using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace AniRemote.Pages;

public partial class RangeSlider : ContentView
{

	public static readonly BindableProperty HeaderTextProperty = BindableProperty.Create(
		 nameof(HeaderText),
		 typeof(string),
		 typeof(RangeSlider),
		 default(string),
		 propertyChanged: null
		 );
	public string HeaderText
	{
		get => (string)this.GetValue(HeaderTextProperty);
		set => this.SetValue(HeaderTextProperty, value);
	}

	public static readonly BindableProperty SliderValueProperty = BindableProperty.Create(
		 nameof(SliderValue),
		 typeof(int),
		 typeof(RangeSlider),
		 default(int),
		 propertyChanged: null
		 );
	public int SliderValue
	{
		get => (int)this.GetValue(SliderValueProperty);
		set => this.SetValue(SliderValueProperty, value);
	}

	public RangeSlider()
	{
		InitializeComponent();
		BindingContext = this;
		//SliderValue = 90;
	}

	private void sliderValue_ValueChanged(object sender, ValueChangedEventArgs e)
	{

	}
}