using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static AulaIA.Api.Shared.Persistence.SeedData;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> builder)
    {
        builder.ToTable("institutions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedNever();

        builder.Property(x => x.Name)
               .HasColumnName("name")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.CircuitCode)
               .HasColumnName("circuit_code")
               .HasMaxLength(20);

        builder.Property(x => x.RegionCode)
               .HasColumnName("region_code")
               .HasMaxLength(20);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.Name).HasDatabaseName("ix_institutions_name");

        // ── Seed data — colegios públicos MEP (fuente: mep.go.cr/oficinas) ──
        var seed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            // DRE Desamparados — Circuito 06 (zona beachhead: Aserrí)
            new Institution { Id = Institutions.LiceoAserri,          Name = "Liceo de Aserrí",                                   CircuitCode = "06", RegionCode = RegionCodes.Desamparados,   CreatedAt = seed },
            new Institution { Id = Institutions.CtpAserri,            Name = "Colegio Técnico Profesional de Aserrí",             CircuitCode = "06", RegionCode = RegionCodes.Desamparados,   CreatedAt = seed },
            new Institution { Id = Institutions.NocturnoAserri,       Name = "Colegio Nocturno de Aserrí",                        CircuitCode = "06", RegionCode = RegionCodes.Desamparados,   CreatedAt = seed },
            new Institution { Id = Institutions.LiceoSanGabriel,      Name = "Liceo San Gabriel",                                 CircuitCode = "06", RegionCode = RegionCodes.Desamparados,   CreatedAt = seed },
            // DRE Desamparados — otros circuitos
            new Institution { Id = Institutions.CtpDesamparados,      Name = "Colegio Técnico Profesional de Desamparados",       CircuitCode = "01", RegionCode = RegionCodes.Desamparados,   CreatedAt = seed },
            new Institution { Id = Institutions.LiceoDesamparados,    Name = "Liceo de Desamparados",                             CircuitCode = "01", RegionCode = RegionCodes.Desamparados,   CreatedAt = seed },
            new Institution { Id = Institutions.LiceoManuelBenavides, Name = "Liceo Ing. Manuel Benavides",                       CircuitCode = "03", RegionCode = RegionCodes.Desamparados,   CreatedAt = seed },
            // DRE San José Central
            new Institution { Id = Institutions.LiceoCostaRica,       Name = "Liceo de Costa Rica",                               CircuitCode = "01", RegionCode = RegionCodes.SanJoseCentral, CreatedAt = seed },
            new Institution { Id = Institutions.ColegioSenoritas,     Name = "Colegio de Señoritas",                              CircuitCode = "01", RegionCode = RegionCodes.SanJoseCentral, CreatedAt = seed },
            new Institution { Id = Institutions.LiceoJulioFonseca,    Name = "Liceo Julio Fonseca",                               CircuitCode = "02", RegionCode = RegionCodes.SanJoseCentral, CreatedAt = seed },
            new Institution { Id = Institutions.LiceoVargasCalvo,     Name = "Liceo José Joaquín Vargas Calvo",                   CircuitCode = "03", RegionCode = RegionCodes.SanJoseCentral, CreatedAt = seed },
            // DRE San José Norte
            new Institution { Id = Institutions.LiceoGuadalupe,       Name = "Liceo de Guadalupe",                                CircuitCode = "01", RegionCode = RegionCodes.SanJoseNorte,   CreatedAt = seed },
            new Institution { Id = Institutions.CtpMoravia,           Name = "Colegio Técnico Profesional de Moravia",            CircuitCode = "02", RegionCode = RegionCodes.SanJoseNorte,   CreatedAt = seed },
            // DRE Heredia
            new Institution { Id = Institutions.LiceoHeredia,         Name = "Liceo de Heredia",                                  CircuitCode = "01", RegionCode = RegionCodes.Heredia,         CreatedAt = seed },
            new Institution { Id = Institutions.LiceoDanielOduber,    Name = "Liceo Daniel Oduber Quirós",                        CircuitCode = "02", RegionCode = RegionCodes.Heredia,         CreatedAt = seed },
            // DRE Cartago
            new Institution { Id = Institutions.LiceoCartago,         Name = "Liceo de Cartago",                                  CircuitCode = "01", RegionCode = RegionCodes.Cartago,         CreatedAt = seed },
            new Institution { Id = Institutions.CtpCartago,           Name = "Colegio Técnico Profesional de Cartago",            CircuitCode = "01", RegionCode = RegionCodes.Cartago,         CreatedAt = seed },
            // DRE Alajuela
            new Institution { Id = Institutions.LiceoAlajuela,        Name = "Liceo de Alajuela",                                 CircuitCode = "01", RegionCode = RegionCodes.Alajuela,        CreatedAt = seed },
            new Institution { Id = Institutions.CtpAlajuela,          Name = "Colegio Técnico Profesional Jesús Ocaña Rojas",     CircuitCode = "01", RegionCode = RegionCodes.Alajuela,        CreatedAt = seed },
            // DRE San José Oeste
            new Institution { Id = Institutions.LiceoEscazu,          Name = "Liceo de Escazú",                                   CircuitCode = "01", RegionCode = RegionCodes.SanJoseOeste,    CreatedAt = seed }
        );
    }
}
