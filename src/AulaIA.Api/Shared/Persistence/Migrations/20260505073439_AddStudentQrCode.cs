using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentQrCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "qr_code",
                table: "students",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                comment: "UUID sin guiones — payload del código QR para asistencia");

            migrationBuilder.CreateIndex(
                name: "ix_students_qr_code",
                table: "students",
                column: "qr_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_students_qr_code",
                table: "students");

            migrationBuilder.DropColumn(
                name: "qr_code",
                table: "students");
        }
    }
}
