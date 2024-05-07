using Client.UI.ViewModels;
using System.Runtime.CompilerServices;

namespace Client.UI.Views;

public partial class FriendsView : ContentView
{
	public FriendsViewModel ViewModel { get; set; }
	public FriendsView()
	{
		InitializeComponent();
		var vm = (Application.Current as App)?.ServiceProvider.GetService<FriendsViewModel>();
		if (vm != null)
		{
            ViewModel = vm;
			Loaded += ViewModel.OnLoaded;
        }
	}

	public static readonly BindableProperty CanInviteProperty =
		BindableProperty.Create(nameof(CanInvite),
			typeof(bool),
			typeof(FriendsView),
			default(bool),
			propertyChanged: (BindableObject bindable, object oldValue, object newValue) =>
			{
				{
					if (bindable is FriendsView view)
					{
						view.CanInvite = (bool)newValue;
                    }
				}
			});

	public bool CanInvite
    {
        get => (bool)GetValue(CanInviteProperty);
        set {
			SetValue(CanInviteProperty, value);
            ViewModel.CanInvite = value;
        }
    }
}