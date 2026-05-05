using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "institutions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    circuit_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    region_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_institutions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    auth0_sub = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Teacher"),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_institution",
                        column: x => x.institution_id,
                        principalTable: "institutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Año/nivel escolar, ej. '7° Año'"),
                    subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Materia, ej. 'Matemáticas'"),
                    school_year = table.Column<int>(type: "integer", nullable: false, defaultValue: 2026),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_groups_institution",
                        column: x => x.institution_id,
                        principalTable: "institutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_groups_teacher",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evaluation_activities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Ej: 'Prueba Escrita', 'Trabajo Cotidiano', 'Proyecto', 'Portafolio'"),
                    max_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false, defaultValue: 100m),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, comment: "Porcentaje que representa dentro del período, ej. 20.00"),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluation_activities", x => x.id);
                    table.ForeignKey(
                        name: "fk_eval_activities_group",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    student_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Número de expediente MEP"),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.id);
                    table.ForeignKey(
                        name: "fk_students_group",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attendance_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Present", comment: "Present | Absent | Late | Justified"),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_attendance_group",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attendance_student",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false, comment: "Nota obtenida, ej. 87.50"),
                    comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades", x => x.id);
                    table.ForeignKey(
                        name: "fk_grades_activity",
                        column: x => x.activity_id,
                        principalTable: "evaluation_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grades_student",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "institutions",
                columns: new[] { "id", "circuit_code", "created_at", "name", "region_code" },
                values: new object[,]
                {
                    { new Guid("aa000001-0000-0000-0000-000000000001"), "06", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo de Aserrí", "DES" },
                    { new Guid("aa000001-0000-0000-0000-000000000002"), "06", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Colegio Técnico Profesional de Aserrí", "DES" },
                    { new Guid("aa000001-0000-0000-0000-000000000003"), "06", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Colegio Nocturno de Aserrí", "DES" },
                    { new Guid("aa000001-0000-0000-0000-000000000004"), "06", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo San Gabriel", "DES" },
                    { new Guid("aa000001-0000-0000-0000-000000000005"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Colegio Técnico Profesional de Desamparados", "DES" },
                    { new Guid("aa000001-0000-0000-0000-000000000006"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo de Desamparados", "DES" },
                    { new Guid("aa000001-0000-0000-0000-000000000007"), "03", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo Ing. Manuel Benavides", "DES" },
                    { new Guid("aa000001-0000-0000-0000-000000000008"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo de Costa Rica", "SJC" },
                    { new Guid("aa000001-0000-0000-0000-000000000009"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Colegio de Señoritas", "SJC" },
                    { new Guid("aa000001-0000-0000-0000-000000000010"), "02", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo Julio Fonseca", "SJC" },
                    { new Guid("aa000001-0000-0000-0000-000000000011"), "03", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo José Joaquín Vargas Calvo", "SJC" },
                    { new Guid("aa000001-0000-0000-0000-000000000012"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo de Guadalupe", "SJN" },
                    { new Guid("aa000001-0000-0000-0000-000000000013"), "02", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Colegio Técnico Profesional de Moravia", "SJN" },
                    { new Guid("aa000001-0000-0000-0000-000000000014"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo de Heredia", "HER" },
                    { new Guid("aa000001-0000-0000-0000-000000000015"), "02", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo Daniel Oduber Quirós", "HER" },
                    { new Guid("aa000001-0000-0000-0000-000000000016"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo de Cartago", "CAR" },
                    { new Guid("aa000001-0000-0000-0000-000000000017"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Colegio Técnico Profesional de Cartago", "CAR" },
                    { new Guid("aa000001-0000-0000-0000-000000000018"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo de Alajuela", "ALA" },
                    { new Guid("aa000001-0000-0000-0000-000000000019"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Colegio Técnico Profesional Jesús Ocaña Rojas", "ALA" },
                    { new Guid("aa000001-0000-0000-0000-000000000020"), "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liceo de Escazú", "SJO" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "auth0_sub", "created_at", "email", "full_name", "institution_id", "role" },
                values: new object[] { new Guid("bb000001-0000-0000-0000-000000000001"), "auth0|PLACEHOLDER_EZEQUIEL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ezekiell1988@hotmail.com", "Ezequiel Baltodano", new Guid("aa000001-0000-0000-0000-000000000001"), "Admin" });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "auth0_sub", "created_at", "email", "full_name", "institution_id" },
                values: new object[] { new Guid("bb000001-0000-0000-0000-000000000002"), "auth0|PLACEHOLDER_ADRIANA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ezekiell1988@gmail.com", "Adriana Guido", new Guid("aa000001-0000-0000-0000-000000000001") });

            migrationBuilder.CreateIndex(
                name: "ix_attendance_group_date",
                table: "attendance_records",
                columns: new[] { "group_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_attendance_student_date",
                table: "attendance_records",
                columns: new[] { "student_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_eval_activities_group_id",
                table: "evaluation_activities",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_grades_activity_student",
                table: "grades",
                columns: new[] { "activity_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_grades_student_id",
                table: "grades",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_institution_id",
                table: "groups",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_teacher_year",
                table: "groups",
                columns: new[] { "teacher_id", "school_year" });

            migrationBuilder.CreateIndex(
                name: "ix_institutions_name",
                table: "institutions",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_students_code_group",
                table: "students",
                columns: new[] { "student_code", "group_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_group_id",
                table: "students",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_auth0_sub",
                table: "users",
                column: "auth0_sub",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_institution_id",
                table: "users",
                column: "institution_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_records");

            migrationBuilder.DropTable(
                name: "grades");

            migrationBuilder.DropTable(
                name: "evaluation_activities");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "institutions");
        }
    }
}
