using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentAdvisor.Server.Migrations
{
    /// <inheritdoc />
    public partial class Addpropertyphotosfixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyPropertyPhotos");

            migrationBuilder.AddColumn<Guid>(
                name: "PropertyId",
                table: "PropertiesPhotos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PropertiesPhotos_PropertyId",
                table: "PropertiesPhotos",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertiesPhotos_Properties_PropertyId",
                table: "PropertiesPhotos",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertiesPhotos_Properties_PropertyId",
                table: "PropertiesPhotos");

            migrationBuilder.DropIndex(
                name: "IX_PropertiesPhotos_PropertyId",
                table: "PropertiesPhotos");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "PropertiesPhotos");

            migrationBuilder.CreateTable(
                name: "PropertyPropertyPhotos",
                columns: table => new
                {
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyPhotosId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyPropertyPhotos", x => new { x.PropertyId, x.PropertyPhotosId });
                    table.ForeignKey(
                        name: "FK_PropertyPropertyPhotos_PropertiesPhotos_PropertyPhotosId",
                        column: x => x.PropertyPhotosId,
                        principalTable: "PropertiesPhotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyPropertyPhotos_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPropertyPhotos_PropertyPhotosId",
                table: "PropertyPropertyPhotos",
                column: "PropertyPhotosId");
        }
    }
}
