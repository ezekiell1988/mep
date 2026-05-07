using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("grades");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedNever();

        builder.Property(x => x.GroupId)
               .HasColumnName("group_id");

        builder.Property(x => x.ActivityId)
               .HasColumnName("activity_id");

        builder.Property(x => x.StudentId)
               .HasColumnName("student_id");

        builder.Property(x => x.Score)
               .HasColumnName("score")
               .HasPrecision(6, 2)
               .HasComment("Nota obtenida, ej. 87.50");

        builder.Property(x => x.Comments)
               .HasColumnName("comments")
               .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at")
               .HasDefaultValueSql("now()");

        // Una calificación por alumno por actividad
        builder.HasIndex(x => new { x.ActivityId, x.StudentId })
               .IsUnique()
               .HasDatabaseName("ix_grades_activity_student");

        builder.HasIndex(x => x.GroupId)
               .HasDatabaseName("ix_grades_group_id");

        builder.HasIndex(x => x.StudentId)
               .HasDatabaseName("ix_grades_student_id");

        builder.HasOne(x => x.Activity)
               .WithMany(a => a.Grades)
               .HasForeignKey(x => x.ActivityId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_grades_activity");

        builder.HasOne(x => x.Student)
               .WithMany(s => s.Grades)
               .HasForeignKey(x => x.StudentId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_grades_student");
    }
}
