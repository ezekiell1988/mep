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

        builder.Property(x => x.TeacherSub).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Asignatura).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ArchivoBlobUrl).HasMaxLength(500);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);

        builder.HasOne(x => x.Group)
               .WithMany()
               .HasForeignKey(x => x.GroupId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TeacherSub);
        builder.HasIndex(x => new { x.GroupId, x.Trimestre, x.AnioLectivo });
    }
}
