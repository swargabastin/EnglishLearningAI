using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class ConversationSessionConfiguration : IEntityTypeConfiguration<ConversationSession>
{
    public void Configure(EntityTypeBuilder<ConversationSession> builder)
    {
        builder.ToTable("conversation_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.HasIndex(s => s.UserId);

        builder.Property(s => s.TopicId)
            .IsRequired();

        builder.HasIndex(s => s.TopicId);

        builder.Property(s => s.EnglishLevel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.StartedAt)
            .IsRequired();

        builder.HasIndex(s => s.StartedAt);

        builder.Property(s => s.EndedAt);

        builder.Property(s => s.DurationSeconds)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.OverallScore)
            .HasColumnType("numeric(5,2)");

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Topic)
            .WithMany()
            .HasForeignKey(s => s.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Messages)
            .WithOne(m => m.Session)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Assessment)
            .WithOne(a => a.Session)
            .HasForeignKey<Assessment>(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
