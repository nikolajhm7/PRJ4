using Client.Library.Models;
using Client.Library.Services.Interfaces;

namespace Client.Library.Services
{
    public class NavigationService : INavigationService
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
