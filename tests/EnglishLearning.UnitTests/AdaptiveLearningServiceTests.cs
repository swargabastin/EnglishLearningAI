using System.Text.Json;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EnglishLearning.UnitTests;

public class AdaptiveLearningServiceTests
{
    private readonly Mock<IAdaptiveLearningRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AdaptiveLearningService _service;

    private readonly Guid _userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _otherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public AdaptiveLearningServiceTests()
    {
        _repositoryMock = new Mock<IAdaptiveLearningRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_userId);

        var user = new User
        {
            Id = _userId,
            EnglishLevel = EnglishProficiencyLevel.B1,
            LearningGoal = LearningGoal.SpeakingFluency
        };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _service = new AdaptiveLearningService(
            _repositoryMock.Object,
            _currentUserServiceMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task GetDiagnosticQuestionsAsync_ShouldReturnValidQuestionsCoveringAllFiveSkills()
    {
        // Act
        var questions = await _service.GetDiagnosticQuestionsAsync();

        // Assert
        questions.Should().NotBeNullOrEmpty();
        var skillCategories = questions.Select(q => q.SkillCategory).Distinct().ToList();
        skillCategories.Should().Contain(new[] { "Grammar", "Vocabulary", "SentenceConstruction", "Reading", "Speaking" });
    }

    [Fact]
    public async Task SubmitAssessmentAsync_WithHighScores_ShouldAssignAdvancedCategoryAndC1()
    {
        // Arrange: All non-speaking questions answered correctly without speech audio
        var questions = await _service.GetDiagnosticQuestionsAsync();
        var nonSpeakingQuestions = questions.Where(q => q.SkillCategory != "Speaking").ToList();
        var answers = nonSpeakingQuestions.Select(q => new QuestionAnswerDto(
            q.Id,
            q.CorrectOptionIndex,
            null,
            10
        )).ToList();

        var request = new SubmitAssessmentRequest(answers, null, null);

        // Act
        var result = await _service.SubmitAssessmentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be("Advanced");
        result.CefrLevel.Should().Be("C2");
        result.OverallScore.Should().Be(100m);
        result.IsSpeakingAssessed.Should().BeFalse();
        result.SpeakingScore.Should().BeNull(); // Speaking is never inferred from missing audio!

        _repositoryMock.Verify(r => r.AddAssessmentAsync(It.IsAny<LearnerAssessment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAssessmentAsync_WithLowScores_ShouldAssignBasicCategoryAndDetectWeaknesses()
    {
        // Arrange: All questions answered incorrectly
        var questions = await _service.GetDiagnosticQuestionsAsync();
        var answers = questions.Select(q => new QuestionAnswerDto(
            q.Id,
            (q.CorrectOptionIndex + 1) % 4, // Wrong index
            null,
            10
        )).ToList();

        var request = new SubmitAssessmentRequest(answers, null, null);

        // Act
        var result = await _service.SubmitAssessmentAsync(request);

        // Assert
        result.Category.Should().Be("Basic");
        result.CefrLevel.Should().Be("A1");
        result.OverallScore.Should().Be(0m);
        result.Weaknesses.Should().NotBeEmpty();
        result.Weaknesses.Should().Contain("Grammar & Tense Consistency");
    }

    [Fact]
    public async Task SubmitAssessmentAsync_WithoutAudio_ShouldMarkSpeakingUnassessedWithoutPenalty()
    {
        // Arrange: Correct answers for non-speaking skills
        var questions = await _service.GetDiagnosticQuestionsAsync();
        var nonSpeakingQuestions = questions.Where(q => q.SkillCategory != "Speaking").ToList();
        var answers = nonSpeakingQuestions.Select(q => new QuestionAnswerDto(
            q.Id,
            q.CorrectOptionIndex,
            null,
            12
        )).ToList();

        var request = new SubmitAssessmentRequest(answers, null, null);

        // Act
        var result = await _service.SubmitAssessmentAsync(request);

        // Assert
        result.IsSpeakingAssessed.Should().BeFalse();
        result.SpeakingScore.Should().BeNull();
        // Since all submitted non-speaking questions were correct, overall score should remain high (not penalized for missing audio)
        result.OverallScore.Should().Be(100m);
    }

    [Fact]
    public async Task GetOrCreateTodayPlanAsync_WhenNoCachedPlan_ShouldGenerateFourMandatoryPhasesWithMatchingDurations()
    {
        // Arrange
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        _repositoryMock.Setup(r => r.GetStudyPlanByDateAsync(_userId, today, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudyPlan?)null);

        _repositoryMock.Setup(r => r.GetLatestAssessmentAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LearnerAssessment?)null); // Fallback to user level (B1 -> Standard)

        _repositoryMock.Setup(r => r.GetAllActiveObjectivesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CurriculumCatalog.MasterObjectives);

        _repositoryMock.Setup(r => r.GetStudyPlanHistoryAsync(_userId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StudyPlan>());

        _repositoryMock.Setup(r => r.GetRecentActivitiesAsync(_userId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LearningActivityHistory>());

        _repositoryMock.Setup(r => r.GetAllSkillMasteriesAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LearnerSkillMastery>());

        // Act
        var plan = await _service.GetOrCreateTodayPlanAsync(explicitDate: null, forceRegenerate: false);

        // Assert
        plan.Should().NotBeNull();
        plan.Phases.Should().HaveCount(4);

        // Check phase order: 1. GrammarStudy, 2. SentenceMaking, 3. Practice, 4. AiSpeaking
        plan.Phases[0].PhaseType.Should().Be("GrammarStudy");
        plan.Phases[0].PhaseOrder.Should().Be(1);
        plan.Phases[0].IsLocked.Should().BeFalse(); // First phase is unlocked

        plan.Phases[1].PhaseType.Should().Be("SentenceMaking");
        plan.Phases[1].PhaseOrder.Should().Be(2);
        plan.Phases[1].IsLocked.Should().BeTrue(); // Later phases locked initially

        plan.Phases[2].PhaseType.Should().Be("Practice");
        plan.Phases[2].PhaseOrder.Should().Be(3);
        plan.Phases[2].IsLocked.Should().BeTrue();

        plan.Phases[3].PhaseType.Should().Be("AiSpeaking");
        plan.Phases[3].PhaseOrder.Should().Be(4);
        plan.Phases[3].IsLocked.Should().BeTrue();

        // Check sum of durations matches target minutes
        var totalPhaseMinutes = plan.Phases.Sum(p => p.PlannedDurationMinutes);
        totalPhaseMinutes.Should().Be(plan.TargetMinutes);

        // Standard target is 20 minutes -> [5, 5, 5, 5]
        plan.TargetMinutes.Should().Be(20);
        totalPhaseMinutes.Should().Be(20);

        _repositoryMock.Verify(r => r.AddStudyPlanAsync(It.IsAny<StudyPlan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateTodayPlanAsync_WhenExistingPlanExistsAndNotForceRegenerate_ShouldBeIdempotent()
    {
        // Arrange
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var objective = new LearningObjective
        {
            Id = Guid.NewGuid(),
            Code = "STD-01",
            Title = "Present Perfect vs Simple Past",
            Difficulty = LearnerCategory.Standard,
            CefrLevel = EnglishProficiencyLevel.B1
        };

        var existingPlan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PlanDate = today,
            ObjectiveId = objective.Id,
            Objective = objective,
            ObjectiveCode = "STD-01",
            TargetMinutes = 20,
            ActualMinutes = 10,
            Status = StudyPlanStatus.InProgress,
            Phases = new List<StudyPlanPhase>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    PhaseOrder = 1,
                    PhaseType = PhaseType.GrammarStudy,
                    ObjectiveTitle = "Study Present Perfect vs Simple Past",
                    PlannedDurationMinutes = 5,
                    ActualDurationMinutes = 5,
                    Status = StudyPlanStatus.Completed,
                    TasksJson = "[]"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    PhaseOrder = 2,
                    PhaseType = PhaseType.SentenceMaking,
                    ObjectiveTitle = "Make sentences with Present Perfect",
                    PlannedDurationMinutes = 5,
                    ActualDurationMinutes = 5,
                    Status = StudyPlanStatus.InProgress,
                    TasksJson = "[]"
                }
            }
        };

        _repositoryMock.Setup(r => r.GetStudyPlanByDateAsync(_userId, today, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        _repositoryMock.Setup(r => r.GetAllActiveObjectivesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CurriculumCatalog.MasterObjectives);

        // Act
        var plan = await _service.GetOrCreateTodayPlanAsync(explicitDate: null, forceRegenerate: false);

        // Assert
        plan.Should().NotBeNull();
        plan.Id.Should().Be(existingPlan.Id);
        plan.IsIdempotentCached.Should().BeTrue();

        // Should not add a new plan
        _repositoryMock.Verify(r => r.AddStudyPlanAsync(It.IsAny<StudyPlan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePhaseProgressAsync_ShouldIncrementActiveDurationAndSetInProgress()
    {
        // Arrange
        var phaseId = Guid.NewGuid();
        var plan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            ActualMinutes = 0,
            Status = StudyPlanStatus.NotStarted
        };

        var phase = new StudyPlanPhase
        {
            Id = phaseId,
            StudyPlanId = plan.Id,
            StudyPlan = plan,
            PhaseOrder = 1,
            PhaseType = PhaseType.GrammarStudy,
            Status = StudyPlanStatus.NotStarted,
            ActualDurationMinutes = 0
        };

        _repositoryMock.Setup(r => r.GetPhaseByIdAsync(phaseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phase);

        var request = new UpdatePhaseProgressRequest(120);

        // Act: add 120 seconds (2 minutes)
        var result = await _service.UpdatePhaseProgressAsync(phaseId, request);

        // Assert
        phase.ActualDurationMinutes.Should().Be(2);
        phase.Status.Should().Be(StudyPlanStatus.InProgress);
        plan.ActualMinutes.Should().Be(2);
        plan.Status.Should().Be(StudyPlanStatus.InProgress);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompletePhaseAsync_WithHighScores_ShouldMarkMasteredAndUnlockNextPhase()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var phase1Id = Guid.NewGuid();
        var phase2Id = Guid.NewGuid();

        var tasks = new List<PhaseTaskDto>
        {
            new("t1", "multiple_choice", "Select option", "Question 1", new[] { "A", "B" }, "A", "Explanation 1"),
            new("t2", "multiple_choice", "Select option", "Question 2", new[] { "X", "Y" }, "X", "Explanation 2")
        };

        var plan = new StudyPlan
        {
            Id = planId,
            UserId = _userId,
            ObjectiveCode = "STD-01",
            Status = StudyPlanStatus.InProgress
        };

        var phase1 = new StudyPlanPhase
        {
            Id = phase1Id,
            StudyPlanId = planId,
            StudyPlan = plan,
            PhaseOrder = 1,
            PhaseType = PhaseType.GrammarStudy,
            Status = StudyPlanStatus.InProgress,
            TasksJson = JsonSerializer.Serialize(tasks),
            ActualDurationMinutes = 4
        };

        var phase2 = new StudyPlanPhase
        {
            Id = phase2Id,
            StudyPlanId = planId,
            StudyPlan = plan,
            PhaseOrder = 2,
            PhaseType = PhaseType.SentenceMaking,
            Status = StudyPlanStatus.NotStarted,
            TasksJson = "[]",
            ActualDurationMinutes = 0
        };

        plan.Phases = new List<StudyPlanPhase> { phase1, phase2 };

        _repositoryMock.Setup(r => r.GetPhaseByIdAsync(phase1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phase1);

        _repositoryMock.Setup(r => r.GetSkillMasteryAsync(_userId, "STD-01", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LearnerSkillMastery?)null);

        // Answers: both correct
        var answers = new Dictionary<string, string>
        {
            { "t1", "A" },
            { "t2", "X" }
        };
        var request = new CompletePhaseRequest(answers, 180);

        // Act
        var result = await _service.CompletePhaseAsync(phase1Id, request);

        // Assert
        result.Score.Should().Be(100m);
        result.IsMastered.Should().BeTrue();
        result.Status.Should().Be("Completed");
        result.NextPhaseUnlocked.Should().BeTrue();

        phase1.Status.Should().Be(StudyPlanStatus.Completed);
        phase1.Score.Should().Be(100m);

        _repositoryMock.Verify(r => r.AddActivityHistoryAsync(It.IsAny<LearningActivityHistory>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _repositoryMock.Verify(r => r.UpsertSkillMasteryAsync(It.IsAny<LearnerSkillMastery>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompletePhaseAsync_WithOtherUsersPhase_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var phaseId = Guid.NewGuid();
        var plan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = _otherUserId // Different user
        };

        var phase = new StudyPlanPhase
        {
            Id = phaseId,
            StudyPlanId = plan.Id,
            StudyPlan = plan
        };

        _repositoryMock.Setup(r => r.GetPhaseByIdAsync(phaseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phase);

        var request = new CompletePhaseRequest(new Dictionary<string, string>(), 60);

        // Act
        var act = async () => await _service.CompletePhaseAsync(phaseId, request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetAllObjectivesAsync_ShouldReturnCatalogWithMasteryStatus()
    {
        // Arrange
        var seededObjectives = CurriculumCatalog.MasterObjectives;
        _repositoryMock.Setup(r => r.GetAllActiveObjectivesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(seededObjectives);

        var mastery = new LearnerSkillMastery
        {
            UserId = _userId,
            ObjectiveCode = "OBJ_B_PRES_SIMPLE",
            MasteryScore = 90.0m,
            IsMastered = true
        };
        _repositoryMock.Setup(r => r.GetAllSkillMasteriesAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { mastery });

        // Act
        var result = await _service.GetAllObjectivesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeGreaterOrEqualTo(12);
        var simplePresent = result.FirstOrDefault(o => o.Code == "OBJ_B_PRES_SIMPLE");
        simplePresent.Should().NotBeNull();
        simplePresent!.IsMastered.Should().BeTrue();
        simplePresent.MasteryScore.Should().Be(90.0m);
    }

    [Fact]
    public async Task GetOrCreateTodayPlanAsync_WithTargetObjective_ShouldGeneratePlanForRequestedTense()
    {
        // Arrange
        var seededObjectives = CurriculumCatalog.MasterObjectives;
        _repositoryMock.Setup(r => r.GetAllActiveObjectivesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(seededObjectives);
        _repositoryMock.Setup(r => r.GetStudyPlanByDateAsync(_userId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudyPlan?)null);

        StudyPlan? capturedPlan = null;
        _repositoryMock.Setup(r => r.AddStudyPlanAsync(It.IsAny<StudyPlan>(), It.IsAny<CancellationToken>()))
            .Callback<StudyPlan, CancellationToken>((p, ct) => capturedPlan = p)
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.GetStudyPlanWithPhasesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => capturedPlan);

        // Act
        var result = await _service.GetOrCreateTodayPlanAsync(targetObjectiveCode: "OBJ_S_PAST_PERF");

        // Assert
        result.Should().NotBeNull();
        result.ObjectiveCode.Should().Be("OBJ_S_PAST_PERF");
        result.Phases.Should().HaveCount(4);
    }
}
