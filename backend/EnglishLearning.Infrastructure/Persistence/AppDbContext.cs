using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<ConversationSession> ConversationSessions => Set<ConversationSession>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Vocabulary> Vocabulary => Set<Vocabulary>();
    public DbSet<LearnerAssessment> LearnerAssessments => Set<LearnerAssessment>();
    public DbSet<LearningObjective> LearningObjectives => Set<LearningObjective>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
    public DbSet<StudyPlanPhase> StudyPlanPhases => Set<StudyPlanPhase>();
    public DbSet<LearnerSkillMastery> LearnerSkillMasteries => Set<LearnerSkillMastery>();
    public DbSet<LearningActivityHistory> LearningActivityHistories => Set<LearningActivityHistory>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
