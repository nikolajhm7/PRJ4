namespace Client.UI.Views;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels;


public partial class NewUserPage : ContentPage
{
	public NewUserPage()
	{
        if (Application.Current is App app)
        {
            Console.WriteLine("Application.Current is App app");
            var viewModel = app.ServiceProvider.GetService<LoginViewModel>();
            if (viewModel == null) throw new InvalidOperationException("ViewModel kan ikke være null");
            BindingContext = viewModel;
        }
    }
}