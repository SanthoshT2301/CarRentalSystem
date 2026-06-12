using CarRentalSystem.DTO.Register;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Authentication;

public interface IPasswordResetService
{
    Task<IActionResult> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<IActionResult> VerifyOtpAsync(VerifyOtpRequest request);
    Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest request);
}