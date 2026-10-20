using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnglishLearning.UnitTests;

public class ConversationServiceTests
{
    private readonly Mock<IConversationSessionRepository> _sessionRepoMock;
    private readonly Mock<ITopicRepository> _topicRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAiConversationService> _aiServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly ConversationService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _topicId = Guid.NewGuid();

    public ConversationServiceTests()
    {
        _sessionRepoMock = new Mock<IConversationSessionRepository>();
        _topicRepoMock = new Mock<ITopicRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _aiServiceMock = new Mock<IAiConversationService>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(_userId);

        _service = new ConversationService(
            _sessionRepoMock.Object,
            _topicRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _aiServiceMock.Object,
            _currentUserMock.Object
        );
    }

    [Fact]
    public async Task StartConversationAsync_ShouldCreateSessionAndInitialGreeting()
    {
        // Arrange
        var user = new User { Id = _userId, EnglishLevel = EnglishProficiencyLevel.B2 };
        var topic = new Topic { Id = _topicId, Name = "Job Interview", Description = "Interview practice", IsActive = true };

        _userRepoMock.Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _topicRepoMock.Setup(r => r.GetByIdAsync(_topicId, It.IsAny<CancellationToken>())).ReturnsAsync(topic);
        _aiServiceMock.Setup(a => a.GenerateResponseAsync(It.IsAny<AiConversationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiConversationResponse("Hello! Let's begin our interview practice."));

        var request = new StartConversationRequest(_topicId, "B2");

        // Act
        var result = await _service.StartConversationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(_userId);
        result.TopicId.Should().Be(_topicId);
        result.EnglishLevel.Should().Be("B2");
        result.Status.Should().Be("Active");
        result.Messages.Should().HaveCount(1);
        result.Messages![0].Role.Should().Be("assistant");
        result.Messages[0].MessageText.Should().Be("Hello! Let's begin our interview practice.");

        _sessionRepoMock.Verify(r => r.AddAsync(It.IsAny<ConversationSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldAppendUserAndAiMessages()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var topic = new Topic { Id = _topicId, Name = "Travel", Description = "Travel practice", IsActive = true };
        var session = new ConversationSession
        {
            Id = sessionId,
            UserId = _userId,
            TopicId = _topicId,
            Topic = topic,
            EnglishLevel = EnglishProficiencyLevel.B1,
            Status = SessionStatus.Active,
            Messages = new List<ConversationMessage>
            {
                new() { SessionId = sessionId, Role = SpeakerRole.Assistant, MessageText = "Where are you going?", SequenceNumber = 1 }
            }
        };

        _sessionRepoMock.Setup(r => r.GetByIdWithDetailsAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _aiServiceMock.Setup(a => a.GenerateResponseAsync(It.IsAny<AiConversationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiConversationResponse("Paris is beautiful! What do you plan to see there?"));

        var request = new SendMessageRequest("I am planning to visit Paris next summer.");

        // Act
        var response = await _service.SendMessageAsync(sessionId, request);

        // Assert
        response.Should().NotBeNull();
        response.UserMessage.Role.Should().Be("user");
        response.UserMessage.MessageText.Should().Be("I am planning to visit Paris next summer.");
        response.UserMessage.SequenceNumber.Should().Be(2);

        response.AiMessage.Role.Should().Be("assistant");
        response.AiMessage.MessageText.Should().Be("Paris is beautiful! What do you plan to see there?");
        response.AiMessage.SequenceNumber.Should().Be(3);

        session.Messages.Should().HaveCount(3);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ToNonOwnedSession_ShouldThrow_UnauthorizedSessionAccessException()
    {
        // Arrange: Session owned by another user
        var sessionId = Guid.NewGuid();
        var session = new ConversationSession
        {
            Id = sessionId,
            UserId = Guid.NewGuid(), // Different user ID
            TopicId = _topicId,
            Status = SessionStatus.Active
        };

        _sessionRepoMock.Setup(r => r.GetByIdWithDetailsAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var request = new SendMessageRequest("Hello there!");

        // Act
        var act = () => _service.SendMessageAsync(sessionId, request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedSessionAccessException>();
    }

    [Fact]
    public async Task SendMessageAsync_ToCompletedSession_ShouldThrow_InvalidSessionStateException()
    {
        // Arrange: Completed session
        var sessionId = Guid.NewGuid();
        var session = new ConversationSession
        {
            Id = sessionId,
            UserId = _userId,
            TopicId = _topicId,
            Status = SessionStatus.Completed
        };

        _sessionRepoMock.Setup(r => r.GetByIdWithDetailsAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var request = new SendMessageRequest("Can I speak more?");

        // Act
        var act = () => _service.SendMessageAsync(sessionId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidSessionStateException>();
    }

    [Fact]
    public async Task EndConversationAsync_ShouldMarkSessionCompleted()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ConversationSession
        {
            Id = sessionId,
            UserId = _userId,
            TopicId = _topicId,
            Status = SessionStatus.Active,
            StartedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _sessionRepoMock.Setup(r => r.GetByIdWithDetailsAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        // Act
        var result = await _service.EndConversationAsync(sessionId);

        // Assert
        result.Status.Should().Be("Completed");
        session.Status.Should().Be(SessionStatus.Completed);
        session.EndedAt.Should().NotBeNull();
        session.DurationSeconds.Should().BeGreaterThan(0);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
