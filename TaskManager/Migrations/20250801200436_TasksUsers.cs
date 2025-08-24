using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class TasksUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserOwnerId",
                table: "Tasks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserOwnerId",
                table: "Tasks",
                column: "UserOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_UserOwnerId",
                table: "Tasks",
                column: "UserOwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_AspNetUsers_UserOwnerId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserOwnerId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "UserOwnerId",
                table: "Tasks");
        }
    }
}
