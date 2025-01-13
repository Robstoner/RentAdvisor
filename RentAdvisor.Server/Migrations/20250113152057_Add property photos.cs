using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentAdvisor.Server.Migrations
{
    /// <inheritdoc />
    public partial class Addpropertyphotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertiesPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertiesPhotos", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyPropertyPhotos");

            migrationBuilder.DropTable(
                name: "PropertiesPhotos");
        }
    }
}
