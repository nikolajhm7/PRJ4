using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Server.API.DTO;
using Server.API.Models;
using Server.API.Services;
using System.Text.RegularExpressions;

public class LoginController : ControllerBase
{

    private readonly ILogger<LoginController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMemoryCache _memoryCache;
    private readonly JwtTokenService _jwtTokenService;

    public LoginController(UserManager<User> userManager, ILogger<LoginController> logger, IMemoryCache memoryCache, JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _logger = logger;
        _memoryCache = memoryCache;
        _jwtTokenService = jwtTokenService;
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
        
        var user = await _userManager.FindByNameAsync(loginDto.UserName);

        if (user == null)
        {
            _logger.LogWarning("Login attempt with unknown username: {UserName}", loginDto.UserName);
            return Unauthorized("Wrong username or password");
        }

        if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            _logger.LogWarning("Incorrect password attempt for user: {UserName}", user.UserName);
            return Unauthorized("Wrong username or password");
        }
        
        var jwtString = _jwtTokenService.GenerateToken(user.UserName);
        
        _logger.LogInformation("User {UserName} logged in successfully.", user.UserName);
        return StatusCode(StatusCodes.Status200OK, jwtString);
    }

    [AllowAnonymous]
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

        _logger.LogInformation("Guest {GuestName} is attempting to log in.", model.GuestName);

        var jwtString = _jwtTokenService.GenerateToken(model.GuestName, true);

        _logger.LogInformation("Guest {GuestName} logged in successfully.", model.GuestName);
        return StatusCode(StatusCodes.Status200OK, jwtString);
    }

    [Authorize]
    [HttpPost("checkLoginToken")]
    public IActionResult CheckLoginToken()
    {
        Console.WriteLine("Token is valid");
        return Ok("Token is valid");
    }
}
