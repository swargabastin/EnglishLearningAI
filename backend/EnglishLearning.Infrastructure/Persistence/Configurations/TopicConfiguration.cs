using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("topics");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.RecommendedDifficulty)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.IconKey)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        builder.HasMany(t => t.ConversationSessions)
            .WithOne(s => s.Topic)
            .HasForeignKey(s => s.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed 7 standard MVP topics (SRS Section 7)
        builder.HasData(
            new Topic
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Daily Conversation",
                Description = "Casual everyday discussions about your routines, hobbies, weather, and weekend plans.",
                RecommendedDifficulty = EnglishProficiencyLevel.A1,
                IconKey = "MessageCircle",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Topic
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Job Interview",
                Description = "Practice answering behavioral interview questions, describing your experience and career strengths.",
                RecommendedDifficulty = EnglishProficiencyLevel.B2,
                IconKey = "Briefcase",
                IsActive = true,
                SortOrder = 2,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Topic
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Travel",
                Description = "Simulate airport check-ins, asking for directions, hotel bookings, and navigating foreign cities.",
                RecommendedDifficulty = EnglishProficiencyLevel.B1,
                IconKey = "Compass",
                IsActive = true,
                SortOrder = 3,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Topic
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Shopping",
                Description = "Inquiring about sizes, prices, returns, bargaining politely, and making purchases in retail stores.",
                RecommendedDifficulty = EnglishProficiencyLevel.A2,
                IconKey = "ShoppingBag",
                IsActive = true,
                SortOrder = 4,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Topic
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Restaurant",
                Description = "Ordering meals, asking about ingredients, dietary requirements, requesting the bill, and complimenting the chef.",
                RecommendedDifficulty = EnglishProficiencyLevel.A2,
                IconKey = "UtensilsCrossed",
                IsActive = true,
                SortOrder = 5,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Topic
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Workplace",
                Description = "Participating in team meetings, giving status updates, negotiating deadlines, and collaborating with colleagues.",
                RecommendedDifficulty = EnglishProficiencyLevel.B1,
                IconKey = "Building2",
                IsActive = true,
                SortOrder = 6,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Topic
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Introducing Yourself",
                Description = "Build confidence introducing your background, interests, profession, and family in personal or networking setups.",
                RecommendedDifficulty = EnglishProficiencyLevel.A1,
                IconKey = "UserCheck",
                IsActive = true,
                SortOrder = 7,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
