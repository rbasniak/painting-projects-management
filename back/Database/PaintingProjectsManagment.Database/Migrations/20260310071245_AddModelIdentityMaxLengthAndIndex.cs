using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddModelIdentityMaxLengthAndIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Identity",
                table: "models.models",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_models.models_TenantId_Identity",
                table: "models.models",
                columns: new[] { "TenantId", "Identity" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_models.models_TenantId_Identity",
                table: "models.models");

            migrationBuilder.AlterColumn<string>(
                name: "Identity",
                table: "models.models",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);
        }
    }
}
