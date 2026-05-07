using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class AccommodationConfiguration : IEntityTypeConfiguration<Accommodation>
{
    public void Configure(EntityTypeBuilder<Accommodation> builder)
    {
        builder.ToTable("accommodations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.Type)
               .HasColumnName("type")
               .HasConversion<string>()
               .HasMaxLength(10)
               .IsRequired();
        builder.Property(x => x.Diagnostico)
               .HasColumnName("diagnostico")
               .HasMaxLength(500)
               .IsRequired();
        builder.Property(x => x.CondicionEspecial)
               .HasColumnName("condicion_especial")
               .HasMaxLength(300);
        builder.Property(x => x.EstrategiasMediacion).HasColumnName("estrategias_mediacion");
        builder.Property(x => x.EstrategiasEvaluacion).HasColumnName("estrategias_evaluacion");
        builder.Property(x => x.Observaciones)
               .HasColumnName("observaciones")
               .HasMaxLength(1000);
        builder.Property(x => x.PropuestaGenerada).HasColumnName("propuesta_generada");
        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20);
        builder.Property(x => x.GeneratedAt).HasColumnName("generated_at");
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(500);
        builder.Property(x => x.CreatedByAuth0Sub)
               .HasColumnName("created_by_auth0_sub")
               .HasMaxLength(128);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        // Un estudiante tiene como máximo una adecuación activa por grupo
        builder.HasIndex(x => new { x.StudentId, x.GroupId }).IsUnique();
        builder.HasIndex(x => x.GroupId);

        builder.HasOne(x => x.Student)
               .WithMany()
               .HasForeignKey(x => x.StudentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Group)
               .WithMany()
               .HasForeignKey(x => x.GroupId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
