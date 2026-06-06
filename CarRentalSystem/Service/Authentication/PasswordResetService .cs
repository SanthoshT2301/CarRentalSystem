using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Register;
using CarRentalSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Authentication;

public class PasswordResetService : IPasswordResetService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PasswordResetService> _logger;

    private const int TokenExpiryMinutes = 30;

    public PasswordResetService(AppDbContext context, ILogger<PasswordResetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return new BadRequestObjectResult(new { error = "Email is required." });

     
        const string genericMessage = "If an account with that email exists, a password reset link has been sent.";

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null)
            return new OkObjectResult(new { message = genericMessage });

     
        var existingTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.UserId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var old in existingTokens)
            old.IsUsed = true;

     
        var rawToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

        var urlSafeToken = rawToken.Replace("+", "-").Replace("/", "_").TrimEnd('=');

        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            Token = urlSafeToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(TokenExpiryMinutes),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();


        _logger.LogWarning(
            "Password reset token for {Email}: {Token} (expires {Expiry})",
            user.Email, urlSafeToken, resetToken.ExpiresAt);

        return new OkObjectResult(new { message = genericMessage });
    }

    public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return new BadRequestObjectResult(new { error = "Token is required." });

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return new BadRequestObjectResult(new { error = "New password is required." });

        if (request.NewPassword.Length < 8)
            return new BadRequestObjectResult(new { error = "Password must be at least 8 characters." });

        if (request.NewPassword != request.ConfirmPassword)
            return new BadRequestObjectResult(new { error = "Passwords do not match." });

        var tokenRecord = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token);

        if (tokenRecord == null)
            return new BadRequestObjectResult(new { error = "Invalid or expired reset token." });

        if (tokenRecord.IsUsed)
            return new BadRequestObjectResult(new { error = "This reset token has already been used." });

        if (tokenRecord.ExpiresAt < DateTime.UtcNow)
            return new BadRequestObjectResult(new { error = "This reset token has expired. Please request a new one." });

        var user = tokenRecord.User;
        if (user == null)
            return new BadRequestObjectResult(new { error = "Invalid or expired reset token." });
      user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

       
        tokenRecord.IsUsed = true;

        await _context.SaveChangesAsync();

        return new OkObjectResult(new { message = "Your password has been reset successfully. You can now log in." });
    }
}