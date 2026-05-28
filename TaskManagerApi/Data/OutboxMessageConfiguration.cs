using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagerApi.Model;

namespace TaskManagerApi.Data;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.Property(o => o.EventType).HasConversion<string>();

        builder.Property(o => o.Payload).HasColumnType("nvarchar(max)");

        builder.HasKey(o => o.Id);

        builder.HasIndex(o => o.IsPublished);

    }
}