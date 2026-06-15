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
        _appDbContext = appDbContext;
        _config = config;
        _emailService = emailService;
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _appDbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new BadRequestObjectResult(new { error = "Invalid credentials." });

        // Block inactive accounts
        if (user.IsActive == false)
            return new BadRequestObjectResult(new { error = "Your account has been deactivated. Please contact support." });

        // Block Admin/Agent accounts that are not yet approved
        if (!user.IsApproved)
            return new UnauthorizedObjectResult(new
            {
                error = "Your account is pending admin approval. You will be able to log in once an administrator activates your account."
            });

        var token = GenerateJwtToken(user);
        return new OkObjectResult(new AuthResponseDto
        {
            Token = token,
            Role = user.Role!.RoleName,
            UserId = user.UserId.ToString(),
            UserName = $"{user.FirstName} {user.LastName}"
        });
    }

    // ── Register ──────────────────────────────────────────────────────────────
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return new BadRequestObjectResult(new { error = "Email and password are required." });

        string emailLower = request.Email.ToLowerInvariant();
        var exists = await _appDbContext.Users.AnyAsync(u => u.Email.ToLower() == emailLower);
        if (exists)
            return new BadRequestObjectResult(new { error = "User already exists with this email." });

        // Resolve the requested role (default Customer = 2 if not specified or invalid)
        int requestedRoleId = request.RoleId ?? 2;

        // Only allow roles 1 (Admin), 2 (Customer), 3 (Agent)
        var roleExists = await _appDbContext.Roles.AnyAsync(r => r.RoleId == requestedRoleId);
        if (!roleExists) requestedRoleId = 2;

        // Customers are auto-approved; Admin/Agent accounts must wait for approval
        bool autoApproved = requestedRoleId == 2; // Customer

        var newUser = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = requestedRoleId,
            IsActive = true,
            IsApproved = autoApproved,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _appDbContext.Users.Add(newUser);
        await _appDbContext.SaveChangesAsync();

        newUser.Role = await _appDbContext.Roles.FindAsync(newUser.RoleId);

        // ── Send email ──────────────────────────────────────────────────────
        if (autoApproved)
        {
            // Welcome email for customers
            var welcomeHtml = $@"
                <div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:20px;border:1px solid #eee;border-radius:8px;'>
                    <h2 style='color:#2563eb;'>Welcome to RoadReady, {newUser.FirstName}!</h2>
                    <p>Thanks for creating an account with us. You're all set to start booking your next ride.</p>
                    <ul>
                        <li><strong>Name:</strong> {newUser.FirstName} {newUser.LastName}</li>
                        <li><strong>Email:</strong> {newUser.Email}</li>
                    </ul>
                    <p>Happy driving!</p>
                    <p style='color:#888;font-size:12px;'>— The RoadReady Team</p>
                </div>";

            await _emailService.SendEmailAsync(newUser.Email, "Welcome to RoadReady!", welcomeHtml);

            var token = GenerateJwtToken(newUser);
            return new OkObjectResult(new AuthResponseDto
            {
                Token = token,
                Role = newUser.Role!.RoleName,
                UserId = newUser.UserId.ToString(),
                UserName = $"{newUser.FirstName} {newUser.LastName}"
            });
        }
        else
        {
            // Pending-approval email for Admin/Agent
            var roleName = newUser.Role?.RoleName ?? "Staff";
            var pendingHtml = $@"
                <div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:20px;border:1px solid #eee;border-radius:8px;'>
                    <h2 style='color:#d97706;'>Account Pending Approval — RoadReady</h2>
                    <p>Hi {newUser.FirstName},</p>
                    <p>Your <strong>{roleName}</strong> account has been created and is now <strong>awaiting approval</strong> from an administrator.</p>
                    <p>You will receive another email once your account has been reviewed. Until then you will not be able to log in.</p>
                    <ul>
                        <li><strong>Email:</strong> {newUser.Email}</li>
                        <li><strong>Role requested:</strong> {roleName}</li>
                    </ul>
                    <p style='color:#888;font-size:12px;'>— The RoadReady Team</p>
                </div>";

            await _emailService.SendEmailAsync(newUser.Email, "RoadReady — Your account is pending approval", pendingHtml);

            // Return 202 Accepted (not a token — the account is not active yet)
            return new ObjectResult(new
            {
                message = $"Your {roleName} account has been created and is pending administrator approval. You will be notified by email once approved.",
                pendingApproval = true,
                email = newUser.Email,
                role = roleName
            })
            { StatusCode = 202 };
        }
    }

    // ── JWT helper ────────────────────────────────────────────────────────────
    private string GenerateJwtToken(User user)
    {
        var jwtSetting = _config.GetSection("JwtSettings");
        var secret = jwtSetting.GetValue<string>("Secret")!;
        var issuer = jwtSetting.GetValue<string>("Issuer");
        var audience = jwtSetting.GetValue<string>("Audience");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role!.RoleName)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddHours(5),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}