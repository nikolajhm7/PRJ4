﻿
using Client.Libary.Services.Interfaces;

namespace Client.Libary.Services
{
    public class NavigationService: INavigationService
    {
        public async Task NavigateToPage(string page)
        {
            await Shell.Current.GoToAsync(page);
        }

        public async Task NavigateBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
