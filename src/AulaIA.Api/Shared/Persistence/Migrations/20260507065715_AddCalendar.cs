using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calendar_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    school_year = table.Column<int>(type: "integer", nullable: false),
                    created_by_auth0_sub = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_calendar_events_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_events_group_id_date",
                table: "calendar_events",
                columns: new[] { "group_id", "date" });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_events_school_year_group_id",
                table: "calendar_events",
                columns: new[] { "school_year", "group_id" });

            // ── Seed: feriados nacionales de Costa Rica 2026 ──────────────────
            // GroupId = null → aplican a todos los grupos
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            migrationBuilder.InsertData(
                table: "calendar_events",
                columns: new[] { "id", "group_id", "date", "end_date", "title", "type", "school_year", "created_by_auth0_sub", "created_at" },
                values: new object[,]
                {
                    { new Guid("ca000001-0000-0000-0000-000000000001"), null, new DateOnly(2026,  1,  1), null,            "Año Nuevo",                  "Holiday", 2026, null, seedDate },
                    { new Guid("ca000001-0000-0000-0000-000000000002"), null, new DateOnly(2026,  4,  2), null,            "Jueves Santo",               "Holiday", 2026, null, seedDate },
                    { new Guid("ca000001-0000-0000-0000-000000000003"), null, new DateOnly(2026,  4,  3), null,            "Viernes Santo",              "Holiday", 2026, null, seedDate },
                    { new Guid("ca000001-0000-0000-0000-000000000004"), null, new DateOnly(2026,  4, 11), null,            "Juan Santamaría",            "Holiday", 2026, null, seedDate },
                    { new Guid("ca000001-0000-0000-0000-000000000005"), null, new DateOnly(2026,  5,  1), null,            "Día del Trabajador",         "Holiday", 2026, null, seedDate },
                    { new Guid("ca000001-0000-0000-0000-000000000006"), null, new DateOnly(2026,  7, 25), null,            "Anexión de Guanacaste",      "Holiday", 2026, null, seedDate },
                    { new Guid("ca000001-0000-0000-0000-000000000007"), null, new DateOnly(2026,  8,  2), null,            "Virgen de los Ángeles",      "Holiday", 2026, null, seedDate },
                    { new Guid("ca000001-0000-0000-0000-000000000008"), null, new DateOnly(2026,  8, 15), null,            "Día de la Madre",            "Holiday", 2026, null, seedDate },
                    { new Guid("ca000001-0000-0000-0000-000000000009"), null, new DateOnly(2026,  9, 15), null,            "Día de la Independencia",    "Holiday", 2026, null, seedDate },
                    { new Guid("ca000001-0000-0000-0000-000000000010"), null, new DateOnly(2026, 12, 25), null,            "Navidad",                    "Holiday", 2026, null, seedDate },
                    // Semana Santa (receso lectivo completo)
                    { new Guid("ca000001-0000-0000-0000-000000000011"), null, new DateOnly(2026,  4,  6), new DateOnly(2026, 4, 10), "Semana Santa — receso lectivo", "Holiday", 2026, null, seedDate },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendar_events");
        }
    }
}
