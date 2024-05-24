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

    private void OnGuestNameComplete(object sender, EventArgs e)
    {
        if (BindingContext is GuestLoginViewModel viewModel)
        {
            if (viewModel.MakeNewUserCommand.CanExecute(null))
            {
                viewModel.MakeNewUserCommand.Execute(null);
            }
        }
    }
}