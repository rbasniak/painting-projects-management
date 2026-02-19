using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class Changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReadOnlyMaterials");

            migrationBuilder.CreateTable(
                name: "projects.projections.materials",
                columns: table => new
                {
                    Tenant = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PricePerUnit_Amount = table.Column<double>(type: "double precision", nullable: false),
                    PricePerUnit_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects.projections.materials", x => new { x.Tenant, x.Id });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "projects.projections.materials");

            migrationBuilder.CreateTable(
                name: "ReadOnlyMaterials",
                columns: table => new
                {
                    Tenant = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PricePerUnit_Amount = table.Column<double>(type: "double precision", nullable: false),
                    PricePerUnit_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadOnlyMaterials", x => new { x.Tenant, x.Id });
                });
        }
    }
}
