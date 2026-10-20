using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class VocabularyConfiguration : IEntityTypeConfiguration<Vocabulary>
{
    public void Configure(EntityTypeBuilder<Vocabulary> builder)
    {
        builder.ToTable("vocabulary");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.UserId)
            .IsRequired();

        builder.HasIndex(v => v.UserId);

        builder.Property(v => v.Word)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(v => new { v.UserId, v.Word });

        builder.Property(v => v.Meaning)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.Example)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(v => v.MasteryLevel)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .IsRequired();

        builder.HasOne(v => v.ContextSession)
            .WithMany()
            .HasForeignKey(v => v.ContextSessionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
