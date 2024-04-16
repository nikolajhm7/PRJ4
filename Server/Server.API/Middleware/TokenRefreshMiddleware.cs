using System.IdentityModel.Tokens.Jwt;
using Server.API.Services;
using Server.API.Services.Interfaces;


namespace Server.API.Middleware;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IJwtTokenService _jwtTokenService;

    public TokenRefreshMiddleware(RequestDelegate next, IJwtTokenService jwtTokenService)
    {
        _next = next;
        _jwtTokenService = jwtTokenService;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var refreshToken = context.Request.Headers["X-Refresh-Token"].FirstOrDefault();
        if (token != null && refreshToken != null)
        {
            var userName = _jwtTokenService.GetUserNameFromToken(token);
            if (userName != null && _jwtTokenService.ValidateRefreshToken(userName, refreshToken) &&
                JwtTokenIsExpiringSoon(token))
            {
                var isGuest = _jwtTokenService.IsGuest(token);
                var newAccessToken = _jwtTokenService.GenerateToken(userName, isGuest);
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken(userName);
                context.Response.Headers.Add("X-New-AccessToken", newAccessToken);
                context.Response.Headers.Add("X-New-RefreshToken", newRefreshToken);
            }
        }

        await _next(context);
    }

    private bool JwtTokenIsExpiringSoon(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        var timeToExpire = securityToken?.ValidTo - DateTime.UtcNow;
        return timeToExpire?.TotalMinutes < 15; // For example, refresh if less than 30 minutes to expire
    }
}
