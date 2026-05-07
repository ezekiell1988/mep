using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedNever();

        builder.Property(x => x.Name)
               .HasColumnName("name")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Level)
               .HasColumnName("level")
               .HasMaxLength(50)
               .IsRequired()
               .HasComment("Año/nivel escolar, ej. '7° Año'");

        builder.Property(x => x.Subject)
               .HasColumnName("subject")
               .HasMaxLength(100)
               .IsRequired()
               .HasComment("Materia, ej. 'Matemáticas'");

        builder.Property(x => x.SchoolYear)
               .HasColumnName("school_year")
               .HasDefaultValue(DateTime.UtcNow.Year);

        builder.Property(x => x.TeacherId)
               .HasColumnName("teacher_id");

        builder.Property(x => x.TeacherSub)
               .HasColumnName("teacher_sub")
               .HasMaxLength(128)
               .IsRequired()
               .HasDefaultValue("")
               .HasComment("Auth0 sub del docente — usado por PowerSync Sync Rules para filtrar sin casts de UUID.");

        builder.Property(x => x.InstitutionId)
               .HasColumnName("institution_id");

        builder.Property(x => x.IsActive)
               .HasColumnName("is_active")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.PctCotidiano)
               .HasColumnName("pct_cotidiano")
               .HasColumnType("decimal(5,2)")
               .HasDefaultValue(20m)
               .HasComment("% Trabajo Cotidiano (MEP default 20)");

        builder.Property(x => x.PctPruebas)
               .HasColumnName("pct_pruebas")
               .HasColumnType("decimal(5,2)")
               .HasDefaultValue(45m)
               .HasComment("% Pruebas y Exámenes (MEP default 45)");

        builder.Property(x => x.PctExtraclase)
               .HasColumnName("pct_extraclase")
               .HasColumnType("decimal(5,2)")
               .HasDefaultValue(20m)
               .HasComment("% Trabajo Extraclase (MEP default 20)");

        builder.Property(x => x.PctOtros)
               .HasColumnName("pct_otros")
               .HasColumnType("decimal(5,2)")
               .HasDefaultValue(15m)
               .HasComment("% Otros (MEP default 15)");

        builder.HasIndex(x => new { x.TeacherId, x.SchoolYear })
               .HasDatabaseName("ix_groups_teacher_year");

        builder.HasIndex(x => x.InstitutionId)
               .HasDatabaseName("ix_groups_institution_id");

        builder.HasOne(x => x.Teacher)
               .WithMany(u => u.Groups)
               .HasForeignKey(x => x.TeacherId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_groups_teacher");

        builder.HasOne(x => x.Institution)
               .WithMany(i => i.Groups)
               .HasForeignKey(x => x.InstitutionId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_groups_institution");
    }
}
