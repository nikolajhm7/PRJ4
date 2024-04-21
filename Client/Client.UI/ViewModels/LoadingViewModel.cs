using Client.Libary.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.UI.Views;

namespace Client.UI.ViewModels
{
    public class LoadingViewModel : ObservableObject
    {
        private IJwtTokenService _jwtTokenService;
        public LoadingViewModel(IJwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }

        public async Task OnNavigatedTo()
        {
            //await Task.Delay(2000);
            if (await _jwtTokenService.IsAuthenticated())
            {
                await Shell.Current.GoToAsync($"{nameof(PlatformPage)}");
            }
            else
            {
                await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
            }
        }
    }
}
