using Client.Library.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.UI.Views;
using Client.Library.Services;
using Client.Library.Services.Interfaces;

namespace Client.UI.ViewModels
{
    public class LoadingViewModel : ObservableObject
    {
        private IJwtTokenService _jwtTokenService;
        private INavigationService _navigationService;
        public LoadingViewModel(IJwtTokenService jwtTokenService, INavigationService navigationService)
        {
            _jwtTokenService = jwtTokenService;
            _navigationService = navigationService;
        }

        public async Task OnNavigatedTo()
        {
            await Task.Delay(500);
            if (await _jwtTokenService.IsAuthenticated())
            {
                await _navigationService.NavigateToPage(nameof(PlatformPage));
                //await Shell.Current.GoToAsync($"{nameof(PlatformPage)}");
            }
            else
            {
                await _navigationService.NavigateToPage(nameof(LoginPage));
                //await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
            }
        }
    }
}
