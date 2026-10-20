using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("assessments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.SessionId)
            .IsRequired();

        builder.HasIndex(a => a.SessionId)
            .IsUnique();

        builder.Property(a => a.GrammarScore)
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.Property(a => a.VocabularyScore)
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.Property(a => a.FluencyScore)
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.Property(a => a.PronunciationScore)
            .HasColumnType("numeric(5,2)");

        builder.Property(a => a.OverallScore)
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.Property(a => a.Feedback)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.StrengthsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(a => a.AreasForImprovementJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(a => a.GrammarCorrectionsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(a => a.VocabularySuggestionsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(a => a.CreatedAt)
            .IsRequired();
    }
}
