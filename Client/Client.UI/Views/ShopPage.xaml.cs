namespace Client.UI.Views;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels;


public partial class ShopPage : ContentPage
{
    public ShopPage(ShopViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}