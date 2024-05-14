using System;
using Client.UI.ViewModels;
using Client.UI.ViewModels.Manager;
using Microsoft.Maui.Controls;

namespace Client.UI.Views 
{ 
	public partial class LobbyPage : ContentPage
	{
		private ViewModelFactory viewModelFactory;
		private LobbyViewModel _vm;
		public LobbyPage(ViewModelFactory viewModelFactory)
		{
			_vm = viewModelFactory.GetLobbyViewModel();
			this.BindingContext = _vm;
            InitializeComponent();
		}

		protected async override void OnAppearing()
		{
			base.OnAppearing();

			await _vm.OnPageappearing();
		}
	}
}