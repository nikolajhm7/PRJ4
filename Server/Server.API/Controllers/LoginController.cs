using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Server.API.DTO;
using Server.API.Models;
using Server.API.Services;
using System.Text.RegularExpressions;
using Server.API.Repository.Interfaces;
using Server.API.Services.Interfaces;

namespace Server.API.Controllers;
public class LoginController : ControllerBase
{

    private readonly ILogger<LoginController> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IJwtTokenService _jwtTokenService;
    private IUserRepository _userRepository;

    public LoginController(ILogger<LoginController> logger, IMemoryCache memoryCache, IJwtTokenService jwtTokenService, IUserRepository userRepository)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var retryLimit = 5;
        var lockoutTime = TimeSpan.FromMinutes(15);
        var cacheKey = $"login_attempts_for_{loginDto.UserName}";

        if (_memoryCache.TryGetValue(cacheKey, out int attempts))
        {
            if (attempts >= retryLimit)
            {
                _logger.LogWarning("Too many login attempts for user: {UserName}", loginDto.UserName);
                return StatusCode(StatusCodes.Status429TooManyRequests, "Too many login attempts. Please try again later.");
            }
        }

        var user = await _userRepository.GetUserByName(loginDto.UserName);

        if (user == null)
        {
            _memoryCache.Set(cacheKey, attempts + 1, lockoutTime);
            _logger.LogWarning("Login attempt with unknown username: {UserName}", loginDto.UserName);
            return Unauthorized("Wrong username or password");
        }

        if (!await _userRepository.UserCheckPassword(user, loginDto.Password))
        {
            _memoryCache.Set(cacheKey, attempts + 1, lockoutTime);
            _logger.LogWarning("Incorrect password attempt for user: {UserName}", loginDto.UserName);
            return Unauthorized("Wrong username or password");
        }

        var token = _jwtTokenService.GenerateToken(user.UserName);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.UserName);

        _logger.LogInformation("User {UserName} logged in successfully.", user.UserName);

        _memoryCache.Remove(cacheKey);

        return StatusCode(StatusCodes.Status200OK, new { Token = token, RefreshToken = refreshToken });

    }

    [HttpPost("login-as-guest")]
    public IActionResult LoginAsGuest([FromBody] GuestLoginDTO model)
    {
        if (string.IsNullOrWhiteSpace(model.GuestName))
        {
            _logger.LogWarning("Guest login attempt without a name.");
            return BadRequest("Guest name is required.");
        }

        string pattern = @"^[A-Za-z0-9]{2,20}$";

        if (!Regex.IsMatch(model.GuestName, pattern))
        {
            _logger.LogWarning("Guest login attempt with illegal name.");
            return BadRequest("Guest name must be 2-20 characters long, and only contain letters and numbers.");
        }

        _logger.LogDebug("Guest {GuestName} is attempting to log in.", model.GuestName);

        var token = _jwtTokenService.GenerateToken(model.GuestName, true);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(model.GuestName);

        _logger.LogInformation("Guest {GuestName} logged in successfully.", model.GuestName);
        return StatusCode(StatusCodes.Status200OK, new { Token = token, RefreshToken = refreshToken });
    }

    [Authorize]
    [HttpPost("checkLoginToken")]
    public IActionResult CheckLoginToken()
    {
        Console.WriteLine("Token is valid");
        return Ok("Token is valid");
    }

    [Authorize(Policy = "Guest+")]
    [HttpPost("checkGuestToken")]
    public IActionResult CheckGuestToken()
    {
        Console.WriteLine("Token is valid");
        return Ok("Token is valid");
    }
}
