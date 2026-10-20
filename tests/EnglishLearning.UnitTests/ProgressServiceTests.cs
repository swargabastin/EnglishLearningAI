using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EnglishLearning.UnitTests;

public class ProgressServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConversationSessionRepository> _sessionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ProgressService _service;

    private readonly Guid _currentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _otherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public ProgressServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _sessionRepositoryMock = new Mock<IConversationSessionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_currentUserId);

        _service = new ProgressService(
            _userRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _currentUserServiceMock.Object,
            NullLogger<ProgressService>.Instance
        );
    }

    [Fact]
    public async Task GetDashboardAsync_WithNoCompletedSessions_ShouldReturnZeroStatsAndEmptyLists()
    {
        // Arrange
        var user = new User
        {
            Id = _currentUserId,
            Name = "John Doe",
            Email = "john@example.com",
            EnglishLevel = EnglishProficiencyLevel.B1,
            LearningGoal = LearningGoal.SpeakingFluency
        };

        _userRepositoryMock.Setup(u => u.GetByIdAsync(_currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _sessionRepositoryMock.Setup(s => s.GetCompletedSessionsWithAssessmentsByUserIdAsync(_currentUserId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConversationSession>());

        // Act
        var result = await _service.GetDashboardAsync();

        // Assert
        result.Should().NotBeNull();
        result.User.Name.Should().Be("John Doe");
        result.User.TotalSessionsCompleted.Should().Be(0);
        result.Stats.TotalCompletedSessions.Should().Be(0);
        result.Stats.AverageOverallScore.Should().Be(0m);
        result.Stats.LatestScore.Should().Be(0m);
        result.Stats.GrammarAverage.Should().Be(0m);
        result.RecentSessions.Should().BeEmpty();
        result.ScoreProgression.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardAsync_WithCompletedAssessedSessions_ShouldAggregateAveragesAndProgression()
    {
        // Arrange
        var user = new User
        {
            Id = _currentUserId,
            Name = "Jane Doe",
            Email = "jane@example.com",
            EnglishLevel = EnglishProficiencyLevel.B2,
            LearningGoal = LearningGoal.JobInterviews
        };

        _userRepositoryMock.Setup(u => u.GetByIdAsync(_currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var topic = new Topic { Id = Guid.NewGuid(), Name = "Job Interview", Description = "Interview practice" };

        var session1 = new ConversationSession
        {
            Id = Guid.NewGuid(),
            UserId = _currentUserId,
            Topic = topic,
            EnglishLevel = EnglishProficiencyLevel.B2,
            Status = SessionStatus.Completed,
            StartedAt = DateTime.UtcNow.AddDays(-2),
            DurationSeconds = 300,
            Assessment = new Assessment
            {
                OverallScore = 80m,
                GrammarScore = 78m,
                VocabularyScore = 82m,
                FluencyScore = 80m
            }
        };

        var session2 = new ConversationSession
        {
            Id = Guid.NewGuid(),
            UserId = _currentUserId,
            Topic = topic,
            EnglishLevel = EnglishProficiencyLevel.B2,
            Status = SessionStatus.Completed,
            StartedAt = DateTime.UtcNow.AddDays(-1),
            DurationSeconds = 600,
            Assessment = new Assessment
            {
                OverallScore = 90m,
                GrammarScore = 88m,
                VocabularyScore = 92m,
                FluencyScore = 90m
            }
        };

        var completedList = new List<ConversationSession> { session2, session1 }; // newest first

        _sessionRepositoryMock.Setup(s => s.GetCompletedSessionsWithAssessmentsByUserIdAsync(_currentUserId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedList);

        // Act
        var result = await _service.GetDashboardAsync();

        // Assert
        result.Should().NotBeNull();
        result.Stats.TotalCompletedSessions.Should().Be(2);
        // (300 + 600) / 60 = 15 minutes
        result.Stats.TotalPracticeMinutes.Should().Be(15);
        // (90 + 80) / 2 = 85.0
        result.Stats.AverageOverallScore.Should().Be(85.0m);
        // Latest (session2) = 90
        result.Stats.LatestScore.Should().Be(90m);
        // (88 + 78) / 2 = 83.0
        result.Stats.GrammarAverage.Should().Be(83.0m);
        result.RecentSessions.Should().HaveCount(2);
        result.ScoreProgression.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetHistoryAsync_WithPagination_ShouldReturnCorrectPageAndTotalPages()
    {
        // Arrange
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Travel", Description = "Travel practice" };
        var sessions = new List<ConversationSession>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = _currentUserId,
                Topic = topic,
                EnglishLevel = EnglishProficiencyLevel.B1,
                Status = SessionStatus.Completed,
                StartedAt = DateTime.UtcNow,
                DurationSeconds = 240,
                Assessment = new Assessment { OverallScore = 85m }
            }
        };

        _sessionRepositoryMock.Setup(s => s.GetPaginatedHistoryByUserIdAsync(_currentUserId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((sessions, 25)); // 25 total items

        // Act
        var result = await _service.GetHistoryAsync(page: 1, pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetSessionDetailAsync_WithOwnedSession_ShouldReturnTranscriptAndAssessment()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Restaurant", Description = "Ordering food" };
        var session = new ConversationSession
        {
            Id = sessionId,
            UserId = _currentUserId,
            Topic = topic,
            EnglishLevel = EnglishProficiencyLevel.A2,
            Status = SessionStatus.Completed,
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            DurationSeconds = 600,
            Assessment = new Assessment
            {
                OverallScore = 80m,
                GrammarScore = 80m,
                VocabularyScore = 80m,
                FluencyScore = 80m,
                Feedback = "Great practice ordering food!",
                StrengthsJson = "[\"Polite vocabulary\"]",
                AreasForImprovementJson = "[\"Plurals\"]"
            }
        };

        session.Messages.Add(new ConversationMessage
        {
            SessionId = sessionId,
            Role = SpeakerRole.Assistant,
            MessageText = "Welcome to the restaurant. Are you ready to order?",
            SequenceNumber = 1
        });

        session.Messages.Add(new ConversationMessage
        {
            SessionId = sessionId,
            Role = SpeakerRole.User,
            MessageText = "Yes, I would like a table for two.",
            SequenceNumber = 2
        });

        _sessionRepositoryMock.Setup(s => s.GetByIdWithDetailsAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _service.GetSessionDetailAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.SessionId.Should().Be(sessionId);
        result.TopicName.Should().Be("Restaurant");
        result.Messages.Should().HaveCount(2);
        result.Assessment.Should().NotBeNull();
        result.Assessment!.OverallScore.Should().Be(80m);
        result.Assessment.Strengths.Should().Contain("Polite vocabulary");
    }

    [Fact]
    public async Task GetSessionDetailAsync_WithNonOwnerSession_ShouldThrow_UnauthorizedSessionAccessException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ConversationSession
        {
            Id = sessionId,
            UserId = _otherUserId,
            Status = SessionStatus.Completed
        };

        _sessionRepositoryMock.Setup(s => s.GetByIdWithDetailsAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        Func<Task> act = async () => await _service.GetSessionDetailAsync(sessionId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedSessionAccessException>();
    }
}
