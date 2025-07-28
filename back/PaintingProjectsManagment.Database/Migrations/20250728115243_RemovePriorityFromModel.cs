using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemovePriorityFromModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Models");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Models",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
