namespace CarRentalSystem.DTO.Admin;

public class AdminUpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? RoleId { get; set; }
}