using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdrianaFromSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Los grupos de demo apuntan al user de Adriana (FK Restrict).
            // Los reasignamos a Ezequiel antes de eliminar la fila.
            migrationBuilder.Sql("""
                UPDATE groups
                SET    teacher_id  = 'bb000001-0000-0000-0000-000000000001',
                       teacher_sub = 'auth0|69fae47c268da9d7e46c6d4b'
                WHERE  teacher_id  = 'bb000001-0000-0000-0000-000000000002';
                """);

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("bb000001-0000-0000-0000-000000000002"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "auth0_sub", "created_at", "email", "full_name", "institution_id", "referred_by_code", "role" },
                values: new object[] { new Guid("bb000001-0000-0000-0000-000000000002"), "auth0|PLACEHOLDER_ADRIANA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ezekiell1988@gmail.com", "Adriana Guido", new Guid("aa000001-0000-0000-0000-000000000001"), null, "Teacher" });
        }
    }
}
