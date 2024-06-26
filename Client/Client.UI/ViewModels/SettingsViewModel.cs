﻿using Client.Library.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using Client.Library.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;


namespace Client.UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {

        private readonly INavigationService _navigationService;
        public SettingsViewModel(IApiService apiService, INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        public async Task GoBack()
        {
           await _navigationService.NavigateBack();
        }

    }
}
