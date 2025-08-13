using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintingProjectsManagment.Database.Migrations
{
    public partial class SplitOutboxes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                newName: "OutboxDomainMessages");

            migrationBuilder.CreateTable(
                name: "OutboxIntegrationEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OccurredUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CausationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    DoNotProcessBeforeUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxIntegrationEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationDeliveries",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Subscriber = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ProcessedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    DoNotProcessBeforeUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationDeliveries", x => new { x.EventId, x.Subscriber });
                    table.ForeignKey(
                        name: "FK_IntegrationDeliveries_OutboxIntegrationEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "OutboxIntegrationEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxIntegrationEvents_TenantId_Name_Version",
                table: "OutboxIntegrationEvents",
                columns: new[] { "TenantId", "Name", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxIntegrationEvents_ProcessedUtc",
                table: "OutboxIntegrationEvents",
                column: "ProcessedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxIntegrationEvents_CreatedUtc",
                table: "OutboxIntegrationEvents",
                column: "CreatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxIntegrationEvents_DoNotProcessBeforeUtc",
                table: "OutboxIntegrationEvents",
                column: "DoNotProcessBeforeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationDeliveries_ProcessedUtc",
                table: "IntegrationDeliveries",
                column: "ProcessedUtc");

            migrationBuilder.CreateTable(
                name: "MaterialCopies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PricePerUnit = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialCopies", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationDeliveries");

            migrationBuilder.DropTable(
                name: "OutboxIntegrationEvents");

            migrationBuilder.DropTable(
                name: "MaterialCopies");

            migrationBuilder.RenameTable(
                name: "OutboxDomainMessages",
                newName: "OutboxMessages");
        }
    }
}
