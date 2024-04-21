//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Maui.Controls;

namespace Client.UI.Services
{
    public class NavigationService
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
