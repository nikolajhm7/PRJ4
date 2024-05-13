using Client.Library.Games;
using Client.Library.Services.Interfaces;
using Client.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.UI.Games;

namespace Client.UI.ViewModels.Manager
{
    public class ViewModelFactory
    {
        private readonly IHangmanService _hangmanService;
        private readonly ILobbyService _lobbyService;
        private readonly INavigationService _navigationService;
        private HangmanViewModel _hangmanViewModel;

        public ViewModelFactory(IHangmanService hangmanService, ILobbyService lobbyService, INavigationService navigationService)
        {
            _hangmanService = hangmanService;
            _lobbyService = lobbyService;
            _navigationService = navigationService;
        }

        public HangmanViewModel GetHangmanViewModel()
        {
            if (_hangmanViewModel == null)
            {
                _hangmanViewModel = new HangmanViewModel(_hangmanService, _lobbyService, _navigationService);
            }
            return _hangmanViewModel;
        }

        // Optionally, you can add a method to clear the cached ViewModel if necessary
        public void ResetHangmanViewModel()
        {
            _hangmanViewModel = null;
        }
    }

}
