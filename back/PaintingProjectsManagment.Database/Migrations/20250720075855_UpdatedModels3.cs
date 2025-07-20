using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModels3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "ModelCategories",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelCategories_TenantId",
                table: "ModelCategories",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelCategories_Tenants_TenantId",
                table: "ModelCategories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Alias",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModelCategories_Tenants_TenantId",
                table: "ModelCategories");

            migrationBuilder.DropIndex(
                name: "IX_ModelCategories_TenantId",
                table: "ModelCategories");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ModelCategories");
        }
    }
}
