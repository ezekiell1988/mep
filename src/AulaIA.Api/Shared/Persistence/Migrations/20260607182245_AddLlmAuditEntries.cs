using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLlmAuditEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "llm_audit_entries",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    component = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    intent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    result = table.Column<string>(type: "text", nullable: false),
                    context_json = table.Column<string>(type: "text", nullable: true),
                    is_error = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_llm_audit_entries", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_llm_audit_entries_category",
                table: "llm_audit_entries",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_llm_audit_entries_created_at",
                table: "llm_audit_entries",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_llm_audit_entries_is_error",
                table: "llm_audit_entries",
                column: "is_error");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "llm_audit_entries");
        }
    }
}
