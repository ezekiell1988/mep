using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class PaymentRequestConfiguration : IEntityTypeConfiguration<PaymentRequest>
{
    public void Configure(EntityTypeBuilder<PaymentRequest> builder)
    {
        builder.ToTable("payment_requests");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Plan)
               .HasColumnName("plan")
               .HasConversion<string>()
               .HasMaxLength(20);
        builder.Property(x => x.AmountUsd)
               .HasColumnName("amount_usd")
               .HasColumnType("numeric(10,2)");
        builder.Property(x => x.AmountCrc)
               .HasColumnName("amount_crc")
               .HasColumnType("numeric(12,2)");
        builder.Property(x => x.ExchangeRateUsed)
               .HasColumnName("exchange_rate_used")
               .HasColumnType("numeric(10,4)");
        builder.Property(x => x.ReferenceCode)
               .HasColumnName("reference_code")
               .HasMaxLength(20);
        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20);
        builder.Property(x => x.VoucherBlobPath)
               .HasColumnName("voucher_blob_path")
               .HasMaxLength(500);
        builder.Property(x => x.AdminNote)
               .HasColumnName("admin_note")
               .HasMaxLength(500);
        builder.Property(x => x.ReviewedByAuth0Sub)
               .HasColumnName("reviewed_by_auth0_sub")
               .HasMaxLength(128);
        builder.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.ReferenceCode)
               .IsUnique()
               .HasDatabaseName("ix_payment_requests_reference_code");

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("ix_payment_requests_user_id");

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
