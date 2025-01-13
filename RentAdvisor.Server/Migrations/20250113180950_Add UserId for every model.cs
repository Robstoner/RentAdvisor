using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentAdvisor.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdforeverymodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PropertiesPhotos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Properties",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PropertiesPhotos_UserId",
                table: "PropertiesPhotos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_UserId",
                table: "Properties",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_AspNetUsers_UserId",
                table: "Properties",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertiesPhotos_AspNetUsers_UserId",
                table: "PropertiesPhotos",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_AspNetUsers_UserId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertiesPhotos_AspNetUsers_UserId",
                table: "PropertiesPhotos");

            migrationBuilder.DropIndex(
                name: "IX_PropertiesPhotos_UserId",
                table: "PropertiesPhotos");

            migrationBuilder.DropIndex(
                name: "IX_Properties_UserId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PropertiesPhotos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Properties");
        }
    }
}
