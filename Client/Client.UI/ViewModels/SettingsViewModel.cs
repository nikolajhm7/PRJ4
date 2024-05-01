using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Library.Interfaces;
using Client.Library.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Client.Library.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Client.Library.Services;

namespace Client.UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {

        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;
        public SettingsViewModel(IApiService apiService, INavigationService navigationService)
        {
            _apiService = apiService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        public async Task GoBack()
        {
           await _navigationService.NavigateBack();
        }

    }
}
