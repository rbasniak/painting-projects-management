using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToModelCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ModelCategories_Name",
                table: "ModelCategories");

            migrationBuilder.DropIndex(
                name: "IX_ModelCategories_TenantId",
                table: "ModelCategories");

            migrationBuilder.CreateIndex(
                name: "IX_ModelCategories_TenantId_Name",
                table: "ModelCategories",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ModelCategories_TenantId_Name",
                table: "ModelCategories");

            migrationBuilder.CreateIndex(
                name: "IX_ModelCategories_Name",
                table: "ModelCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ModelCategories_TenantId",
                table: "ModelCategories",
                column: "TenantId");
        }
    }
}
