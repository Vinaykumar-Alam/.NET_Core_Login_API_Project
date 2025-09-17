using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flutter_Backed.Migrations
{
    /// <inheritdoc />
    public partial class AddedContraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Tbl_Test_UserLogin",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Tbl_Test_UserLogin",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Test_UserLogin_Email",
                table: "Tbl_Test_UserLogin",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Test_UserLogin_UserName",
                table: "Tbl_Test_UserLogin",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tbl_Test_UserLogin_Email",
                table: "Tbl_Test_UserLogin");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Test_UserLogin_UserName",
                table: "Tbl_Test_UserLogin");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Tbl_Test_UserLogin",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Tbl_Test_UserLogin",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
