using System;
using Client.UI.Services;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace Client.UI.Views 
{ 
	public partial class LobbyPage : ContentPage
	{	
		
		public LobbyPage(LobbyViewModel vm)
		{
			this.BindingContext = vm;
            InitializeComponent();
		}
	}
}