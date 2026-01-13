using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waads.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNavigationToFollowUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "FollowUps",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowUps_UserId",
                table: "FollowUps",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FollowUps_AspNetUsers_UserId",
                table: "FollowUps",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FollowUps_AspNetUsers_UserId",
                table: "FollowUps");

            migrationBuilder.DropIndex(
                name: "IX_FollowUps_UserId",
                table: "FollowUps");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FollowUps");
        }
    }
}
