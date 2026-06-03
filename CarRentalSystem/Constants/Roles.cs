namespace CarRentalSystem.Constants;

/// <summary>
/// Centralised role name constants — use these instead of raw strings in [Authorize] attributes
/// to prevent typos and make refactoring easier.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Agent = "Agent";
    public const string Customer = "Customer";
}