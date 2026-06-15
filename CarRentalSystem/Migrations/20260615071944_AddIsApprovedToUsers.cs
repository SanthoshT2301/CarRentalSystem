using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add IsApproved column — default false so existing rows start unapproved.
            // We then immediately set existing Admin (RoleId=1) and the seeded users to approved
            // so production data isn't broken.
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Approve all existing users (seeded data + any previously created accounts)
            // so the migration doesn't lock everyone out on an existing database.
            migrationBuilder.Sql("UPDATE [Users] SET [IsApproved] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Users");
        }
    }
}