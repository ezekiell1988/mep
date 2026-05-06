using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class LessonPlanConfiguration : IEntityTypeConfiguration<LessonPlan>
{
    public void Configure(EntityTypeBuilder<LessonPlan> builder)
    {
        builder.ToTable("lesson_plans");
        builder.HasKey(x => x.Id);

        // Explicit snake_case column names (InitialCreate usó snake_case manual;
        // AddCurriculumAndPlanning generó PascalCase — se corrige aquí con HasColumnName).
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.TeacherSub).HasColumnName("teacher_sub").HasMaxLength(128).IsRequired();
        builder.Property(x => x.Asignatura).HasColumnName("asignatura").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Nivel).HasColumnName("nivel");
        builder.Property(x => x.Trimestre).HasColumnName("trimestre");
        builder.Property(x => x.AnioLectivo).HasColumnName("anio_lectivo");
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin");
        builder.Property(x => x.LeccionesPorSemana).HasColumnName("lecciones_por_semana");
        builder.Property(x => x.ContenidoGenerado).HasColumnName("contenido_generado");
        builder.Property(x => x.ArchivoBlobUrl).HasColumnName("archivo_blob_url").HasMaxLength(500);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.GeneratedAt).HasColumnName("generated_at");

        builder.HasOne(x => x.Group)
               .WithMany()
               .HasForeignKey(x => x.GroupId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TeacherSub);
        builder.HasIndex(x => new { x.GroupId, x.Trimestre, x.AnioLectivo });
    }
}
