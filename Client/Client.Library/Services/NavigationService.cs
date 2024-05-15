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

        public async Task NavigateBackToPage(string page)
        {
            // Construct the target page path
            string AuthenticatedPagePath = $"//Loading/{page}";
            string LoggedInPagePath = $"//Loading/LoginPage/{page}";

            // Loop until the current location matches the target page or the root page
            while (Shell.Current.CurrentState.Location.OriginalString != AuthenticatedPagePath
                   && Shell.Current.CurrentState.Location.OriginalString != LoggedInPagePath
                   && Shell.Current.CurrentState.Location.OriginalString != "//Loading")
            {
                await Shell.Current.GoToAsync("..");

                // To prevent the loop from running too fast
                await Task.Delay(300);
            }
        }

    }
}
