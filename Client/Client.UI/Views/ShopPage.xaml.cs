namespace Client.UI.Views;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels;


public partial class ShopPage : ContentPage
{
    public ShopPage(ShopViewModel vm)
    {
        InitializeComponent();
        this.Appearing += OnPageAppearing;
        BindingContext = vm;
    }
    private void OnPageAppearing(object sender, EventArgs e)
    {
        // Call the base implementation first
        if (BindingContext is ShopViewModel viewModel)
        {
            viewModel.OnPageAppearing();
        }
    }

}