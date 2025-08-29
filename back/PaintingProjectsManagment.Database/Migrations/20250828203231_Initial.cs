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
                name: "__SeedHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DateApplied = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___SeedHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Identification = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Hidden = table.Column<bool>(type: "boolean", nullable: false),
                    IsProtected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OccurredUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CausationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TraceId = table.Column<string>(type: "text", nullable: true),
                    ParentSpanId = table.Column<string>(type: "text", nullable: true),
                    TraceFlags = table.Column<int>(type: "integer", nullable: true),
                    TraceState = table.Column<string>(type: "text", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<short>(type: "smallint", nullable: false),
                    DoNotProcessBeforeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClaimedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClaimedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPoisoned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainOutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    HandlerName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ReceivedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => new { x.EventId, x.HandlerName });
                });

            migrationBuilder.CreateTable(
                name: "IntegrationOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OccurredUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CausationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    TraceId = table.Column<string>(type: "text", nullable: true),
                    ParentSpanId = table.Column<string>(type: "text", nullable: true),
                    TraceFlags = table.Column<int>(type: "integer", nullable: true),
                    TraceState = table.Column<string>(type: "text", nullable: true),
                    ProcessedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<short>(type: "smallint", nullable: false),
                    DoNotProcessBeforeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClaimedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClaimedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPoisoned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationOutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "paints_catalog.brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paints_catalog.brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReadOnlyMaterials",
                columns: table => new
                {
                    Tenant = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PricePerUnit = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadOnlyMaterials", x => new { x.Tenant, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Alias = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Alias);
                });

            migrationBuilder.CreateTable(
                name: "paints_catalog.lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paints_catalog.lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_paints_catalog.lines_paints_catalog.brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "paints_catalog.brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "materials.materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PackageContent_Amount = table.Column<double>(type: "double precision", nullable: false),
                    PackageContent_Unit = table.Column<int>(type: "integer", nullable: false),
                    PackagePrice_Amount = table.Column<double>(type: "double precision", nullable: false),
                    PackagePrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materials.materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_materials.materials_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Alias",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "models.categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_models.categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_models.categories_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Alias",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "projects.projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PictureUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects.projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projects.projects_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Alias",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Alias",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    RefreshToken = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AuthenticationMode = table.Column<int>(type: "integer", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Avatar = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivationCode = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PasswordRedefineCode_CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordRedefineCode_Hash = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    RefreshTokenValidity = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Alias",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "paints_catalog.colors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HexColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    ManufacturerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BottleSize = table.Column<double>(type: "double precision", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    LineId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paints_catalog.colors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_paints_catalog.colors_paints_catalog.lines_LineId",
                        column: x => x.LineId,
                        principalTable: "paints_catalog.lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "models.models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Franchise = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: false),
                    Characters = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Artist = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tags = table.Column<string>(type: "text", nullable: false),
                    CoverPicture = table.Column<string>(type: "text", nullable: true),
                    Pictures = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BaseSize = table.Column<int>(type: "integer", nullable: false),
                    FigureSize = table.Column<int>(type: "integer", nullable: false),
                    NumberOfFigures = table.Column<int>(type: "integer", nullable: false),
                    SizeInMb = table.Column<int>(type: "integer", nullable: false),
                    MustHave = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Identity = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_models.models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_models.models_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Alias",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_models.models_models.categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "models.categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "projects.picture_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects.picture_references", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projects.picture_references_projects.projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects.projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects.pictures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects.pictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projects.pictures_projects.projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects.projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects.project_color_groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects.project_color_groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projects.project_color_groups_projects.projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects.projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects.project_materials",
                columns: table => new
                {
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects.project_materials", x => new { x.ProjectId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_projects.project_materials_materials.materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials.materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_projects.project_materials_projects.projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects.projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStepData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Step = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStepData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectStepData_projects.projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects.projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolesToClaims",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesToClaims", x => new { x.ClaimId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_RolesToClaims_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolesToClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsersToClaims",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    Access = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersToClaims", x => new { x.ClaimId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UsersToClaims_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsersToClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsersToRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersToRoles", x => new { x.RoleId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UsersToRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsersToRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project.project_color_sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Zone = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SuggestedColorIds = table.Column<string>(type: "text", nullable: false),
                    ColorGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project.project_color_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_project.project_color_sections_projects.project_color_group~",
                        column: x => x.ColorGroupId,
                        principalTable: "projects.project_color_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project.project_color_sections_projects.projects_ColorGroup~",
                        column: x => x.ColorGroupId,
                        principalTable: "projects.projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationOptions_Key_TenantId_Username",
                table: "ApplicationOptions",
                columns: new[] { "Key", "TenantId", "Username" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainOutboxMessages_CreatedUtc",
                table: "DomainOutboxMessages",
                column: "CreatedUtc",
                filter: "\"ProcessedUtc\" IS NULL AND \"IsPoisoned\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_DomainOutboxMessages_DoNotProcessBeforeUtc",
                table: "DomainOutboxMessages",
                column: "DoNotProcessBeforeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DomainOutboxMessages_IsPoisoned",
                table: "DomainOutboxMessages",
                column: "IsPoisoned");

            migrationBuilder.CreateIndex(
                name: "IX_DomainOutboxMessages_ProcessedUtc",
                table: "DomainOutboxMessages",
                column: "ProcessedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_ProcessedUtc",
                table: "InboxMessages",
                column: "ProcessedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationOutboxMessages_CreatedUtc",
                table: "IntegrationOutboxMessages",
                column: "CreatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationOutboxMessages_DoNotProcessBeforeUtc",
                table: "IntegrationOutboxMessages",
                column: "DoNotProcessBeforeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationOutboxMessages_IsPoisoned",
                table: "IntegrationOutboxMessages",
                column: "IsPoisoned");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationOutboxMessages_ProcessedUtc",
                table: "IntegrationOutboxMessages",
                column: "ProcessedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationOutboxMessages_TenantId_Name_Version",
                table: "IntegrationOutboxMessages",
                columns: new[] { "TenantId", "Name", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_materials.materials_Name",
                table: "materials.materials",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_materials.materials_Name_TenantId",
                table: "materials.materials",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_materials.materials_TenantId",
                table: "materials.materials",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_models.categories_TenantId_Name",
                table: "models.categories",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_models.models_CategoryId",
                table: "models.models",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_models.models_TenantId_Name",
                table: "models.models",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_paints_catalog.brands_Name",
                table: "paints_catalog.brands",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paints_catalog.colors_LineId",
                table: "paints_catalog.colors",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_paints_catalog.colors_LineId_HexColor",
                table: "paints_catalog.colors",
                columns: new[] { "LineId", "HexColor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paints_catalog.colors_LineId_Name",
                table: "paints_catalog.colors",
                columns: new[] { "LineId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paints_catalog.colors_Name",
                table: "paints_catalog.colors",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_paints_catalog.colors_Type",
                table: "paints_catalog.colors",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_paints_catalog.lines_BrandId_Name",
                table: "paints_catalog.lines",
                columns: new[] { "BrandId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project.project_color_sections_ColorGroupId_Zone",
                table: "project.project_color_sections",
                columns: new[] { "ColorGroupId", "Zone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects.picture_references_ProjectId",
                table: "projects.picture_references",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_projects.pictures_ProjectId",
                table: "projects.pictures",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_projects.project_color_groups_ProjectId",
                table: "projects.project_color_groups",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_projects.project_color_groups_ProjectId_Name",
                table: "projects.project_color_groups",
                columns: new[] { "ProjectId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects.project_materials_MaterialId",
                table: "projects.project_materials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_projects.project_materials_ProjectId",
                table: "projects.project_materials",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_projects.projects_EndDate",
                table: "projects.projects",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_projects.projects_Name",
                table: "projects.projects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_projects.projects_StartDate",
                table: "projects.projects",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_projects.projects_TenantId",
                table: "projects.projects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStepData_ProjectId",
                table: "ProjectStepData",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId",
                table: "Roles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesToClaims_RoleId",
                table: "RolesToClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersToClaims_UserId",
                table: "UsersToClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersToRoles_UserId",
                table: "UsersToRoles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__SeedHistory");

            migrationBuilder.DropTable(
                name: "ApplicationOptions");

            migrationBuilder.DropTable(
                name: "DomainOutboxMessages");

            migrationBuilder.DropTable(
                name: "InboxMessages");

            migrationBuilder.DropTable(
                name: "IntegrationOutboxMessages");

            migrationBuilder.DropTable(
                name: "models.models");

            migrationBuilder.DropTable(
                name: "paints_catalog.colors");

            migrationBuilder.DropTable(
                name: "project.project_color_sections");

            migrationBuilder.DropTable(
                name: "projects.picture_references");

            migrationBuilder.DropTable(
                name: "projects.pictures");

            migrationBuilder.DropTable(
                name: "projects.project_materials");

            migrationBuilder.DropTable(
                name: "ProjectStepData");

            migrationBuilder.DropTable(
                name: "ReadOnlyMaterials");

            migrationBuilder.DropTable(
                name: "RolesToClaims");

            migrationBuilder.DropTable(
                name: "UsersToClaims");

            migrationBuilder.DropTable(
                name: "UsersToRoles");

            migrationBuilder.DropTable(
                name: "models.categories");

            migrationBuilder.DropTable(
                name: "paints_catalog.lines");

            migrationBuilder.DropTable(
                name: "projects.project_color_groups");

            migrationBuilder.DropTable(
                name: "materials.materials");

            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "paints_catalog.brands");

            migrationBuilder.DropTable(
                name: "projects.projects");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
