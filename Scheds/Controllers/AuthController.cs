using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Scheds.MVC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("google")]
    public async Task<IActionResult> Google([FromBody] GoogleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.IdToken))
            return BadRequest(new { error = "IdToken is required" });

        var clientId = _configuration["Authentication:Google:ClientId"];
        if (string.IsNullOrEmpty(clientId))
            return StatusCode(500, new { error = "Google ClientId not configured" });

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });

            var name = payload.Name ?? payload.Email ?? "";
            var email = payload.Email ?? "";
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email not found in token" });

            var accessToken = GenerateJwt(name, email);
            var refreshToken = GenerateRefreshToken(name, email);
            var expiresIn = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60) * 60;
            var refreshExpirationDays = _configuration.GetValue<int>("Jwt:RefreshExpirationDays", 7);
            var refreshExpiresIn = refreshExpirationDays * 24 * 60 * 60;

            return Ok(new { accessToken, expiresIn, refreshToken, refreshExpiresIn });
        }
        catch (InvalidJwtException)
        {
            return BadRequest(new { error = "Invalid token" });
        }
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult Me()
    {
        if (User?.Identity?.IsAuthenticated != true)
            return Unauthorized();
        return Ok(new { name = User.Identity!.Name });
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.RefreshToken))
            return Unauthorized();

        var (name, email) = ValidateRefreshToken(request.RefreshToken);
        if (name == null || email == null)
            return Unauthorized();

        var accessToken = GenerateJwt(name, email);
        var expiresIn = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60) * 60;
        return Ok(new { accessToken, expiresIn });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok();
    }

    private (string? name, string? email) ValidateRefreshToken(string refreshToken)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key)) return (null, null);
        var issuer = _configuration["Jwt:Issuer"] ?? "scheds";
        var audience = _configuration["Jwt:Audience"] ?? "scheds-frontend";
        var keyBytes = Convert.FromBase64String(key);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validationParameters, out _);
            if (principal.FindFirst("type")?.Value != "refresh")
                return (null, null);
            var name = principal.FindFirst(ClaimTypes.Name)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            return (name, email);
        }
        catch
        {
            return (null, null);
        }
    }

    private string GenerateJwt(string name, string email)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException("Jwt:Key is not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "scheds";
        var audience = _configuration["Jwt:Audience"] ?? "scheds-frontend";
        var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);

        var keyBytes = Convert.FromBase64String(key);
        var signingKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email),
            new Claim("sub", email),
            new Claim("jti", Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(string name, string email)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException("Jwt:Key is not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "scheds";
        var audience = _configuration["Jwt:Audience"] ?? "scheds-frontend";
        var expirationDays = _configuration.GetValue<int>("Jwt:RefreshExpirationDays", 7);

        var keyBytes = Convert.FromBase64String(key);
        var signingKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email),
            new Claim("sub", email),
            new Claim("jti", Guid.NewGuid().ToString()),
            new Claim("type", "refresh")
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddDays(expirationDays),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public class GoogleRequest
    {
        public string? IdToken { get; set; }
    }

    public class RefreshRequest
    {
        public string? RefreshToken { get; set; }
    }
}
