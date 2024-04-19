using Client.UI.ViewModels;

namespace Client.UI.Views;

public partial class LoadingPage : ContentPage
{
    private LoadingViewModel _vm;
	public LoadingPage(LoadingViewModel vm)
	{
		InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        await _vm.OnNavigatedTo();
    }
}