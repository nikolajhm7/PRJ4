using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Server.API.Repository;
using Server.API.Repository.Interfaces;
using Server.API.Services.Interfaces;

namespace Server.API.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ITokenRepository _tokenRepository;
    private readonly ITimeService _timeService;

    public JwtTokenService(IConfiguration configuration, ITokenRepository tokenRepository, ITimeService timeService)
    {
        _configuration = configuration;
        _tokenRepository = tokenRepository;
        _timeService = timeService;
    }

    public bool IsGuest(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        return securityToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value == "Guest";
    }
    
    public string GetUserNameFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        return securityToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
    }

    public string GenerateToken(string userName, bool isGuest = false)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, isGuest ? "Guest" : "User")
            // Tilføj yderligere claims her efter behov
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: _timeService.UtcNow.AddMinutes(30),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken(string userName)
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);
            _tokenRepository.SaveRefreshToken(userName, refreshToken, DateTime.UtcNow.AddDays(7));
            return refreshToken;
        }
    }

    public bool ValidateRefreshToken(string userName, string refreshToken)
    {
        return _tokenRepository.GetRefreshToken(userName) == refreshToken && _tokenRepository.IsActive(userName);
    }
    
}