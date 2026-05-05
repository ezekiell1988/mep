using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedNever();

        builder.Property(x => x.FullName)
               .HasColumnName("full_name")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.StudentCode)
               .HasColumnName("student_code")
               .HasMaxLength(50)
               .IsRequired()
               .HasComment("Número de expediente MEP");

        builder.Property(x => x.QrCode)
               .HasColumnName("qr_code")
               .HasMaxLength(32)
               .IsRequired()
               .HasComment("UUID sin guiones — payload del código QR para asistencia");

        builder.Property(x => x.GroupId)
               .HasColumnName("group_id");

        builder.Property(x => x.IsActive)
               .HasColumnName("is_active")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();

        // Código de estudiante único dentro de un grupo
        builder.HasIndex(x => new { x.StudentCode, x.GroupId })
               .IsUnique()
               .HasDatabaseName("ix_students_code_group");

        // QR único globalmente — el scanner resuelve estudiante sin saber el grupo
        builder.HasIndex(x => x.QrCode)
               .IsUnique()
               .HasDatabaseName("ix_students_qr_code");

        builder.HasIndex(x => x.GroupId)
               .HasDatabaseName("ix_students_group_id");

        builder.HasOne(x => x.Group)
               .WithMany(g => g.Students)
               .HasForeignKey(x => x.GroupId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_students_group");
    }
}
