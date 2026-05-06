using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameLessonPlanColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lesson_plans_groups_GroupId",
                table: "lesson_plans");

            migrationBuilder.RenameColumn(
                name: "Trimestre",
                table: "lesson_plans",
                newName: "trimestre");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "lesson_plans",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Nivel",
                table: "lesson_plans",
                newName: "nivel");

            migrationBuilder.RenameColumn(
                name: "Asignatura",
                table: "lesson_plans",
                newName: "asignatura");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "lesson_plans",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TeacherSub",
                table: "lesson_plans",
                newName: "teacher_sub");

            migrationBuilder.RenameColumn(
                name: "LeccionesPorSemana",
                table: "lesson_plans",
                newName: "lecciones_por_semana");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "lesson_plans",
                newName: "group_id");

            migrationBuilder.RenameColumn(
                name: "GeneratedAt",
                table: "lesson_plans",
                newName: "generated_at");

            migrationBuilder.RenameColumn(
                name: "FechaInicio",
                table: "lesson_plans",
                newName: "fecha_inicio");

            migrationBuilder.RenameColumn(
                name: "FechaFin",
                table: "lesson_plans",
                newName: "fecha_fin");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "lesson_plans",
                newName: "error_message");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "lesson_plans",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ContenidoGenerado",
                table: "lesson_plans",
                newName: "contenido_generado");

            migrationBuilder.RenameColumn(
                name: "ArchivoBlobUrl",
                table: "lesson_plans",
                newName: "archivo_blob_url");

            migrationBuilder.RenameColumn(
                name: "AnioLectivo",
                table: "lesson_plans",
                newName: "anio_lectivo");

            migrationBuilder.RenameIndex(
                name: "IX_lesson_plans_TeacherSub",
                table: "lesson_plans",
                newName: "IX_lesson_plans_teacher_sub");

            migrationBuilder.RenameIndex(
                name: "IX_lesson_plans_GroupId_Trimestre_AnioLectivo",
                table: "lesson_plans",
                newName: "IX_lesson_plans_group_id_trimestre_anio_lectivo");

            migrationBuilder.AddForeignKey(
                name: "FK_lesson_plans_groups_group_id",
                table: "lesson_plans",
                column: "group_id",
                principalTable: "groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lesson_plans_groups_group_id",
                table: "lesson_plans");

            migrationBuilder.RenameColumn(
                name: "trimestre",
                table: "lesson_plans",
                newName: "Trimestre");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "lesson_plans",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "nivel",
                table: "lesson_plans",
                newName: "Nivel");

            migrationBuilder.RenameColumn(
                name: "asignatura",
                table: "lesson_plans",
                newName: "Asignatura");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "lesson_plans",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "teacher_sub",
                table: "lesson_plans",
                newName: "TeacherSub");

            migrationBuilder.RenameColumn(
                name: "lecciones_por_semana",
                table: "lesson_plans",
                newName: "LeccionesPorSemana");

            migrationBuilder.RenameColumn(
                name: "group_id",
                table: "lesson_plans",
                newName: "GroupId");

            migrationBuilder.RenameColumn(
                name: "generated_at",
                table: "lesson_plans",
                newName: "GeneratedAt");

            migrationBuilder.RenameColumn(
                name: "fecha_inicio",
                table: "lesson_plans",
                newName: "FechaInicio");

            migrationBuilder.RenameColumn(
                name: "fecha_fin",
                table: "lesson_plans",
                newName: "FechaFin");

            migrationBuilder.RenameColumn(
                name: "error_message",
                table: "lesson_plans",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "lesson_plans",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "contenido_generado",
                table: "lesson_plans",
                newName: "ContenidoGenerado");

            migrationBuilder.RenameColumn(
                name: "archivo_blob_url",
                table: "lesson_plans",
                newName: "ArchivoBlobUrl");

            migrationBuilder.RenameColumn(
                name: "anio_lectivo",
                table: "lesson_plans",
                newName: "AnioLectivo");

            migrationBuilder.RenameIndex(
                name: "IX_lesson_plans_teacher_sub",
                table: "lesson_plans",
                newName: "IX_lesson_plans_TeacherSub");

            migrationBuilder.RenameIndex(
                name: "IX_lesson_plans_group_id_trimestre_anio_lectivo",
                table: "lesson_plans",
                newName: "IX_lesson_plans_GroupId_Trimestre_AnioLectivo");

            migrationBuilder.AddForeignKey(
                name: "FK_lesson_plans_groups_GroupId",
                table: "lesson_plans",
                column: "GroupId",
                principalTable: "groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
