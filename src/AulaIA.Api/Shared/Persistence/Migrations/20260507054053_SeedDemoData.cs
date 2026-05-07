using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoData : Migration
    {
        // ── IDs fijos para seed demo ──────────────────────────────────────────
        // Institución y usuarios ya existen en BD (sembrados antes).
        private static readonly Guid InstitutionId  = new("aa000001-0000-0000-0000-000000000001");
        private static readonly Guid TeacherId       = new("bb000001-0000-0000-0000-000000000002"); // Adriana Guido
        private const string         TeacherSub      = "auth0|PLACEHOLDER_ADRIANA";

        // Grupos
        private static readonly Guid Grp7A  = new("cc000001-0000-0000-0000-000000000001");
        private static readonly Guid Grp8B  = new("cc000001-0000-0000-0000-000000000002");

        // Estudiantes grupo 7° A
        private static readonly Guid Stu01 = new("dd000001-0000-0000-0000-000000000001");
        private static readonly Guid Stu02 = new("dd000001-0000-0000-0000-000000000002");
        private static readonly Guid Stu03 = new("dd000001-0000-0000-0000-000000000003");
        private static readonly Guid Stu04 = new("dd000001-0000-0000-0000-000000000004");
        private static readonly Guid Stu05 = new("dd000001-0000-0000-0000-000000000005");

        // Estudiantes grupo 8° B
        private static readonly Guid Stu06 = new("dd000001-0000-0000-0000-000000000006");
        private static readonly Guid Stu07 = new("dd000001-0000-0000-0000-000000000007");
        private static readonly Guid Stu08 = new("dd000001-0000-0000-0000-000000000008");
        private static readonly Guid Stu09 = new("dd000001-0000-0000-0000-000000000009");
        private static readonly Guid Stu10 = new("dd000001-0000-0000-0000-000000000010");

        // Actividades grupo 7° A
        private static readonly Guid Act01 = new("ee000001-0000-0000-0000-000000000001");
        private static readonly Guid Act02 = new("ee000001-0000-0000-0000-000000000002");
        private static readonly Guid Act03 = new("ee000001-0000-0000-0000-000000000003");

        // Actividades grupo 8° B
        private static readonly Guid Act04 = new("ee000001-0000-0000-0000-000000000004");
        private static readonly Guid Act05 = new("ee000001-0000-0000-0000-000000000005");
        private static readonly Guid Act06 = new("ee000001-0000-0000-0000-000000000006");

        private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Grupos ────────────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "groups",
                columns: new[] { "id", "name", "level", "subject", "school_year",
                                 "teacher_id", "teacher_sub", "institution_id",
                                 "is_active", "created_at",
                                 "pct_cotidiano", "pct_pruebas", "pct_extraclase", "pct_otros" },
                values: new object[,]
                {
                    { Grp7A, "7° A", "III Ciclo", "Matemáticas", 2026,
                      TeacherId, TeacherSub, InstitutionId,
                      true, SeedDate,
                      20m, 45m, 20m, 15m },
                    { Grp8B, "8° B", "III Ciclo", "Español", 2026,
                      TeacherId, TeacherSub, InstitutionId,
                      true, SeedDate,
                      20m, 45m, 20m, 15m },
                });

            // ── Estudiantes 7° A ──────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "students",
                columns: new[] { "id", "full_name", "student_code", "qr_code", "group_id", "is_active", "created_at" },
                values: new object[,]
                {
                    { Stu01, "Juan Pérez Rodríguez",    "2026-001", Stu01.ToString("N"), Grp7A, true, SeedDate },
                    { Stu02, "María García López",      "2026-002", Stu02.ToString("N"), Grp7A, true, SeedDate },
                    { Stu03, "Carlos Hernández Mora",   "2026-003", Stu03.ToString("N"), Grp7A, true, SeedDate },
                    { Stu04, "Ana Jiménez Castro",      "2026-004", Stu04.ToString("N"), Grp7A, true, SeedDate },
                    { Stu05, "Luis Vargas Quesada",     "2026-005", Stu05.ToString("N"), Grp7A, true, SeedDate },
                });

            // ── Estudiantes 8° B ──────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "students",
                columns: new[] { "id", "full_name", "student_code", "qr_code", "group_id", "is_active", "created_at" },
                values: new object[,]
                {
                    { Stu06, "Sofía Méndez Ulate",      "2026-006", Stu06.ToString("N"), Grp8B, true, SeedDate },
                    { Stu07, "Diego Chaves Arce",        "2026-007", Stu07.ToString("N"), Grp8B, true, SeedDate },
                    { Stu08, "Valentina Solís Fonseca",  "2026-008", Stu08.ToString("N"), Grp8B, true, SeedDate },
                    { Stu09, "Andrés Mora Sibaja",       "2026-009", Stu09.ToString("N"), Grp8B, true, SeedDate },
                    { Stu10, "Camila Rojas Herrera",     "2026-010", Stu10.ToString("N"), Grp8B, true, SeedDate },
                });

            // ── Actividades de evaluación 7° A ────────────────────────────────
            migrationBuilder.InsertData(
                table: "evaluation_activities",
                columns: new[] { "id", "group_id", "name", "type", "max_score", "percentage", "due_date", "created_at" },
                values: new object[,]
                {
                    { Act01, Grp7A, "Prueba Parcial I",    "Prueba",    100m, 45m, new DateOnly(2026, 3, 20), SeedDate },
                    { Act02, Grp7A, "Trabajo Cotidiano 1", "Cotidiano",  10m, 20m, new DateOnly(2026, 3,  7), SeedDate },
                    { Act03, Grp7A, "Trabajo Extraclase",  "Extraclase",100m, 20m, new DateOnly(2026, 4, 10), SeedDate },
                });

            // ── Actividades de evaluación 8° B ────────────────────────────────
            migrationBuilder.InsertData(
                table: "evaluation_activities",
                columns: new[] { "id", "group_id", "name", "type", "max_score", "percentage", "due_date", "created_at" },
                values: new object[,]
                {
                    { Act04, Grp8B, "Prueba Parcial I",    "Prueba",    100m, 45m, new DateOnly(2026, 3, 20), SeedDate },
                    { Act05, Grp8B, "Participación Oral",  "Cotidiano",  10m, 20m, new DateOnly(2026, 3, 14), SeedDate },
                    { Act06, Grp8B, "Ensayo Literario",    "Extraclase",100m, 20m, new DateOnly(2026, 4, 10), SeedDate },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "evaluation_activities", keyColumn: "id", keyValue: Act01);
            migrationBuilder.DeleteData(table: "evaluation_activities", keyColumn: "id", keyValue: Act02);
            migrationBuilder.DeleteData(table: "evaluation_activities", keyColumn: "id", keyValue: Act03);
            migrationBuilder.DeleteData(table: "evaluation_activities", keyColumn: "id", keyValue: Act04);
            migrationBuilder.DeleteData(table: "evaluation_activities", keyColumn: "id", keyValue: Act05);
            migrationBuilder.DeleteData(table: "evaluation_activities", keyColumn: "id", keyValue: Act06);

            migrationBuilder.DeleteData(table: "students", keyColumn: "id", keyValue: Stu01);
            migrationBuilder.DeleteData(table: "students", keyColumn: "id", keyValue: Stu02);
            migrationBuilder.DeleteData(table: "students", keyColumn: "id", keyValue: Stu03);
            migrationBuilder.DeleteData(table: "students", keyColumn: "id", keyValue: Stu04);
            migrationBuilder.DeleteData(table: "students", keyColumn: "id", keyValue: Stu05);
            migrationBuilder.DeleteData(table: "students", keyColumn: "id", keyValue: Stu06);
            migrationBuilder.DeleteData(table: "students", keyColumn: "id", keyValue: Stu07);
            migrationBuilder.DeleteData(table: "students", keyColumn: "id", keyValue: Stu08);
            migrationBuilder.DeleteData(table: "students", keyColumn: "id", keyValue: Stu09);
            migrationBuilder.DeleteData(table: "students", keyColumn: "id", keyValue: Stu10);

            migrationBuilder.DeleteData(table: "groups", keyColumn: "id", keyValue: Grp7A);
            migrationBuilder.DeleteData(table: "groups", keyColumn: "id", keyValue: Grp8B);
        }
    }
}

