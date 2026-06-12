using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Register;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using CarRentalSystem.DTO.Authentication;
using CarRentalSystem.Service.Email;

namespace CarRentalSystem.Service.Authentication;

public class AuthenticationService : IAuthentication
{
    private readonly AppDbContext _appDbContext;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;

    public AuthenticationService(AppDbContext appDbContext, IConfiguration config, IEmailService emailService)
    {
        this._appDbContext = appDbContext;
        _config = config;
        _emailService = emailService;
    }

    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _appDbContext.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new BadRequestObjectResult(new { error = "Invalid credentials." });
        }
        var token = GenerateJwtToken(user);
        return new OkObjectResult(new AuthResponseDto
        {
            Token = token,
            Role = user.Role.RoleName,
            UserId = user.UserId.ToString(),
            UserName = user.FirstName + " " + user.LastName
        });
    }
    private string GenerateJwtToken(User user)
    {
        var jwtSetting = _config.GetSection("JwtSettings");
        var Secret = jwtSetting.GetValue<string>("Secret");
        var issuer = jwtSetting.GetValue<string>("Issuer");
        var audience = jwtSetting.GetValue<string>("Audience");
        var keys = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(keys, SecurityAlgorithms.HmacSha256);
        var Claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserId.ToString()),
                new Claim(ClaimTypes.Name,user.FirstName+""+user.LastName),
                new Claim(ClaimTypes.Role,user.Role.RoleName)

            };
        var token = new JwtSecurityToken(
            issuer,
            audience,
            Claims,
            expires: DateTime.UtcNow.AddHours(5),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new BadRequestObjectResult(new { error = "Email and password are required." });
        }

        string emailLower = request.Email.ToLowerInvariant();
        var exists = await _appDbContext.Users.AnyAsync(u => u.Email.ToLower() == emailLower);
        if (exists)
        {
            return new BadRequestObjectResult(new { error = "User already exists with this email." });
        }
        var newUser = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _appDbContext.Users.Add(newUser);
        await _appDbContext.SaveChangesAsync();


        newUser.Role = await _appDbContext.Roles.FindAsync(newUser.RoleId);

        var token = GenerateJwtToken(newUser);

        // Send welcome email (fire-and-forget style, errors logged internally and won't break registration)
        var welcomeHtml = $@"
                <div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:20px;border:1px solid #eee;border-radius:8px;'>
                    <h2 style='color:#2563eb;'>Welcome to RoadReady, {newUser.FirstName}!</h2>
                    <p>Thanks for creating an account with us. You're all set to start booking your next ride.</p>
                    <p>Here's a quick summary of your account:</p>
                    <ul>
                        <li><strong>Name:</strong> {newUser.FirstName} {newUser.LastName}</li>
                        <li><strong>Email:</strong> {newUser.Email}</li>
                    </ul>
                    <p>Happy driving!</p>
                    <p style='color:#888;font-size:12px;'>— The RoadReady Team</p>
                </div>";

        await _emailService.SendEmailAsync(newUser.Email, "Welcome to RoadReady!", welcomeHtml);

        return new OkObjectResult(new AuthResponseDto
        {
            Token = token,
            Role = newUser.Role.RoleName,
            UserId = newUser.UserId.ToString(),
            UserName = newUser.FirstName + " " + newUser.LastName
        });
    }

}