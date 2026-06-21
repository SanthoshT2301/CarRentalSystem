using CarRentalSystem.DATA;
using CarRentalSystem.DTO.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Controllers.v1;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    public UsersController(AppDbContext context) => _context = context;

  
    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetProfile(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null) return NotFound(new { error = "User not found." });

        return Ok(new UserProfileDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role?.RoleName ?? "Customer",
            CreatedAt = user.CreatedAt
        });
    }
}