using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Hubs;
using Server.API.Repositories.Interfaces;
using Server.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Test
{
    public class FriendsHubTests
    {
        private FriendsHub _uut;
        private IHubCallerClients _clients;
        private IGroupManager _groups;
        private HubCallerContext _context;

        private ILogger<FriendsHub> _logger;
        private IFriendsRepository _friendsRepository;
        private IClientProxy _clientProxy;
        private ISingleClientProxy _singleClientProxy;

        [SetUp]
        public void Setup()
        {

            _clients = Substitute.For<IHubCallerClients>();
            _groups = Substitute.For<IGroupManager>();
            _context = Substitute.For<HubCallerContext>();
            _clientProxy = Substitute.For<IClientProxy>();
            _singleClientProxy = Substitute.For<ISingleClientProxy>();

            _friendsRepository = Substitute.For<IFriendsRepository>();
            _logger = Substitute.For<ILogger<FriendsHub>>();

            _uut = new FriendsHub(_logger, _friendsRepository)
            {
                Clients = _clients,
                Groups = _groups,
                Context = _context
            };
        }

        [TearDown]
        public void TearDown()
        {
            _uut?.Dispose();
        }
    }
}
