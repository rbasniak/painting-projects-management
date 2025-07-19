using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Materials",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Name_TenantId",
                table: "Materials",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_TenantId",
                table: "Materials",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Tenants_TenantId",
                table: "Materials",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Alias",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Tenants_TenantId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_Name_TenantId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_TenantId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Materials");
        }
    }
}
