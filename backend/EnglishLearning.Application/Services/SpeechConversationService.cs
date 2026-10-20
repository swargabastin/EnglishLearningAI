using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Application.Services;

public class SpeechConversationService : ISpeechConversationService
{
    private readonly ISpeechToTextService _sttService;
    private readonly ITextToSpeechService _ttsService;
    private readonly IConversationService _conversationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SpeechConversationService> _logger;

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "audio/webm",
        "audio/webm;codecs=opus",
        "audio/wav",
        "audio/x-wav",
        "audio/wave",
        "audio/mp3",
        "audio/mpeg",
        "audio/mp4",
        "audio/m4a",
        "audio/x-m4a",
        "audio/ogg",
        "audio/aac",
        "audio/octet-stream"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".webm",
        ".wav",
        ".mp3",
        ".m4a",
        ".mp4",
        ".ogg",
        ".aac"
    };

    public SpeechConversationService(
        ISpeechToTextService sttService,
        ITextToSpeechService ttsService,
        IConversationService conversationService,
        IConfiguration configuration,
        ILogger<SpeechConversationService> logger)
    {
        _sttService = sttService;
        _ttsService = ttsService;
        _conversationService = conversationService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SpeechConversationResponseDto> ProcessSpeechAsync(
        Guid sessionId,
        Stream audioStream,
        string fileName,
        string contentType,
        long fileLength,
        CancellationToken cancellationToken = default)
    {
        ValidateAudioUpload(fileName, contentType, fileLength);

        _logger.LogInformation("Processing speech input for session '{SessionId}' (File: '{FileName}', Type: '{ContentType}', Size: {Bytes} bytes)",
            sessionId, fileName, contentType, fileLength);

        // 1. Transcribe speech to text via ISpeechToTextService
        var sttResult = await _sttService.TranscribeAsync(audioStream, contentType, "en-US", cancellationToken);

        if (string.IsNullOrWhiteSpace(sttResult.Transcript))
        {
            throw new InvalidAudioException("No speech could be recognized from the audio. Please speak clearly into your microphone.");
        }

        var recognizedText = sttResult.Transcript.Trim();
        _logger.LogInformation("Speech recognized successfully for session '{SessionId}': \"{Transcript}\"", sessionId, recognizedText);

        // 2. Feed recognized text into the existing Phase 6 Conversation pipeline
        var sendResult = await _conversationService.SendMessageAsync(
            sessionId,
            new SendMessageRequest(recognizedText),
            cancellationToken
        );

        // 3. Synthesize AI tutor response to speech via ITextToSpeechService
        string? audioBase64 = null;
        try
        {
            var ttsResult = await _ttsService.SynthesizeAsync(
                sendResult.AiMessage.MessageText,
                new VoiceOptions("nova", 1.0f, "audio/mpeg"),
                cancellationToken
            );

            if (ttsResult.AudioData != null && ttsResult.AudioData.Length > 0)
            {
                audioBase64 = Convert.ToBase64String(ttsResult.AudioData);
                _logger.LogInformation("Text-to-speech synthesized successfully ({Bytes} bytes) for session '{SessionId}'",
                    ttsResult.AudioData.Length, sessionId);
            }
        }
        catch (Exception ex)
        {
            // Partial failure safe handling: Log warning but do not fail the overall conversation
            _logger.LogWarning(ex, "Text-to-speech synthesis failed for session '{SessionId}'. Returning text-only conversation response.", sessionId);
        }

        return new SpeechConversationResponseDto(
            sendResult.Session,
            sendResult.UserMessage,
            sendResult.AiMessage,
            recognizedText,
            audioBase64,
            "audio/mpeg",
            sendResult.PedagogicalTip
        );
    }

    private void ValidateAudioUpload(string fileName, string contentType, long fileLength)
    {
        if (fileLength <= 0)
        {
            throw new InvalidAudioException("Uploaded audio file is empty.");
        }

        var maxSizeBytes = long.TryParse(_configuration["AiSettings:MaxAudioSizeBytes"], out var maxBytes)
            ? maxBytes
            : 10 * 1024 * 1024; // 10 MB

        if (fileLength > maxSizeBytes)
        {
            throw new InvalidAudioException($"Audio file size ({fileLength} bytes) exceeds the maximum permitted limit of {maxSizeBytes / (1024 * 1024)}MB.");
        }

        var extension = Path.GetExtension(fileName);
        var baseContentType = (contentType ?? string.Empty).Split(';')[0].Trim();

        var isMimeValid = AllowedMimeTypes.Contains(baseContentType) || baseContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);
        var isExtensionValid = !string.IsNullOrWhiteSpace(extension) && AllowedExtensions.Contains(extension);

        if (!isMimeValid && !isExtensionValid)
        {
            throw new InvalidAudioException($"Unsupported audio format. Supported formats include WebM, WAV, MP3, MP4, M4A, OGG, and AAC.");
        }
    }
}
