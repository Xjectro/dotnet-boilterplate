using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncMemberModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "members",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "members",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "members",
                newName: "email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "username",
                table: "members",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "members",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "members",
                newName: "Email");
        }
    }
}
