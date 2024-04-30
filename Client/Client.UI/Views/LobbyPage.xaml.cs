using System;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace Client.UI.Views 
{ 
	public partial class LobbyPage : ContentPage
	{
		private LobbyViewModel _vm;
		public LobbyPage(LobbyViewModel vm)
		{
			this.BindingContext = vm;
			_vm = vm;
            InitializeComponent();
		}

		protected async override void OnAppearing()
		{
			base.OnAppearing();

			await _vm.OnPageappearing();
		}
	}
}