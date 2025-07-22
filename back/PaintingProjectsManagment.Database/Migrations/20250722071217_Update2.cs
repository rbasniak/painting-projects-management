using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class Update2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Models_Name",
                table: "Models");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "Models",
                newName: "SizeInMb");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Models",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Models_TenantId_Name",
                table: "Models",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Models_Tenants_TenantId",
                table: "Models",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Alias",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Models_Tenants_TenantId",
                table: "Models");

            migrationBuilder.DropIndex(
                name: "IX_Models_TenantId_Name",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Models");

            migrationBuilder.RenameColumn(
                name: "SizeInMb",
                table: "Models",
                newName: "Size");

            migrationBuilder.CreateIndex(
                name: "IX_Models_Name",
                table: "Models",
                column: "Name");
        }
    }
}
