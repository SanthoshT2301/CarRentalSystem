namespace CarRentalSystem.DTO.Register;

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }

    /// <summary>
    /// Optional. 1 = Admin, 2 = Customer (default), 3 = Agent.
    /// Admin and Agent registrations require admin approval before login is permitted.
    /// </summary>
    public int? RoleId { get; set; }
}