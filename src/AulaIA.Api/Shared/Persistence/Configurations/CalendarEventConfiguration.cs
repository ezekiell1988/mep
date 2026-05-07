using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaIA.Api.Shared.Persistence.Configurations;

public sealed class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable("calendar_events");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.Date).HasColumnName("date");
        builder.Property(x => x.EndDate).HasColumnName("end_date");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Type)
               .HasColumnName("type")
               .HasConversion<string>()
               .HasMaxLength(30);
        builder.Property(x => x.SchoolYear).HasColumnName("school_year");
        builder.Property(x => x.CreatedByAuth0Sub)
               .HasColumnName("created_by_auth0_sub")
               .HasMaxLength(128);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.Group)
               .WithMany()
               .HasForeignKey(x => x.GroupId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false);

        builder.HasIndex(x => new { x.GroupId, x.Date });
        builder.HasIndex(x => new { x.SchoolYear, x.GroupId });
    }
}
