using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static AulaIA.Api.Shared.Persistence.SeedData;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedNever();

        builder.Property(x => x.Auth0Sub)
               .HasColumnName("auth0_sub")
               .HasMaxLength(128)
               .IsRequired();

        builder.Property(x => x.Email)
               .HasColumnName("email")
               .HasMaxLength(256)
               .IsRequired();

        builder.Property(x => x.FullName)
               .HasColumnName("full_name")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Role)
               .HasColumnName("role")
               .HasConversion<string>()
               .HasMaxLength(20)
               .HasDefaultValue(UserRole.Teacher);

        builder.Property(x => x.InstitutionId)
               .HasColumnName("institution_id");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.Auth0Sub)
               .IsUnique()
               .HasDatabaseName("ix_users_auth0_sub");

        builder.HasIndex(x => x.Email)
               .IsUnique()
               .HasDatabaseName("ix_users_email");

        builder.HasIndex(x => x.InstitutionId)
               .HasDatabaseName("ix_users_institution_id");

        builder.Property(x => x.ReferredByCode)
               .HasColumnName("referred_by_code")
               .HasMaxLength(32);

        builder.HasOne(x => x.Institution)
               .WithMany(i => i.Users)
               .HasForeignKey(x => x.InstitutionId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_users_institution");

        // ── Seed data ──────────────────────────────────────────────────────────────
        // Auth0Sub se actualiza después de configurar Auth0 (F0-14).
        var seed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new User
            {
                Id            = Users.Ezequiel,
                Auth0Sub      = "auth0|69fae47c268da9d7e46c6d4b",
                Email         = "ezekiell1988@hotmail.com",
                FullName      = "Ezequiel Baltodano",
                Role          = UserRole.Admin,
                InstitutionId = Institutions.LiceoAserri,
                CreatedAt     = seed
            }
        );
    }
}
