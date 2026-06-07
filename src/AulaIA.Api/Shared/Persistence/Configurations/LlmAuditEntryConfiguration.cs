using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class LlmAuditEntryConfiguration : IEntityTypeConfiguration<LlmAuditEntry>
{
    public void Configure(EntityTypeBuilder<LlmAuditEntry> builder)
    {
        builder.ToTable("llm_audit_entries");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .UseIdentityByDefaultColumn();

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.Category)
               .HasColumnName("category")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Component)
               .HasColumnName("component")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Intent)
               .HasColumnName("intent")
               .HasMaxLength(1000);

        builder.Property(x => x.Result)
               .HasColumnName("result")
               .HasColumnType("text");

        builder.Property(x => x.ContextJson)
               .HasColumnName("context_json")
               .HasColumnType("text");

        builder.Property(x => x.IsError)
               .HasColumnName("is_error")
               .HasDefaultValue(false);

        builder.HasIndex(x => x.CreatedAt)
               .HasDatabaseName("ix_llm_audit_entries_created_at");

        builder.HasIndex(x => x.Category)
               .HasDatabaseName("ix_llm_audit_entries_category");

        builder.HasIndex(x => x.IsError)
               .HasDatabaseName("ix_llm_audit_entries_is_error");
    }
}
