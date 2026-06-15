using BackerUp.Admin.Server.Data;
using BackerUp.Admin.Server.Models.DTOs;
using BackerUp.Admin.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BackerUp.Admin.Server.Services;

public class AuthService
{
    private const int AccessTokenMinutes = 15;
    private const int RefreshTokenDays = 7;

    private readonly BackerUpDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ProblemLogService _problemLogService;

    public AuthService(BackerUpDbContext db, IConfiguration configuration, ProblemLogService problemLogService)
    {
        _db = db;
        _configuration = configuration;
        _problemLogService = problemLogService;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
        {
            _problemLogService.LogWarning($"Auth.Login invalid username '{request.Username}'.");
            return null;
        }

        var passwordHash = HashPassword(request.Password);
        if (!string.Equals(user.Password, passwordHash, StringComparison.OrdinalIgnoreCase))
        {
            _problemLogService.LogWarning($"Auth.Login invalid password for '{request.Username}'.");
            return null;
        }

        var response = await IssueTokensAsync(user, revokeExistingRefreshTokens: true);
        return response;
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var token = await _db.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (token == null)
        {
            _problemLogService.LogWarning("Auth.Refresh invalid refresh token.");
            return null;
        }

        if (token.RevokedAt.HasValue || token.ExpiresAt <= DateTime.UtcNow)
        {
            _problemLogService.LogWarning($"Auth.Refresh expired or revoked token for user {token.UserId}.");
            return null;
        }

        token.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await IssueTokensAsync(token.User, revokeExistingRefreshTokens: false);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (token == null)
        {
            _problemLogService.LogWarning("Auth.Logout invalid refresh token.");
            return false;
        }

        token.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, bool revokeExistingRefreshTokens)
    {
        if (revokeExistingRefreshTokens)
        {
            var existingTokens = await _db.RefreshTokens.Where(t => t.UserId == user.Id && t.RevokedAt == null).ToListAsync();
            foreach (var existingToken in existingTokens)
            {
                existingToken.RevokedAt = DateTime.UtcNow;
            }
        }

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(AccessTokenMinutes);
        var refreshToken = GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays)
        });

        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = CreateAccessToken(user, accessTokenExpiresAt),
            RefreshToken = refreshToken,
            ExpiresAt = accessTokenExpiresAt,
            UserId = user.Id,
            Username = user.Username
        };
    }

    private string CreateAccessToken(User user, DateTime expiresAt)
    {
        var key = GetSigningKey();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: GetIssuer(),
            audience: GetAudience(),
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private SymmetricSecurityKey GetSigningKey()
    {
        var keyValue = _configuration["Jwt:Key"] ?? "BackerUp-Development-Key-Change-Me-Please-1234567890";
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
    }

    private string GetIssuer() => _configuration["Jwt:Issuer"] ?? "BackerUp.Admin.Server";

    private string GetAudience() => _configuration["Jwt:Audience"] ?? "BackerUp.Admin.Frontend";

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private static string HashPassword(string password) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();
}