using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Unit = table.Column<int>(type: "INTEGER", nullable: false),
                    PricePerUnit = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModelCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaintBrands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintBrands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectColorGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectColorGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PictureUrl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Artist = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    PictureUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseSize = table.Column<int>(type: "INTEGER", nullable: false),
                    FigureSize = table.Column<int>(type: "INTEGER", nullable: false),
                    NumberOfFigures = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Models_ModelCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ModelCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaintLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BrandId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaintLines_PaintBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "PaintBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectColorSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Zone = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    SuggestedColorIds = table.Column<string>(type: "TEXT", nullable: false),
                    ColorGroupId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectColorSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectColorSections_ProjectColorGroups_ColorGroupId",
                        column: x => x.ColorGroupId,
                        principalTable: "ProjectColorGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectColorSections_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMaterials",
                columns: table => new
                {
                    MaterialId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMaterials", x => new { x.ProjectId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_ProjectMaterials_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPictures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPictures_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectReferences_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Planning_Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Planning_Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Planning_Duration = table.Column<double>(type: "REAL", nullable: false),
                    Painting_Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Painting_Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Painting_Duration = table.Column<double>(type: "REAL", nullable: false),
                    Preparation_Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Preparation_Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Preparation_Duration = table.Column<double>(type: "REAL", nullable: false),
                    Supporting_Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Supporting_Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Supporting_Duration = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSteps_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    HexColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    ManufacturerCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    BottleSize = table.Column<double>(type: "REAL", nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    LineId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paints_PaintLines_LineId",
                        column: x => x.LineId,
                        principalTable: "PaintLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Name",
                table: "Materials",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ModelCategories_Name",
                table: "ModelCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Models_CategoryId",
                table: "Models",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Models_Name",
                table: "Models",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaintBrands_Name",
                table: "PaintBrands",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaintLines_BrandId",
                table: "PaintLines",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintLines_Name",
                table: "PaintLines",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Paints_LineId",
                table: "Paints",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_Paints_Name",
                table: "Paints",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Paints_Type",
                table: "Paints",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectColorGroups_Name",
                table: "ProjectColorGroups",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectColorGroups_ProjectId",
                table: "ProjectColorGroups",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectColorSections_ColorGroupId",
                table: "ProjectColorSections",
                column: "ColorGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectColorSections_ProjectId",
                table: "ProjectColorSections",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectColorSections_Zone",
                table: "ProjectColorSections",
                column: "Zone");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMaterials_MaterialId",
                table: "ProjectMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMaterials_ProjectId",
                table: "ProjectMaterials",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPictures_ProjectId",
                table: "ProjectPictures",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectReferences_ProjectId",
                table: "ProjectReferences",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_EndDate",
                table: "Projects",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_StartDate",
                table: "Projects",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSteps_ProjectId",
                table: "ProjectSteps",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "Paints");

            migrationBuilder.DropTable(
                name: "ProjectColorSections");

            migrationBuilder.DropTable(
                name: "ProjectMaterials");

            migrationBuilder.DropTable(
                name: "ProjectPictures");

            migrationBuilder.DropTable(
                name: "ProjectReferences");

            migrationBuilder.DropTable(
                name: "ProjectSteps");

            migrationBuilder.DropTable(
                name: "ModelCategories");

            migrationBuilder.DropTable(
                name: "PaintLines");

            migrationBuilder.DropTable(
                name: "ProjectColorGroups");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "PaintBrands");
        }
    }
}
