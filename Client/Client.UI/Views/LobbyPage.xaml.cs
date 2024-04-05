using System;
using Client.UI.Services;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace Client.UI.Views 
{ 
	public partial class LobbyPage : ContentPage
	{	
		
		public LobbyPage()
		{
            var LobbyService = new LobbyService(new ConnectionService());
			var vm = new LobbyViewModel(LobbyService);
			this.BindingContext = vm;
            InitializeComponent();
		}
	}
}