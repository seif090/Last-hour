using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Seeding;

/// <summary>
/// Configures the relational shape of <see cref="SeedHistory"/>.
/// </summary>
public sealed class SeedHistoryConfiguration : IEntityTypeConfiguration<SeedHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SeedHistory> builder)
    {
        builder.ToTable("seeding_history");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.SeederType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(history => history.ExecutedAtUtc)
            .IsRequired();

        builder.HasIndex(history => history.SeederType)
            .IsUnique()
            .HasDatabaseName("IX_seeding_history_seeder_type");
    }
}
