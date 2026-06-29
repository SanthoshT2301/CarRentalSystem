namespace CarRentalSystem.DTO.Admin;

public class AdminCreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int RoleId { get; set; } = 2;
}