using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Plan)
               .HasColumnName("plan")
               .HasConversion<string>()
               .HasMaxLength(20);
        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20);
        builder.Property(x => x.IsTrial).HasColumnName("is_trial");
        builder.Property(x => x.CurrentPeriodStart).HasColumnName("current_period_start");
        builder.Property(x => x.CurrentPeriodEnd).HasColumnName("current_period_end");
        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.UserId)
               .IsUnique()
               .HasDatabaseName("ix_subscriptions_user_id");

        builder.HasOne(x => x.User)
               .WithOne(u => u.Subscription)
               .HasForeignKey<Subscription>(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
