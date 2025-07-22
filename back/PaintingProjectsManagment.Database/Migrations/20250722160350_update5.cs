using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class update5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaintLines_BrandId",
                table: "PaintLines");

            migrationBuilder.DropIndex(
                name: "IX_PaintLines_Name",
                table: "PaintLines");

            migrationBuilder.DropIndex(
                name: "IX_PaintBrands_Name",
                table: "PaintBrands");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Paints");

            migrationBuilder.CreateIndex(
                name: "IX_PaintLines_BrandId_Name",
                table: "PaintLines",
                columns: new[] { "BrandId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaintBrands_Name",
                table: "PaintBrands",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaintLines_BrandId_Name",
                table: "PaintLines");

            migrationBuilder.DropIndex(
                name: "IX_PaintBrands_Name",
                table: "PaintBrands");

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Paints",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_PaintLines_BrandId",
                table: "PaintLines",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintLines_Name",
                table: "PaintLines",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaintBrands_Name",
                table: "PaintBrands",
                column: "Name");
        }
    }
}
