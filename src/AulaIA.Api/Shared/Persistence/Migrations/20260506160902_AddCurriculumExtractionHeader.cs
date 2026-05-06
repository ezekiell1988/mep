using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumExtractionHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractedAt",
                table: "curriculum_units");

            migrationBuilder.DropColumn(
                name: "PdfSourceUrl",
                table: "curriculum_units");

            migrationBuilder.DropColumn(
                name: "TokensUsed",
                table: "curriculum_units");

            migrationBuilder.AddColumn<Guid>(
                name: "ExtractionId",
                table: "curriculum_units",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "curriculum_extractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Asignatura = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ciclo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PdfSourceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TotalTokensUsed = table.Column<int>(type: "integer", nullable: false),
                    UnidadCount = table.Column<int>(type: "integer", nullable: false),
                    ExtractedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curriculum_extractions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_units_ExtractionId",
                table: "curriculum_units",
                column: "ExtractionId");

            migrationBuilder.AddForeignKey(
                name: "FK_curriculum_units_curriculum_extractions_ExtractionId",
                table: "curriculum_units",
                column: "ExtractionId",
                principalTable: "curriculum_extractions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_curriculum_units_curriculum_extractions_ExtractionId",
                table: "curriculum_units");

            migrationBuilder.DropTable(
                name: "curriculum_extractions");

            migrationBuilder.DropIndex(
                name: "IX_curriculum_units_ExtractionId",
                table: "curriculum_units");

            migrationBuilder.DropColumn(
                name: "ExtractionId",
                table: "curriculum_units");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExtractedAt",
                table: "curriculum_units",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "PdfSourceUrl",
                table: "curriculum_units",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokensUsed",
                table: "curriculum_units",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
