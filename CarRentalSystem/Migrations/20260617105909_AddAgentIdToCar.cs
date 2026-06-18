using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentIdToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgentId",
                table: "Cars",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cars_AgentId",
                table: "Cars",
                column: "AgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cars_Users_AgentId",
                table: "Cars",
                column: "AgentId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cars_Users_AgentId",
                table: "Cars");

            migrationBuilder.DropIndex(
                name: "IX_Cars_AgentId",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "Cars");
        }
    }
}