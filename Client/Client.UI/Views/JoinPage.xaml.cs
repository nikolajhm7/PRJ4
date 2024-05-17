using System;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace Client.UI.Views {
	public partial class JoinPage : ContentPage
	{
		public JoinPage(JoinViewModel vm)
		{
			InitializeComponent();
			BindingContext = vm;
		}
        private void OnGuestNameComplete(object sender, EventArgs e)
        {
            if (BindingContext is JoinViewModel viewModel)
            {
                if (viewModel.GoToLobbyCommand.CanExecute(null))
                {
                    viewModel.GoToLobbyCommand.Execute(null);
                }
            }
        }
    }
}