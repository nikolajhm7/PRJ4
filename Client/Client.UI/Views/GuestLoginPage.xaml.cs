namespace Client.UI.Views;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels;


public partial class GuestLoginPage : ContentPage
{
	public GuestLoginPage()
	{
        InitializeComponent();
        if (Application.Current is App app)
        {
            var viewModel = app.ServiceProvider.GetService<GuestLoginViewModel>();
            BindingContext = viewModel;
        }
    }
}