namespace GamifyApi.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GamifyApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class AuthService
{
    private readonly GamifyDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(GamifyDbContext context, IConfiguration config)
    {
        _config = config;
        _context = context;
    }

    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET_KEY") ?? throw new ArgumentNullException("SECRET_KEY not configured")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier,user.FullName ?? ""),
            new Claim(ClaimTypes.Email,user.Email ?? ""),
            new Claim(ClaimTypes.Role,user.Role)
        };

        if (!int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_DAYS"), out var expiryDays))
        {
            expiryDays = 30;
        }

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expiryDays),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<User> Authenticate(string email, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;
                
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }
        catch (System.Exception)
        {

            return null;
        }
    }
}