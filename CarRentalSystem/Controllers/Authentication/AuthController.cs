using CarRentalSystem.DTO.Authentication;
using CarRentalSystem.DTO.Register;
using CarRentalSystem.Service.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Authentication;

[Route("api/[controller]/[action]")]
[ApiController]

public class AuthenticationController : ControllerBase
{
    private readonly IAuthentication _authenticationService;

    public AuthenticationController(IAuthentication authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var res= await _authenticationService.Register(request);
        if(res is OkObjectResult)
        {
            return Ok(res);
        }
        return BadRequest();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        return await _authenticationService.Login(request);
    }
}