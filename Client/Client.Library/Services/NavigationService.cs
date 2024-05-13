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

        // Removes the current page from the navigation stack
        public async Task RemoveLastPageFromStack()
        {
            var navigationStack = Shell.Current.Navigation.NavigationStack;
            if (navigationStack.Count > 1) // Ensure there's at least two pages on the stack
            {
                Page currentPage = Shell.Current.Navigation.NavigationStack.LastOrDefault();
                if (currentPage != null)
                {
                    Shell.Current.Navigation.RemovePage(currentPage);
                }
            }
        }
    }
}
