using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(x => x.Date).HasColumnName("date");
        builder.Property(x => x.UsdToCrc)
               .HasColumnName("usd_to_crc")
               .HasColumnType("numeric(10,4)");
        builder.Property(x => x.Source)
               .HasColumnName("source")
               .HasMaxLength(20);
        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.Date)
               .IsUnique()
               .HasDatabaseName("ix_exchange_rates_date");
    }
}
