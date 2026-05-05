using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumAndPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "curriculum_units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Asignatura = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ciclo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nivel = table.Column<int>(type: "integer", nullable: false),
                    Trimestre = table.Column<int>(type: "integer", nullable: false),
                    UnidadNumero = table.Column<int>(type: "integer", nullable: false),
                    UnidadNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AprendizajesEsperados = table.Column<string>(type: "jsonb", nullable: false),
                    IndicadoresEvaluacion = table.Column<string>(type: "jsonb", nullable: false),
                    ContenidoConceptual = table.Column<string>(type: "jsonb", nullable: false),
                    ContenidoProcedimental = table.Column<string>(type: "jsonb", nullable: false),
                    ContenidoActitudinal = table.Column<string>(type: "jsonb", nullable: false),
                    EstrategiasSugeridas = table.Column<string>(type: "jsonb", nullable: false),
                    PdfSourceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExtractedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ValidatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curriculum_units", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lesson_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherSub = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Asignatura = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nivel = table.Column<int>(type: "integer", nullable: false),
                    Trimestre = table.Column<int>(type: "integer", nullable: false),
                    AnioLectivo = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    LeccionesPorSemana = table.Column<int>(type: "integer", nullable: false),
                    ContenidoGenerado = table.Column<string>(type: "text", nullable: true),
                    ArchivoBlobUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lesson_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lesson_plans_groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_units_Asignatura_Nivel_Trimestre_UnidadNumero",
                table: "curriculum_units",
                columns: new[] { "Asignatura", "Nivel", "Trimestre", "UnidadNumero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lesson_plans_GroupId_Trimestre_AnioLectivo",
                table: "lesson_plans",
                columns: new[] { "GroupId", "Trimestre", "AnioLectivo" });

            migrationBuilder.CreateIndex(
                name: "IX_lesson_plans_TeacherSub",
                table: "lesson_plans",
                column: "TeacherSub");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "curriculum_units");

            migrationBuilder.DropTable(
                name: "lesson_plans");
        }
    }
}
