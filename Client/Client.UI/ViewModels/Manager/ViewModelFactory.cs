using Client.Library.Games;
using Client.Library.Services.Interfaces;
using Client.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.UI.Games;
using Microsoft.Extensions.Logging;

namespace Client.UI.ViewModels.Manager
{
    public class ViewModelFactory
    {
        private readonly IHangmanService _hangmanService;
        private readonly ILobbyService _lobbyService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<LobbyViewModel> _logger;
        private HangmanViewModel _hangmanViewModel;
        private LobbyViewModel _lobbyViewModel;

        public ViewModelFactory(IHangmanService hangmanService, ILobbyService lobbyService, INavigationService navigationService, ILogger<LobbyViewModel> logger)
        {
            _hangmanService = hangmanService;
            _lobbyService = lobbyService;
            _navigationService = navigationService;
            _logger = logger;
        }

        public HangmanViewModel GetHangmanViewModel()
        {
            if (_hangmanViewModel == null)
            {
                _hangmanViewModel = new HangmanViewModel(_hangmanService, _lobbyService, _navigationService);
            }
            return _hangmanViewModel;
        }

        public LobbyViewModel GetLobbyViewModel()
        {
            if (_lobbyViewModel == null)
            {
                _lobbyViewModel = new LobbyViewModel(_lobbyService, _navigationService, this, _hangmanService, _logger);
            }
            return _lobbyViewModel;
        }

        public void ResetLobbyViewModel()
        {
            _lobbyViewModel = null;
        }

        // Optionally, you can add a method to clear the cached ViewModel if necessary
        public void ResetHangmanViewModel()
        {
            _hangmanViewModel = null;
        }

        public void ResetAllViewModels()
        {
            ResetLobbyViewModel();
            ResetHangmanViewModel();
        }
    }

}
