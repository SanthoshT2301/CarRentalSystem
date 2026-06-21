namespace CarRentalSystem.DTO.User;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}