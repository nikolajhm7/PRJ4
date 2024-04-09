namespace Client.UI.Views;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels;


public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}