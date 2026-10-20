using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Exceptions;

namespace EnglishLearning.Application.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationSessionRepository _sessionRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiConversationService _aiService;
    private readonly ICurrentUserService _currentUserService;

    public ConversationService(
        IConversationSessionRepository sessionRepository,
        ITopicRepository topicRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IAiConversationService aiService,
        ICurrentUserService currentUserService)
    {
        _sessionRepository = sessionRepository;
        _topicRepository = topicRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _aiService = aiService;
        _currentUserService = currentUserService;
    }

    public async Task<ConversationSessionDto> StartConversationAsync(StartConversationRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var user = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException(nameof(User), currentUserId);
        }

        var topic = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
        if (topic == null || !topic.IsActive)
        {
            throw new EntityNotFoundException(nameof(Topic), request.TopicId);
        }

        var level = user.EnglishLevel;
        if (!string.IsNullOrWhiteSpace(request.EnglishLevel) &&
            Enum.TryParse<EnglishProficiencyLevel>(request.EnglishLevel, true, out var parsedLevel))
        {
            level = parsedLevel;
        }

        var session = new ConversationSession
        {
            UserId = currentUserId,
            TopicId = topic.Id,
            EnglishLevel = level,
            Status = SessionStatus.Active,
            StartedAt = DateTime.UtcNow,
            DurationSeconds = 0
        };

        // Generate initial tutor greeting based on the topic, CEFR level, and optional target objective
        var initialAiRequest = new AiConversationRequest(
            topic.Name,
            topic.Description,
            level.ToString(),
            Array.Empty<AiMessageContext>(),
            request.TargetObjective
        );

        var aiGreeting = await _aiService.GenerateResponseAsync(initialAiRequest, cancellationToken);

        var initialMessage = new ConversationMessage
        {
            SessionId = session.Id,
            Role = SpeakerRole.Assistant,
            MessageText = aiGreeting.Message,
            SequenceNumber = 1,
            CreatedAt = DateTime.UtcNow
        };

        session.Messages.Add(initialMessage);

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(session, topic.Name);
    }

    public async Task<SendMessageResponseDto> SendMessageAsync(Guid sessionId, SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new EntityNotFoundException(nameof(ConversationSession), sessionId);
        }

        if (session.UserId != currentUserId)
        {
            throw new UnauthorizedSessionAccessException(sessionId);
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new InvalidSessionStateException("Cannot send messages to a completed or abandoned conversation session.");
        }

        var nextSequence = (session.Messages.Count > 0 ? session.Messages.Max(m => m.SequenceNumber) : 0) + 1;

        var userMessage = new ConversationMessage
        {
            SessionId = session.Id,
            Role = SpeakerRole.User,
            MessageText = request.Message.Trim(),
            SequenceNumber = nextSequence,
            CreatedAt = DateTime.UtcNow
        };

        session.Messages.Add(userMessage);
        await _sessionRepository.AddMessageAsync(userMessage, cancellationToken);

        // Build rolling context window (last 10 messages)
        var contextMessages = session.Messages
            .OrderBy(m => m.SequenceNumber)
            .TakeLast(10)
            .Select(m => new AiMessageContext(
                m.Role == SpeakerRole.User ? "user" : "assistant",
                m.MessageText
            ))
            .ToList();

        var topicName = session.Topic?.Name ?? "General English Practice";
        var topicDesc = session.Topic?.Description ?? "English Conversation Dialogue";

        var aiRequest = new AiConversationRequest(
            topicName,
            topicDesc,
            session.EnglishLevel.ToString(),
            contextMessages
        );

        var aiResponse = await _aiService.GenerateResponseAsync(aiRequest, cancellationToken);

        var aiMessage = new ConversationMessage
        {
            SessionId = session.Id,
            Role = SpeakerRole.Assistant,
            MessageText = aiResponse.Message,
            SequenceNumber = nextSequence + 1,
            CreatedAt = DateTime.UtcNow
        };

        session.Messages.Add(aiMessage);
        await _sessionRepository.AddMessageAsync(aiMessage, cancellationToken);

        session.DurationSeconds = Math.Max(1, (int)(DateTime.UtcNow - session.StartedAt).TotalSeconds);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SendMessageResponseDto(
            MapToDto(session),
            MapMessageToDto(userMessage),
            MapMessageToDto(aiMessage),
            aiResponse.PedagogicalTip
        );
    }

    public async Task<ConversationSessionDto> GetConversationByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new EntityNotFoundException(nameof(ConversationSession), sessionId);
        }

        if (session.UserId != currentUserId)
        {
            throw new UnauthorizedSessionAccessException(sessionId);
        }

        return MapToDto(session);
    }

    public async Task<ConversationSessionDto> EndConversationAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new EntityNotFoundException(nameof(ConversationSession), sessionId);
        }

        if (session.UserId != currentUserId)
        {
            throw new UnauthorizedSessionAccessException(sessionId);
        }

        if (session.Status == SessionStatus.Active)
        {
            session.Status = SessionStatus.Completed;
            session.EndedAt = DateTime.UtcNow;
            session.DurationSeconds = Math.Max(1, (int)(DateTime.UtcNow - session.StartedAt).TotalSeconds);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(session);
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new InvalidCredentialsException("User is not authenticated.");
        }

        return _currentUserService.UserId.Value;
    }

    private static ConversationSessionDto MapToDto(ConversationSession session, string? overrideTopicName = null) =>
        new(
            session.Id,
            session.UserId,
            session.TopicId,
            overrideTopicName ?? session.Topic?.Name ?? string.Empty,
            session.EnglishLevel.ToString(),
            session.StartedAt,
            session.EndedAt,
            session.DurationSeconds,
            session.OverallScore,
            session.Status.ToString(),
            session.Messages
                .OrderBy(m => m.SequenceNumber)
                .Select(MapMessageToDto)
                .ToList()
        );

    private static ConversationMessageDto MapMessageToDto(ConversationMessage message) =>
        new(
            message.Id,
            message.SessionId,
            message.Role == SpeakerRole.User ? "user" : "assistant",
            message.MessageText,
            message.AudioStoragePath,
            message.SequenceNumber,
            message.CreatedAt
        );
}
