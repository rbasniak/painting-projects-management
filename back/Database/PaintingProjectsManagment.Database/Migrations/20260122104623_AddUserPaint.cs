using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPaint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "user_paints",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PaintColorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_paints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_paints_paints_catalog.colors_PaintColorId",
                        column: x => x.PaintColorId,
                        principalTable: "paints_catalog.colors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_paints_PaintColorId",
                schema: "inventory",
                table: "user_paints",
                column: "PaintColorId");

            migrationBuilder.CreateIndex(
                name: "IX_user_paints_Username",
                schema: "inventory",
                table: "user_paints",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_user_paints_Username_PaintColorId",
                schema: "inventory",
                table: "user_paints",
                columns: new[] { "Username", "PaintColorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_paints",
                schema: "inventory");
        }
    }
}
