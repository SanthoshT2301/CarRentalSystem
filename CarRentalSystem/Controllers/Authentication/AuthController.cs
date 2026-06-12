// ── Authentication ────────────────────────────────────────────────────────────
using CarRentalSystem.DTO.Register;
using CarRentalSystem.Service.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers.v1;

[Route("api/v1/authentication")]
[ApiController]
[AllowAnonymous]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthentication _authService;
    private readonly IPasswordResetService _passwordResetService;
    public AuthenticationController(IAuthentication authService, IPasswordResetService passwordResetService)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.Register(request);
        return result is OkObjectResult ? result : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
        => await _authService.Login(request);

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    => await _passwordResetService.ForgotPasswordAsync(request);

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        => await _passwordResetService.VerifyOtpAsync(request);

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        => await _passwordResetService.ResetPasswordAsync(request);
}