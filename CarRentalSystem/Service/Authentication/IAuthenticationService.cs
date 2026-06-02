using CarRentalSystem.DTO.Register;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Authentication;
public interface IAuthentication
{
    Task<IActionResult> Login(LoginRequest request);
     Task<IActionResult> Register(RegisterRequest request);
}