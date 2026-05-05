using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class EvaluationActivityConfiguration : IEntityTypeConfiguration<EvaluationActivity>
{
    public void Configure(EntityTypeBuilder<EvaluationActivity> builder)
    {
        builder.ToTable("evaluation_activities");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedNever();

        builder.Property(x => x.GroupId)
               .HasColumnName("group_id");

        builder.Property(x => x.Name)
               .HasColumnName("name")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Type)
               .HasColumnName("type")
               .HasMaxLength(100)
               .IsRequired()
               .HasComment("Ej: 'Prueba Escrita', 'Trabajo Cotidiano', 'Proyecto', 'Portafolio'");

        builder.Property(x => x.MaxScore)
               .HasColumnName("max_score")
               .HasPrecision(6, 2)
               .HasDefaultValue(100m);

        builder.Property(x => x.Percentage)
               .HasColumnName("percentage")
               .HasPrecision(5, 2)
               .HasComment("Porcentaje que representa dentro del período, ej. 20.00");

        builder.Property(x => x.DueDate)
               .HasColumnName("due_date")
               .HasColumnType("date");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.GroupId)
               .HasDatabaseName("ix_eval_activities_group_id");

        builder.HasOne(x => x.Group)
               .WithMany(g => g.EvaluationActivities)
               .HasForeignKey(x => x.GroupId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_eval_activities_group");
    }
}
