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

namespace CarRentalSystem.Service.Authentication;

public class AuthenticationService : IAuthentication
{
    private readonly AppDbContext _appDbContext;
    private readonly IConfiguration _config;
    public AuthenticationService(AppDbContext appDbContext,  IConfiguration config)
    {
        this._appDbContext = appDbContext;
        _config=config;
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
        var token=GenerateJwtToken(user);
        return new OkObjectResult(new AuthResponseDto
        {
        Token=token,
        Role=user.Role.RoleName,
        UserId=user.UserId.ToString(),
        UserName=user.FirstName+" "+user.LastName
        });
    }
        private string GenerateJwtToken(User user)
        {
            var jwtSetting=_config.GetSection("JwtSettings");
            var Secret=jwtSetting.GetValue<string>("Secret");
            var issuer=jwtSetting.GetValue<string>("Issuer");
            var audience=jwtSetting.GetValue<string>("Audience");
           var keys = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
            var creds=new SigningCredentials(keys,SecurityAlgorithms.HmacSha256);
            var Claims=new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserId.ToString()),
                new Claim(ClaimTypes.Name,user.FirstName+""+user.LastName),
                new Claim(ClaimTypes.Role,user.Role.RoleName)
                
            };
            var token=new JwtSecurityToken(
                issuer,
                audience,
                Claims,
                expires:DateTime.UtcNow.AddHours(5),
                signingCredentials:creds
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
            return new OkObjectResult(new AuthResponseDto
            {
                Token = token,
                Role = newUser.Role.RoleName,
                UserId = newUser.UserId.ToString(),
                UserName = newUser.FirstName + " " + newUser.LastName
            });
        }

}
