using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class CurriculumExtractionConfiguration : IEntityTypeConfiguration<CurriculumExtraction>
{
    public void Configure(EntityTypeBuilder<CurriculumExtraction> builder)
    {
        builder.ToTable("curriculum_extractions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Asignatura).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Ciclo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PdfSourceUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ModelUsed).HasMaxLength(100).IsRequired();

        builder.HasMany(x => x.Units)
               .WithOne(u => u.Extraction)
               .HasForeignKey(u => u.ExtractionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
