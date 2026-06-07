using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedEzequielPlaneamientoDemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "groups",
                columns: new[] { "id", "created_at", "institution_id", "is_active", "level", "name", "pct_cotidiano", "pct_extraclase", "pct_otros", "pct_pruebas", "school_year", "subject", "teacher_id", "teacher_sub" },
                values: new object[] { new Guid("cc000002-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aa000001-0000-0000-0000-000000000001"), true, "7° Año", "7-1 Artes Plásticas", 20m, 20m, 15m, 45m, 2026, "Artes Plásticas", new Guid("bb000001-0000-0000-0000-000000000001"), "auth0|69fae47c268da9d7e46c6d4b" });

            migrationBuilder.InsertData(
                table: "calendar_events",
                columns: new[] { "id", "created_at", "created_by_auth0_sub", "date", "end_date", "group_id", "school_year", "title", "type" },
                values: new object[,]
                {
                    { new Guid("ff000002-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "auth0|69fae47c268da9d7e46c6d4b", new DateOnly(2026, 2, 23), null, new Guid("cc000002-0000-0000-0000-000000000001"), 2026, "Acto cívico institucional", "Civic" },
                    { new Guid("ff000002-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "auth0|69fae47c268da9d7e46c6d4b", new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 20), new Guid("cc000002-0000-0000-0000-000000000001"), 2026, "Semana de exámenes I Trimestre", "Exam" },
                    { new Guid("ff000002-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "auth0|69fae47c268da9d7e46c6d4b", new DateOnly(2026, 3, 27), null, new Guid("cc000002-0000-0000-0000-000000000001"), 2026, "Consejo de profesores", "TeacherMeeting" },
                    { new Guid("ff000002-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "auth0|69fae47c268da9d7e46c6d4b", new DateOnly(2026, 4, 13), new DateOnly(2026, 4, 17), new Guid("cc000002-0000-0000-0000-000000000001"), 2026, "FEA institucional", "SportWeek" },
                    { new Guid("ff000002-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "auth0|69fae47c268da9d7e46c6d4b", new DateOnly(2026, 4, 24), null, new Guid("cc000002-0000-0000-0000-000000000001"), 2026, "Capacitación institucional", "Institutional" }
                });

            migrationBuilder.InsertData(
                table: "students",
                columns: new[] { "id", "created_at", "full_name", "group_id", "is_active", "qr_code", "student_code" },
                values: new object[,]
                {
                    { new Guid("dd000002-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Valeria Arias Montero", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000001", "AP7-2026-001" },
                    { new Guid("dd000002-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sebastián Barrantes Solís", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000002", "AP7-2026-002" },
                    { new Guid("dd000002-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "María José Brenes Castro", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000003", "AP7-2026-003" },
                    { new Guid("dd000002-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Diego Calderón Vargas", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000004", "AP7-2026-004" },
                    { new Guid("dd000002-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Camila Campos Rojas", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000005", "AP7-2026-005" },
                    { new Guid("dd000002-0000-0000-0000-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Andrés Cordero Jiménez", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000006", "AP7-2026-006" },
                    { new Guid("dd000002-0000-0000-0000-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sofía Delgado Mora", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000007", "AP7-2026-007" },
                    { new Guid("dd000002-0000-0000-0000-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Daniela Díaz Hernández", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000008", "AP7-2026-008" },
                    { new Guid("dd000002-0000-0000-0000-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Samuel Fernández Chaves", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000009", "AP7-2026-009" },
                    { new Guid("dd000002-0000-0000-0000-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lucía Flores Quesada", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000010", "AP7-2026-010" },
                    { new Guid("dd000002-0000-0000-0000-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mateo García López", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000011", "AP7-2026-011" },
                    { new Guid("dd000002-0000-0000-0000-000000000012"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Isabella Gómez Arce", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000012", "AP7-2026-012" },
                    { new Guid("dd000002-0000-0000-0000-000000000013"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Emiliano Gutiérrez Salazar", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000013", "AP7-2026-013" },
                    { new Guid("dd000002-0000-0000-0000-000000000014"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Natalia Herrera Ulate", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000014", "AP7-2026-014" },
                    { new Guid("dd000002-0000-0000-0000-000000000015"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gabriel Jiménez Rodríguez", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000015", "AP7-2026-015" },
                    { new Guid("dd000002-0000-0000-0000-000000000016"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Paula León Sánchez", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000016", "AP7-2026-016" },
                    { new Guid("dd000002-0000-0000-0000-000000000017"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Julián López Núñez", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000017", "AP7-2026-017" },
                    { new Guid("dd000002-0000-0000-0000-000000000018"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mariana Martínez Vega", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000018", "AP7-2026-018" },
                    { new Guid("dd000002-0000-0000-0000-000000000019"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Esteban Méndez Castro", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000019", "AP7-2026-019" },
                    { new Guid("dd000002-0000-0000-0000-000000000020"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ana Lucía Molina Rojas", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000020", "AP7-2026-020" },
                    { new Guid("dd000002-0000-0000-0000-000000000021"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "José Pablo Mora Alfaro", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000021", "AP7-2026-021" },
                    { new Guid("dd000002-0000-0000-0000-000000000022"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Daniel Navarro Céspedes", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000022", "AP7-2026-022" },
                    { new Guid("dd000002-0000-0000-0000-000000000023"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fernanda Núñez Chacón", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000023", "AP7-2026-023" },
                    { new Guid("dd000002-0000-0000-0000-000000000024"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Adrián Pacheco Soto", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000024", "AP7-2026-024" },
                    { new Guid("dd000002-0000-0000-0000-000000000025"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nicole Pérez Bonilla", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000025", "AP7-2026-025" },
                    { new Guid("dd000002-0000-0000-0000-000000000026"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Luis Diego Quesada Ramírez", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000026", "AP7-2026-026" },
                    { new Guid("dd000002-0000-0000-0000-000000000027"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Laura Ramírez Fallas", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000027", "AP7-2026-027" },
                    { new Guid("dd000002-0000-0000-0000-000000000028"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fabián Rojas Hidalgo", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000028", "AP7-2026-028" },
                    { new Guid("dd000002-0000-0000-0000-000000000029"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Daniela Salazar Picado", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000029", "AP7-2026-029" },
                    { new Guid("dd000002-0000-0000-0000-000000000030"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Christopher Sánchez Vargas", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000030", "AP7-2026-030" },
                    { new Guid("dd000002-0000-0000-0000-000000000031"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Michelle Segura Campos", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000031", "AP7-2026-031" },
                    { new Guid("dd000002-0000-0000-0000-000000000032"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kevin Solano Aguilar", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000032", "AP7-2026-032" },
                    { new Guid("dd000002-0000-0000-0000-000000000033"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ariana Soto Morales", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000033", "AP7-2026-033" },
                    { new Guid("dd000002-0000-0000-0000-000000000034"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Brandon Torres Rivas", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000034", "AP7-2026-034" },
                    { new Guid("dd000002-0000-0000-0000-000000000035"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Melissa Ureña Castro", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000035", "AP7-2026-035" },
                    { new Guid("dd000002-0000-0000-0000-000000000036"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Anthony Valverde Mora", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000036", "AP7-2026-036" },
                    { new Guid("dd000002-0000-0000-0000-000000000037"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Karla Vargas Esquivel", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000037", "AP7-2026-037" },
                    { new Guid("dd000002-0000-0000-0000-000000000038"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Jeremy Vega Delgado", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000038", "AP7-2026-038" },
                    { new Guid("dd000002-0000-0000-0000-000000000039"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tatiana Villalobos Cordero", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000039", "AP7-2026-039" },
                    { new Guid("dd000002-0000-0000-0000-000000000040"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Santiago Zamora Ruiz", new Guid("cc000002-0000-0000-0000-000000000001"), true, "dd000002000000000000000000000040", "AP7-2026-040" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "calendar_events",
                keyColumn: "id",
                keyValue: new Guid("ff000002-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "calendar_events",
                keyColumn: "id",
                keyValue: new Guid("ff000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "calendar_events",
                keyColumn: "id",
                keyValue: new Guid("ff000002-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "calendar_events",
                keyColumn: "id",
                keyValue: new Guid("ff000002-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "calendar_events",
                keyColumn: "id",
                keyValue: new Guid("ff000002-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000025"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000026"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000027"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000028"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000029"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000030"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000031"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000032"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000033"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000034"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000035"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000036"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000037"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000038"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000039"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "id",
                keyValue: new Guid("dd000002-0000-0000-0000-000000000040"));

            migrationBuilder.DeleteData(
                table: "groups",
                keyColumn: "id",
                keyValue: new Guid("cc000002-0000-0000-0000-000000000001"));
        }
    }
}
