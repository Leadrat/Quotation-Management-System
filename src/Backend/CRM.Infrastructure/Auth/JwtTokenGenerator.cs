using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CRM.Application.Auth.Services;
using CRM.Domain.Entities;
using CRM.Shared.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CRM.Infrastructure.Auth;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;
    private readonly ILogger<JwtTokenGenerator> _logger;

    public JwtTokenGenerator(IConfiguration configuration, ILogger<JwtTokenGenerator> logger)
    {
        _logger = logger;
        _settings = new JwtSettings();
        configuration.GetSection("Jwt").Bind(_settings);
    }

    public string GenerateAccessToken(User user, DateTime nowUtc, string? refreshJti = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var handler = new JwtSecurityTokenHandler();
        var claimsList = new System.Collections.Generic.List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
            new Claim("roleId", user.RoleId.ToString()), // legacy
            new Claim("role_id", user.RoleId.ToString()),
            new Claim("role", CRM.Shared.Constants.RoleConstants.GetName(user.RoleId)),
            new Claim("iss", _settings.Issuer),
            new Claim("aud", _settings.Audience)
        };
        if (!string.IsNullOrWhiteSpace(refreshJti))
        {
            claimsList.Add(new Claim("rt_jti", refreshJti));
        }
        var token = new JwtSecurityToken(
            claims: claimsList,
            notBefore: nowUtc,
            expires: nowUtc.AddSeconds(_settings.AccessTokenExpiration),
            signingCredentials: creds
        );
        return handler.WriteToken(token);
    }

    public (string token, string jti, DateTime expiresAtUtc) GenerateRefreshToken(Guid userId, DateTime nowUtc)
    {
        var jti = Guid.NewGuid().ToString();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var handler = new JwtSecurityTokenHandler();
        var expires = nowUtc.AddSeconds(_settings.RefreshTokenExpiration);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim("iss", _settings.Issuer),
            new Claim("aud", _settings.Audience)
        };
        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: nowUtc,
            expires: expires,
            signingCredentials: creds
        );
        return (handler.WriteToken(token), jti, expires);
    }

    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
    {
        var handler = new JwtSecurityTokenHandler();
        var parameters = BuildValidationParameters(validateLifetime, allowPrevious: true);
        try
        {
            return handler.ValidateToken(token, parameters, out _);
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Token validation failed");
            return null;
        }
    }

    public Guid GetUserIdFromToken(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    public string? GetJtiFromToken(ClaimsPrincipal principal)
    {
        return principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
    }

    private TokenValidationParameters BuildValidationParameters(bool validateLifetime, bool allowPrevious)
    {
        var currentKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = currentKey,
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
        if (allowPrevious && !string.IsNullOrWhiteSpace(_settings.PreviousSecret))
        {
            parameters.IssuerSigningKeys = new[]
            {
                currentKey,
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.PreviousSecret!))
            };
        }
        return parameters;
    }
}
