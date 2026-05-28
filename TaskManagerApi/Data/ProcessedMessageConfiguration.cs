using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagerApi.Model;

namespace TaskManagerApi.Data;

public class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.HasKey(pm => new { pm.TaskId, pm.ConsumerGroup });

    }
}