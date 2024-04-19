//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Maui.Controls;

//namespace Client.UI.Services
//{
//    public class NavigationService
//    {
//        public async Task NavigateToPage(string pageName)
//        {
//            await Shell.Current.GoToAsync($"//{pageName}");
//        }

//        public async Task NavigateToPage(string pageName, string parameter)
//        {
//            await Shell.Current.GoToAsync($"//{pageName}?{parameter}");
//        }

//        public async Task NavigateToPage(string pageName, string parameter1, string parameter2)
//        {
//            await Shell.Current.GoToAsync($"//{pageName}?{parameter1}&{parameter2}");
//        }

//        public async Task NavigateBack()
//        {
//            await Shell.Current.GoToAsync("..");
//        }
//    }
//}
