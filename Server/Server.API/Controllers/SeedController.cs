using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.API.Models;
using Server.API.Repository.Interfaces;

namespace Server.API.Controllers
{
    public class SeedController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<SeedController> _logger;
        private readonly IGameRepository _gameRepository;
        private readonly IUserRepository _userRepository;

        public SeedController(
            UserManager<User> userManager,
            ILogger<SeedController> logger,
            IGameRepository gameRepository,
            IUserRepository userRepository)
        {
            _userManager = userManager;
            _logger = logger;
            _gameRepository = gameRepository;
            _userRepository = userRepository;
        }

        [HttpPost("seed-data")]
        public async Task<IActionResult> SeedData()
        {
            _logger.LogDebug("Starting seeding process.");

            try
            {
                // Seed User
                var user = new User { UserName = "Hugo123", Email = "Hugo123@gmail.com" };
                var result = await _userRepository.AddUser(user, "Hugo123Hugo123");
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to seed user: {Errors}", result.Errors);
                    return BadRequest(result.Errors);
                }

                // Seed Game
                var game = new Game { Name = "hangman" };

                await _gameRepository.AddGame(game);

                await _gameRepository.AddGameToUser(user.Id, game.GameId);

                _logger.LogInformation("Seeding completed successfully.");
                return Ok("Seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during seeding.");
                return StatusCode(500, "An error occurred during seeding.");
            }
        }
    }
}
