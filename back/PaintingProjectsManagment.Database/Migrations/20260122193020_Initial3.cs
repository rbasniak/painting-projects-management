using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuggestedColorIds",
                table: "project.project_color_sections");

            migrationBuilder.AddColumn<string>(
                name: "SuggestedColorsJson",
                table: "project.project_color_sections",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuggestedColorsJson",
                table: "project.project_color_sections");

            migrationBuilder.AddColumn<string>(
                name: "SuggestedColorIds",
                table: "project.project_color_sections",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
