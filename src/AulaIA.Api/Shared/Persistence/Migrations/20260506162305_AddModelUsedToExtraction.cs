using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModelUsedToExtraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelUsed",
                table: "curriculum_extractions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelUsed",
                table: "curriculum_extractions");
        }
    }
}
