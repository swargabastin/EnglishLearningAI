using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EnglishLearning.Api.Hubs;

[Authorize]
public class ConversationHub : Hub
{
    private readonly IConversationService _conversationService;
    private readonly ISpeechConversationService _speechConversationService;
    private readonly ILogger<ConversationHub> _logger;

    public ConversationHub(
        IConversationService conversationService,
        ISpeechConversationService speechConversationService,
        ILogger<ConversationHub> logger)
    {
        _conversationService = conversationService;
        _speechConversationService = speechConversationService;
        _logger = logger;
    }

    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        _logger.LogInformation("SignalR connection {ConnectionId} joined session {SessionId}", Context.ConnectionId, sessionId);
    }

    public async Task SendLiveMessage(string sessionId, string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText) || !Guid.TryParse(sessionId, out var sessionGuid))
        {
            await Clients.Caller.SendAsync("OnError", "Invalid session or empty message.");
            return;
        }

        try
        {
            await Clients.Caller.SendAsync("OnAiThinking");
            var result = await _conversationService.SendMessageAsync(sessionGuid, new SendMessageRequest(messageText));
            await Clients.Caller.SendAsync("OnAiResponse", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing live message in session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("OnError", ex.Message);
        }
    }

    public async Task SendLiveAudioTurn(string sessionId, string base64Audio, string? clientTranscript = null)
    {
        if (string.IsNullOrWhiteSpace(base64Audio) || !Guid.TryParse(sessionId, out var sessionGuid))
        {
            await Clients.Caller.SendAsync("OnError", "Invalid audio payload or session ID.");
            return;
        }

        try
        {
            await Clients.Caller.SendAsync("OnAudioProcessing");

            byte[] audioBytes = Convert.FromBase64String(base64Audio);
            using var stream = new MemoryStream(audioBytes);

            var result = await _speechConversationService.ProcessSpeechAsync(
                sessionGuid,
                stream,
                "streaming-turn.webm",
                "audio/webm",
                audioBytes.Length,
                clientTranscript
            );

            await Clients.Caller.SendAsync("OnLiveTurnComplete", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing live speech audio turn in session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("OnError", ex.Message);
        }
    }

    public async Task InterruptAi(string sessionId)
    {
        _logger.LogInformation("Learner interrupted AI playback in session {SessionId}", sessionId);
        await Clients.Caller.SendAsync("OnInterruptionAcknowledged");
    }
}
