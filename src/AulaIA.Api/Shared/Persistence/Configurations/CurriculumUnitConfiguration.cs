using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class CurriculumUnitConfiguration : IEntityTypeConfiguration<CurriculumUnit>
{
    public void Configure(EntityTypeBuilder<CurriculumUnit> builder)
    {
        builder.ToTable("curriculum_units");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Asignatura).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Ciclo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.UnidadNombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PdfSourceUrl).HasMaxLength(500);
        builder.Property(x => x.ValidatedBy).HasMaxLength(128);

        // JSONB columns
        builder.Property(x => x.AprendizajesEsperados).HasColumnType("jsonb");
        builder.Property(x => x.IndicadoresEvaluacion).HasColumnType("jsonb");
        builder.Property(x => x.ContenidoConceptual).HasColumnType("jsonb");
        builder.Property(x => x.ContenidoProcedimental).HasColumnType("jsonb");
        builder.Property(x => x.ContenidoActitudinal).HasColumnType("jsonb");
        builder.Property(x => x.EstrategiasSugeridas).HasColumnType("jsonb");

        builder.HasIndex(x => new { x.Asignatura, x.Nivel, x.Trimestre, x.UnidadNumero })
               .IsUnique();
    }
}
