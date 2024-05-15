using System;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;


namespace Client.UI.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            if (Application.Current is App app)
            {
                var viewModel = app.ServiceProvider.GetService<LoginViewModel>();
                if (viewModel == null) throw new InvalidOperationException("ViewModel kan ikke være null");
                BindingContext = viewModel;
            }
        }

        private void OnPasswordEntryCompleted(object sender, EventArgs e)
        {
            if (BindingContext is LoginViewModel viewModel)
            {
                if (viewModel.LoginOnPlatformCommand.CanExecute(null))
                {
                    viewModel.LoginOnPlatformCommand.Execute(null);
                }
            }
        }

    }

}
