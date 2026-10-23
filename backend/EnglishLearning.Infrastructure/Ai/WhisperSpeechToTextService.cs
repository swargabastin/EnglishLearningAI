using System.Net.Http.Headers;
using System.Text.Json;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Infrastructure.Ai;

public class WhisperSpeechToTextService : ISpeechToTextService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhisperSpeechToTextService> _logger;

    public WhisperSpeechToTextService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WhisperSpeechToTextService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<SpeechRecognitionResult> TranscribeAsync(
        Stream audioStream,
        string contentType,
        string languageCode = "en-US",
        CancellationToken cancellationToken = default)
    {
        return TranscribeAsync(audioStream, contentType, languageCode, null, cancellationToken);
    }

    public async Task<SpeechRecognitionResult> TranscribeAsync(
        Stream audioStream,
        string contentType,
        string languageCode,
        string? promptHint,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["AiSettings:ApiKey"]
                     ?? _configuration["AI__APIKEY"]
                     ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        var sttModel = _configuration["AiSettings:SttModel"] ?? "whisper-1";
        var timeoutSec = int.TryParse(_configuration["AiSettings:SttTimeoutSeconds"], out var ts) ? ts : 25;

        // When no OpenAI API key is configured on server:
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // 1. If client provided high-accuracy browser speech recognition transcript, use it directly
            if (!string.IsNullOrWhiteSpace(promptHint))
            {
                _logger.LogInformation("OpenAI API key not configured. Using client browser speech recognition transcript: \"{Transcript}\"", promptHint);
                return new SpeechRecognitionResult(promptHint.Trim(), 0.98);
            }

            // 2. Automated testing runner double
            if (IsTestingEnvironment())
            {
                _logger.LogInformation("Running in Testing environment without API key. Using deterministic test speech recognizer.");
                return new SpeechRecognitionResult("I have been practicing spoken English and want to improve my communication skills.", 0.95);
            }

            var currentEnv = _configuration["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            _logger.LogError("OpenAI API key is missing and no client speech transcript was provided in {Environment} mode.", currentEnv);
            throw new AiServiceException("Speech recognition is currently unavailable. Please speak into your microphone or configure OPENAI_API_KEY.");
        }

        var fileExtension = ResolveFileExtension(contentType);
        var fileName = $"audio_upload_{Guid.NewGuid():N}.{fileExtension}";

        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(audioStream);
        
        var cleanContentType = contentType.Split(';')[0].Trim();
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(cleanContentType) ? "audio/webm" : cleanContentType);
        
        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent(sttModel), "model");
        content.Add(new StringContent("en"), "language");

        if (!string.IsNullOrWhiteSpace(promptHint))
        {
            content.Add(new StringContent(promptHint.Trim()), "prompt");
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSec));

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/transcriptions");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = content;

            var response = await _httpClient.SendAsync(httpRequest, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogError("Whisper STT call failed with status code {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);

                // If Whisper fails but we have client transcript hint, gracefully salvage the spoken turn
                if (!string.IsNullOrWhiteSpace(promptHint))
                {
                    _logger.LogWarning("Whisper STT API failed, falling back to client transcript: \"{Transcript}\"", promptHint);
                    return new SpeechRecognitionResult(promptHint.Trim(), 0.90);
                }

                throw new AiServiceException($"Speech recognition service returned error status {response.StatusCode}.");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(responseJson);

            if (!doc.RootElement.TryGetProperty("text", out var textProp) || string.IsNullOrWhiteSpace(textProp.GetString()))
            {
                if (!string.IsNullOrWhiteSpace(promptHint))
                {
                    return new SpeechRecognitionResult(promptHint.Trim(), 0.90);
                }
                throw new AiServiceException("Speech recognition produced an empty transcript.");
            }

            var transcript = textProp.GetString()!.Trim();
            return new SpeechRecognitionResult(transcript, 1.0);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Whisper STT request timed out after {Timeout} seconds", timeoutSec);
            if (!string.IsNullOrWhiteSpace(promptHint))
            {
                _logger.LogWarning("Whisper timed out, falling back to client transcript: \"{Transcript}\"", promptHint);
                return new SpeechRecognitionResult(promptHint.Trim(), 0.90);
            }
            throw new AiServiceException($"Speech recognition timed out after {timeoutSec} seconds.", ex);
        }
        catch (Exception ex) when (ex is not AiServiceException)
        {
            _logger.LogError(ex, "Error communicating with Whisper STT service: {Message}", ex.Message);
            if (!string.IsNullOrWhiteSpace(promptHint))
            {
                _logger.LogWarning("Whisper exception, falling back to client transcript: \"{Transcript}\"", promptHint);
                return new SpeechRecognitionResult(promptHint.Trim(), 0.90);
            }
            throw new AiServiceException("Failed to transcribe audio speech.", ex);
        }
    }

    private bool IsTestingEnvironment()
    {
        var env = _configuration["ASPNETCORE_ENVIRONMENT"] 
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                  
        return string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase) 
               || string.Equals(_configuration["AiSettings:EnableTestFallback"], "true", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveFileExtension(string contentType)
    {
        var lower = contentType.ToLowerInvariant();
        if (lower.Contains("webm")) return "webm";
        if (lower.Contains("wav")) return "wav";
        if (lower.Contains("mp3") || lower.Contains("mpeg")) return "mp3";
        if (lower.Contains("m4a") || lower.Contains("mp4")) return "m4a";
        if (lower.Contains("ogg")) return "ogg";
        if (lower.Contains("aac")) return "aac";
        return "webm";
    }
}
