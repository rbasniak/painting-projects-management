using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ColorGroupId1",
                table: "project.project_color_sections",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_project.project_color_sections_ColorGroupId1",
                table: "project.project_color_sections",
                column: "ColorGroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_project.project_color_sections_projects.project_color_grou~1",
                table: "project.project_color_sections",
                column: "ColorGroupId1",
                principalTable: "projects.project_color_groups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_project.project_color_sections_projects.project_color_grou~1",
                table: "project.project_color_sections");

            migrationBuilder.DropIndex(
                name: "IX_project.project_color_sections_ColorGroupId1",
                table: "project.project_color_sections");

            migrationBuilder.DropColumn(
                name: "ColorGroupId1",
                table: "project.project_color_sections");
        }
    }
}
