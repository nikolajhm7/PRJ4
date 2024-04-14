namespace Client.UI.Views;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels;


public partial class NewUserPage : ContentPage
{
	public NewUserPage()
	{
        InitializeComponent();
        if (Application.Current is App app)
        {
            var viewModel = app.ServiceProvider.GetService<NewUserViewModel>();
            BindingContext = viewModel;
        }
    }
}