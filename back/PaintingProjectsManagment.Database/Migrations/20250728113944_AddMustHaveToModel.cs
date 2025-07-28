using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMustHaveToModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MustHave",
                table: "Models",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustHave",
                table: "Models");
        }
    }
}
