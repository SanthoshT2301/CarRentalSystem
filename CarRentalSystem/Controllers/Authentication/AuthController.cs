using CarRentalSystem.DTO.Authentication;
using CarRentalSystem.DTO.Register;
using CarRentalSystem.Service.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Authentication;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous] // Login and register must always be publicly accessible
public class AuthenticationController : ControllerBase
{
    private readonly IAuthentication _authService;

    public AuthenticationController(IAuthentication authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.Register(request);
        return result is OkObjectResult ? result : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
        => await _authService.Login(request);
}