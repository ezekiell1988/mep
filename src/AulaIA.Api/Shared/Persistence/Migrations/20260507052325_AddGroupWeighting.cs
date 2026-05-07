using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupWeighting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "pct_cotidiano",
                table: "groups",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 20m,
                comment: "% Trabajo Cotidiano (MEP default 20)");

            migrationBuilder.AddColumn<decimal>(
                name: "pct_extraclase",
                table: "groups",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 20m,
                comment: "% Trabajo Extraclase (MEP default 20)");

            migrationBuilder.AddColumn<decimal>(
                name: "pct_otros",
                table: "groups",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 15m,
                comment: "% Otros (MEP default 15)");

            migrationBuilder.AddColumn<decimal>(
                name: "pct_pruebas",
                table: "groups",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 45m,
                comment: "% Pruebas y Exámenes (MEP default 45)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pct_cotidiano",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "pct_extraclase",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "pct_otros",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "pct_pruebas",
                table: "groups");
        }
    }
}
