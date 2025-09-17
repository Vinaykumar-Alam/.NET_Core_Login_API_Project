using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flutter_Backed.Migrations
{
    /// <inheritdoc />
    public partial class AddedRoleforUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Tbl_Test_UserLogin",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "USER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Tbl_Test_UserLogin");
        }
    }
}
