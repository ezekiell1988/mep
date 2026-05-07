using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.ToTable("commissions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(x => x.ReferralCodeId).HasColumnName("referral_code_id");
        builder.Property(x => x.ReferredUserId).HasColumnName("referred_user_id");
        builder.Property(x => x.Month).HasColumnName("month");
        builder.Property(x => x.GrossRevenueCrc)
               .HasColumnName("gross_revenue_crc")
               .HasColumnType("numeric(14,2)");
        builder.Property(x => x.InfraCostCrc)
               .HasColumnName("infra_cost_crc")
               .HasColumnType("numeric(14,2)");
        builder.Property(x => x.BaseAmountCrc)
               .HasColumnName("base_amount_crc")
               .HasColumnType("numeric(14,2)");
        builder.Property(x => x.CommissionRate)
               .HasColumnName("commission_rate")
               .HasColumnType("numeric(5,4)");
        builder.Property(x => x.CommissionAmountCrc)
               .HasColumnName("commission_amount_crc")
               .HasColumnType("numeric(14,2)");
        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20);
        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();

        // Una comisión por referido por mes
        builder.HasIndex(x => new { x.ReferralCodeId, x.ReferredUserId, x.Month })
               .IsUnique()
               .HasDatabaseName("ix_commissions_referral_user_month");

        builder.HasOne(x => x.ReferralCode)
               .WithMany(r => r.Commissions)
               .HasForeignKey(x => x.ReferralCodeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReferredUser)
               .WithMany()
               .HasForeignKey(x => x.ReferredUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
