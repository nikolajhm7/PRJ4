using Client.Libary.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.UI.Views;
using Client.Libary.Services;

namespace Client.UI.ViewModels
{
    public class LoadingViewModel : ObservableObject
    {
        private IJwtTokenService _jwtTokenService;
        private NavigationService _navigationService;
        public LoadingViewModel(IJwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
            _navigationService = new NavigationService();
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
