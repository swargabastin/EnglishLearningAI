using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class LearnerAssessmentConfiguration : IEntityTypeConfiguration<LearnerAssessment>
{
    public void Configure(EntityTypeBuilder<LearnerAssessment> builder)
    {
        builder.ToTable("learner_assessments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).IsRequired();
        builder.HasIndex(a => a.UserId);

        builder.Property(a => a.Category).HasConversion<int>().IsRequired();
        builder.Property(a => a.CefrLevel).HasConversion<int>().IsRequired();
        builder.Property(a => a.GrammarScore).HasPrecision(5, 2).IsRequired();
        builder.Property(a => a.VocabularyScore).HasPrecision(5, 2).IsRequired();
        builder.Property(a => a.SentenceScore).HasPrecision(5, 2).IsRequired();
        builder.Property(a => a.ReadingScore).HasPrecision(5, 2).IsRequired();
        builder.Property(a => a.SpeakingScore).HasPrecision(5, 2).IsRequired(false);
        builder.Property(a => a.OverallScore).HasPrecision(5, 2).IsRequired();
        builder.Property(a => a.Confidence).HasMaxLength(50).IsRequired();
        builder.Property(a => a.ResponsesJson).HasColumnType("text").IsRequired();
        builder.Property(a => a.WeaknessesJson).HasColumnType("text").IsRequired();
        builder.Property(a => a.ReassessmentRecommended).IsRequired();
        builder.Property(a => a.CompletedAt).IsRequired();

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LearningObjectiveConfiguration : IEntityTypeConfiguration<LearningObjective>
{
    public void Configure(EntityTypeBuilder<LearningObjective> builder)
    {
        builder.ToTable("learning_objectives");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(o => o.Code).IsUnique();

        builder.Property(o => o.Title).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Description).HasMaxLength(1000).IsRequired();
        builder.Property(o => o.SkillCategory).HasConversion<int>().IsRequired();
        builder.Property(o => o.Difficulty).HasConversion<int>().IsRequired();
        builder.Property(o => o.CefrLevel).HasConversion<int>().IsRequired();
        builder.Property(o => o.PrerequisiteCodesJson).HasColumnType("text").IsRequired();
        builder.Property(o => o.MasteryScoreThreshold).HasPrecision(5, 2).IsRequired();
        builder.Property(o => o.SuggestedDurationMinutes).IsRequired();
        builder.Property(o => o.SortOrder).IsRequired();
        builder.Property(o => o.IsActive).IsRequired();
    }
}

public class StudyPlanConfiguration : IEntityTypeConfiguration<StudyPlan>
{
    public void Configure(EntityTypeBuilder<StudyPlan> builder)
    {
        builder.ToTable("study_plans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.PlanDate).HasMaxLength(10).IsRequired(); // YYYY-MM-DD
        builder.HasIndex(p => new { p.UserId, p.PlanDate }).IsUnique();

        builder.Property(p => p.ObjectiveId).IsRequired();
        builder.Property(p => p.ObjectiveCode).HasMaxLength(50).IsRequired();
        builder.Property(p => p.TargetMinutes).IsRequired();
        builder.Property(p => p.ActualMinutes).IsRequired();
        builder.Property(p => p.Status).HasConversion<int>().IsRequired();
        builder.Property(p => p.OverallScore).HasPrecision(5, 2).IsRequired(false);
        builder.Property(p => p.CompletedAt).IsRequired(false);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Objective)
            .WithMany()
            .HasForeignKey(p => p.ObjectiveId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Phases)
            .WithOne(ph => ph.StudyPlan)
            .HasForeignKey(ph => ph.StudyPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StudyPlanPhaseConfiguration : IEntityTypeConfiguration<StudyPlanPhase>
{
    public void Configure(EntityTypeBuilder<StudyPlanPhase> builder)
    {
        builder.ToTable("study_plan_phases");
        builder.HasKey(ph => ph.Id);

        builder.Property(ph => ph.StudyPlanId).IsRequired();
        builder.HasIndex(ph => new { ph.StudyPlanId, ph.PhaseOrder }).IsUnique();

        builder.Property(ph => ph.PhaseOrder).IsRequired();
        builder.Property(ph => ph.PhaseType).HasConversion<int>().IsRequired();
        builder.Property(ph => ph.ObjectiveTitle).HasMaxLength(200).IsRequired();
        builder.Property(ph => ph.LessonContentJson).HasColumnType("text").IsRequired();
        builder.Property(ph => ph.TasksJson).HasColumnType("text").IsRequired();
        builder.Property(ph => ph.PlannedDurationMinutes).IsRequired();
        builder.Property(ph => ph.ActualDurationMinutes).IsRequired();
        builder.Property(ph => ph.Attempts).IsRequired();
        builder.Property(ph => ph.Score).HasPrecision(5, 2).IsRequired(false);
        builder.Property(ph => ph.Status).HasConversion<int>().IsRequired();
        builder.Property(ph => ph.Feedback).HasColumnType("text").IsRequired(false);
        builder.Property(ph => ph.CompletedAt).IsRequired(false);
    }
}

public class LearnerSkillMasteryConfiguration : IEntityTypeConfiguration<LearnerSkillMastery>
{
    public void Configure(EntityTypeBuilder<LearnerSkillMastery> builder)
    {
        builder.ToTable("learner_skill_masteries");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.UserId).IsRequired();
        builder.Property(m => m.ObjectiveCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => new { m.UserId, m.ObjectiveCode }).IsUnique();

        builder.Property(m => m.MasteryScore).HasPrecision(5, 2).IsRequired();
        builder.Property(m => m.AttemptCount).IsRequired();
        builder.Property(m => m.IsMastered).IsRequired();
        builder.Property(m => m.LastPracticedAt).IsRequired();
        builder.Property(m => m.NextRevisionDueDate).IsRequired(false);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LearningActivityHistoryConfiguration : IEntityTypeConfiguration<LearningActivityHistory>
{
    public void Configure(EntityTypeBuilder<LearningActivityHistory> builder)
    {
        builder.ToTable("learning_activity_histories");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.UserId).IsRequired();
        builder.HasIndex(h => h.UserId);

        builder.Property(h => h.StudyPlanId).IsRequired();
        builder.Property(h => h.PhaseId).IsRequired();
        builder.Property(h => h.TaskIdentifier).HasMaxLength(100).IsRequired();
        builder.Property(h => h.Score).HasPrecision(5, 2).IsRequired(false);
        builder.Property(h => h.DurationSeconds).IsRequired();
        builder.Property(h => h.AnswerPayloadJson).HasColumnType("text").IsRequired();
        builder.Property(h => h.IsCorrect).IsRequired();

        builder.HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
