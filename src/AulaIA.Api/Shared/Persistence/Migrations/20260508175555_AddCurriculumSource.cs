using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "curriculum_sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Asignatura = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ciclo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MepUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LastEtag = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastModifiedMep = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curriculum_sources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_sources_Asignatura_Ciclo",
                table: "curriculum_sources",
                columns: new[] { "Asignatura", "Ciclo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "curriculum_sources");
        }
    }
}
