using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class CurriculumSourceConfiguration : IEntityTypeConfiguration<CurriculumSource>
{
    public void Configure(EntityTypeBuilder<CurriculumSource> builder)
    {
        builder.ToTable("curriculum_sources");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Asignatura).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Ciclo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MepUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.LastEtag).HasMaxLength(200);

        builder.HasIndex(x => new { x.Asignatura, x.Ciclo }).IsUnique();
    }
}
