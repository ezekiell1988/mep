using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTokensUsedToCurriculumUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokensUsed",
                table: "curriculum_units",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokensUsed",
                table: "curriculum_units");
        }
    }
}
