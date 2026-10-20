using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("conversation_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.SessionId)
            .IsRequired();

        builder.HasIndex(m => m.SessionId);

        builder.Property(m => m.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.MessageText)
            .IsRequired();

        builder.Property(m => m.AudioStoragePath)
            .HasMaxLength(500);

        builder.Property(m => m.SequenceNumber)
            .IsRequired();

        builder.HasIndex(m => new { m.SessionId, m.SequenceNumber });

        builder.Property(m => m.CreatedAt)
            .IsRequired();
    }
}
