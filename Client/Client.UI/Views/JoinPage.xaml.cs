using System;
using Client.UI.Services;
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
	}
}