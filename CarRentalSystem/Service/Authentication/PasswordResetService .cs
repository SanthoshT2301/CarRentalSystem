using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Register;
using CarRentalSystem.Models;
using CarRentalSystem.Service.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Authentication;

public class PasswordResetService : IPasswordResetService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PasswordResetService> _logger;
    private readonly IEmailService _emailService;

    private const int OtpExpiryMinutes = 10;

    public PasswordResetService(AppDbContext context, ILogger<PasswordResetService> logger, IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return new BadRequestObjectResult(new { error = "Email is required." });

        const string genericMessage = "If an account with that email exists, an OTP has been sent to it.";

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null)
            return new OkObjectResult(new { message = genericMessage });

        // Invalidate previous unused tokens
        var existingTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.UserId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var old in existingTokens)
            old.IsUsed = true;

        // Generate 6-digit OTP
        var otp = Random.Shared.Next(100000, 999999).ToString();

        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            Otp = otp,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        var htmlBody = $@"
            <div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:20px;border:1px solid #eee;border-radius:8px;'>
                <h2 style='color:#2563eb;'>RoadReady Password Reset</h2>
                <p>Hi {user.FirstName},</p>
                <p>Use the OTP below to reset your password. This code is valid for {OtpExpiryMinutes} minutes.</p>
                <div style='font-size:28px;font-weight:bold;letter-spacing:6px;background:#f3f4f6;padding:12px 20px;text-align:center;border-radius:6px;margin:16px 0;'>
                    {otp}
                </div>
                <p>If you did not request this, please ignore this email.</p>
                <p style='color:#888;font-size:12px;'>— The RoadReady Team</p>
            </div>";

        await _emailService.SendEmailAsync(user.Email, "RoadReady - Password Reset OTP", htmlBody);

        _logger.LogInformation("Password reset OTP generated for {Email}", user.Email);

        return new OkObjectResult(new { message = genericMessage });
    }

    public async Task<IActionResult> VerifyOtpAsync(VerifyOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
            return new BadRequestObjectResult(new { error = "Email and OTP are required." });

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (user == null)
            return new BadRequestObjectResult(new { error = "Invalid OTP." });

        var tokenRecord = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.UserId && t.Otp == request.Otp && !t.IsUsed)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (tokenRecord == null)
            return new BadRequestObjectResult(new { error = "Invalid OTP." });

        if (tokenRecord.ExpiresAt < DateTime.UtcNow)
            return new BadRequestObjectResult(new { error = "OTP has expired. Please request a new one." });

        return new OkObjectResult(new { message = "OTP verified successfully." });
    }

    public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
            return new BadRequestObjectResult(new { error = "Email and OTP are required." });

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return new BadRequestObjectResult(new { error = "New password is required." });

        if (request.NewPassword.Length < 8)
            return new BadRequestObjectResult(new { error = "Password must be at least 8 characters." });

        if (request.NewPassword != request.ConfirmPassword)
            return new BadRequestObjectResult(new { error = "Passwords do not match." });

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (user == null)
            return new BadRequestObjectResult(new { error = "Invalid OTP." });

        var tokenRecord = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.UserId && t.Otp == request.Otp && !t.IsUsed)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (tokenRecord == null)
            return new BadRequestObjectResult(new { error = "Invalid OTP." });

        if (tokenRecord.ExpiresAt < DateTime.UtcNow)
            return new BadRequestObjectResult(new { error = "OTP has expired. Please request a new one." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        tokenRecord.IsUsed = true;

        await _context.SaveChangesAsync();

        return new OkObjectResult(new { message = "Your password has been reset successfully. You can now log in." });
    }
}