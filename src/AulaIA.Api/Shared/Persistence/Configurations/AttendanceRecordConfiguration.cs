using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("attendance_records");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedNever();

        builder.Property(x => x.GroupId)
               .HasColumnName("group_id");

        builder.Property(x => x.StudentId)
               .HasColumnName("student_id");

        builder.Property(x => x.Date)
               .HasColumnName("date")
               .HasColumnType("date");

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20)
               .HasDefaultValue(AttendanceStatus.Present)
               .HasComment("Present | Absent | Late | Justified");

        builder.Property(x => x.Notes)
               .HasColumnName("notes")
               .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();

        // Un registro por alumno por día
        builder.HasIndex(x => new { x.StudentId, x.Date })
               .IsUnique()
               .HasDatabaseName("ix_attendance_student_date");

        // Consulta frecuente: todos los alumnos de un grupo en una fecha
        builder.HasIndex(x => new { x.GroupId, x.Date })
               .HasDatabaseName("ix_attendance_group_date");

        builder.HasOne(x => x.Group)
               .WithMany(g => g.AttendanceRecords)
               .HasForeignKey(x => x.GroupId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_attendance_group");

        builder.HasOne(x => x.Student)
               .WithMany(s => s.AttendanceRecords)
               .HasForeignKey(x => x.StudentId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_attendance_student");
    }
}
