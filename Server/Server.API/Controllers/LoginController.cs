using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Server.API.DTO;
using Server.API.Models;

public class LoginController : ControllerBase
{
    private readonly string? _jwtKey;
    private readonly string? _jwtIssuer;
    private readonly string? _jwtAudience;
    private readonly ILogger<LoginController> _logger;

    private readonly UserManager<User> _userManager;
    
    private readonly IMemoryCache _memoryCache;

    public LoginController(IConfiguration configuration, UserManager<User> userManager, ILogger<LoginController> logger, IMemoryCache memoryCache)
    {
        _jwtKey = configuration["Jwt:Key"];
        _jwtIssuer = configuration["Jwt:Issuer"];
        _jwtAudience = configuration["Jwt:Audience"];
        _userManager = userManager;
        _logger = logger;
        _memoryCache = memoryCache;
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

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_jwtKey)),
            SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        var jwtObject = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.Now.AddSeconds(300),
            signingCredentials: signingCredentials);
        var jwtString = new JwtSecurityTokenHandler()
            .WriteToken(jwtObject);
        
        _logger.LogInformation("User {UserName} logged in successfully.", user.UserName);
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
