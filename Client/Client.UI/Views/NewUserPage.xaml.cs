namespace Client.UI.Views;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels;


public partial class NewUserPage : ContentPage
{
	public NewUserPage(NewUserViewModel vm)
	{
        InitializeComponent();
        BindingContext = vm;
    }
}