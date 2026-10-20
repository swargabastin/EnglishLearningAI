using System.Text.Json;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Exceptions;
using EnglishLearning.Infrastructure.Ai.Prompts;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EnglishLearning.UnitTests;

public class AssessmentServiceTests
{
    private readonly Mock<IConversationSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IAssessmentRepository> _assessmentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAiAssessmentService> _aiAssessmentServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AssessmentService _service;

    private readonly Guid _currentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _otherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private readonly Guid _sessionId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public AssessmentServiceTests()
    {
        _sessionRepositoryMock = new Mock<IConversationSessionRepository>();
        _assessmentRepositoryMock = new Mock<IAssessmentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _aiAssessmentServiceMock = new Mock<IAiAssessmentService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_currentUserId);

        _service = new AssessmentService(
            _sessionRepositoryMock.Object,
            _assessmentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _aiAssessmentServiceMock.Object,
            _currentUserServiceMock.Object,
            NullLogger<AssessmentService>.Instance
        );
    }

    [Fact]
    public async Task GenerateAssessmentAsync_WithCompletedSession_ShouldCalculateOverallScoreAndSave()
    {
        // Arrange
        var topic = new Topic
        {
            Id = Guid.NewGuid(),
            Name = "Job Interview",
            Description = "Practice job interview questions",
            RecommendedDifficulty = EnglishProficiencyLevel.B2
        };

        var session = new ConversationSession
        {
            Id = _sessionId,
            UserId = _currentUserId,
            TopicId = topic.Id,
            Topic = topic,
            EnglishLevel = EnglishProficiencyLevel.B2,
            Status = SessionStatus.Completed,
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            EndedAt = DateTime.UtcNow,
            DurationSeconds = 600
        };

        session.Messages.Add(new ConversationMessage
        {
            SessionId = _sessionId,
            Role = SpeakerRole.Assistant,
            MessageText = "Tell me about yourself.",
            SequenceNumber = 1
        });

        session.Messages.Add(new ConversationMessage
        {
            SessionId = _sessionId,
            Role = SpeakerRole.User,
            MessageText = "I have been working as a software developer for five years.",
            SequenceNumber = 2
        });

        _sessionRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _assessmentRepositoryMock.Setup(r => r.GetBySessionIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Assessment?)null);

        var aiResponse = new AssessmentAiResponse(
            GrammarScore: 80m,
            VocabularyScore: 86m,
            FluencyScore: 80m,
            PronunciationScore: null,
            Feedback: "Excellent responses and vocabulary choice.",
            Strengths: new List<string> { "Strong past experience phrasing", "Clear technical vocabulary" },
            AreasForImprovement: new List<string> { "Incorporate more transitional connectors" },
            GrammarCorrections: new List<GrammarCorrectionDto>(),
            VocabularySuggestions: new List<VocabularySuggestionDto>(),
            Recommendations: new List<string> { "Practice behavioural interview questions" }
        );

        _aiAssessmentServiceMock.Setup(a => a.EvaluateConversationAsync(It.IsAny<AssessmentAiRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.GenerateAssessmentAsync(_sessionId);

        // Assert
        result.Should().NotBeNull();
        result.SessionId.Should().Be(_sessionId);
        result.GrammarScore.Should().Be(80m);
        result.VocabularyScore.Should().Be(86m);
        result.FluencyScore.Should().Be(80m);
        // (80 + 86 + 80) / 3 = 82.0
        result.OverallScore.Should().Be(82.0m);
        result.Feedback.Should().Be("Excellent responses and vocabulary choice.");
        result.Strengths.Should().HaveCount(2);

        _assessmentRepositoryMock.Verify(r => r.AddAsync(It.Is<Assessment>(a =>
            a.SessionId == _sessionId &&
            a.OverallScore == 82.0m &&
            a.GrammarScore == 80m
        ), It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAssessmentAsync_WithActiveSession_ShouldThrow_InvalidSessionStateException()
    {
        // Arrange
        var session = new ConversationSession
        {
            Id = _sessionId,
            UserId = _currentUserId,
            Status = SessionStatus.Active
        };

        _sessionRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        Func<Task> act = async () => await _service.GenerateAssessmentAsync(_sessionId);

        // Assert
        await act.Should().ThrowAsync<InvalidSessionStateException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public async Task GenerateAssessmentAsync_WithNoUserMessages_ShouldThrow_InvalidSessionStateException()
    {
        // Arrange
        var session = new ConversationSession
        {
            Id = _sessionId,
            UserId = _currentUserId,
            Status = SessionStatus.Completed
        };
        // Only assistant greeting, no user messages
        session.Messages.Add(new ConversationMessage
        {
            SessionId = _sessionId,
            Role = SpeakerRole.Assistant,
            MessageText = "Hello!",
            SequenceNumber = 1
        });

        _sessionRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        Func<Task> act = async () => await _service.GenerateAssessmentAsync(_sessionId);

        // Assert
        await act.Should().ThrowAsync<InvalidSessionStateException>()
            .WithMessage("*no user messages*");
    }

    [Fact]
    public async Task GenerateAssessmentAsync_WithNonOwner_ShouldThrow_UnauthorizedSessionAccessException()
    {
        // Arrange
        var session = new ConversationSession
        {
            Id = _sessionId,
            UserId = _otherUserId,
            Status = SessionStatus.Completed
        };

        _sessionRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        Func<Task> act = async () => await _service.GenerateAssessmentAsync(_sessionId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedSessionAccessException>();
    }

    [Fact]
    public async Task GenerateAssessmentAsync_WhenAlreadyExists_ShouldReturnExistingWithoutCallingAi()
    {
        // Arrange
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Travel", Description = "Travel practice" };
        var session = new ConversationSession
        {
            Id = _sessionId,
            UserId = _currentUserId,
            Topic = topic,
            Status = SessionStatus.Completed
        };
        session.Messages.Add(new ConversationMessage { Role = SpeakerRole.User, MessageText = "I went to Rome." });

        var existingAssessment = new Assessment
        {
            Id = Guid.NewGuid(),
            SessionId = _sessionId,
            GrammarScore = 85m,
            VocabularyScore = 90m,
            FluencyScore = 88m,
            OverallScore = 87.7m,
            Feedback = "Previously generated evaluation.",
            StrengthsJson = JsonSerializer.Serialize(new List<string> { "Good vocabulary" }),
            AreasForImprovementJson = JsonSerializer.Serialize(new List<string> { "Minor prepositions" }),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _sessionRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _assessmentRepositoryMock.Setup(r => r.GetBySessionIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAssessment);

        // Act
        var result = await _service.GenerateAssessmentAsync(_sessionId);

        // Assert
        result.Should().NotBeNull();
        result.OverallScore.Should().Be(87.7m);
        result.Feedback.Should().Be("Previously generated evaluation.");

        _aiAssessmentServiceMock.Verify(a => a.EvaluateConversationAsync(It.IsAny<AssessmentAiRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _assessmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Assessment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAssessmentBySessionIdAsync_WithValidSessionAndAssessment_ShouldReturnDto()
    {
        // Arrange
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Daily Life", Description = "Daily conversation" };
        var session = new ConversationSession
        {
            Id = _sessionId,
            UserId = _currentUserId,
            Topic = topic,
            Status = SessionStatus.Completed
        };

        var assessment = new Assessment
        {
            Id = Guid.NewGuid(),
            SessionId = _sessionId,
            GrammarScore = 80m,
            VocabularyScore = 85m,
            FluencyScore = 82m,
            OverallScore = 82.3m,
            Feedback = "Great conversational flow.",
            StrengthsJson = "[\"Clear sentences\"]",
            AreasForImprovementJson = "[\"Past tenses\"]",
            CreatedAt = DateTime.UtcNow
        };

        _sessionRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _assessmentRepositoryMock.Setup(r => r.GetBySessionIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assessment);

        // Act
        var result = await _service.GetAssessmentBySessionIdAsync(_sessionId);

        // Assert
        result.Should().NotBeNull();
        result.OverallScore.Should().Be(82.3m);
        result.GrammarScore.Should().Be(80m);
        result.Strengths.Should().Contain("Clear sentences");
    }

    [Fact]
    public void AssessmentPromptBuilder_BuildSystemPrompt_ShouldContain_AntiInjectionAndSchemaRules()
    {
        // Act
        var systemPrompt = AssessmentPromptBuilder.BuildSystemPrompt();

        // Assert
        systemPrompt.Should().Contain("SECURITY & ANTI-PROMPT-INJECTION RULES");
        systemPrompt.Should().Contain("grammarScore");
        systemPrompt.Should().Contain("vocabularyScore");
        systemPrompt.Should().Contain("fluencyScore");
        systemPrompt.Should().Contain("REQUIRED JSON OUTPUT SCHEMA");
    }
}
