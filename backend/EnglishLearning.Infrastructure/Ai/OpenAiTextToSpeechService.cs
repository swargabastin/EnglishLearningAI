using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Infrastructure.Ai;

public class OpenAiTextToSpeechService : ITextToSpeechService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiTextToSpeechService> _logger;

    public OpenAiTextToSpeechService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAiTextToSpeechService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TextToSpeechResult> SynthesizeAsync(
        string text,
        VoiceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["AiSettings:ApiKey"]
                     ?? _configuration["AI__APIKEY"]
                     ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        var ttsModel = _configuration["AiSettings:TtsModel"] ?? "tts-1";
        var defaultVoice = _configuration["AiSettings:TtsVoice"] ?? "nova";
        var timeoutSec = int.TryParse(_configuration["AiSettings:TtsTimeoutSeconds"], out var ts) ? ts : 25;

        // Fallback permitted ONLY in explicit Testing environment (e.g. automated test runners)
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            if (IsTestingEnvironment())
            {
                _logger.LogInformation("Running in Testing environment without API key. Using deterministic test audio synthesizer.");
                var fallbackMp3 = GenerateFallbackMp3Bytes();
                return new TextToSpeechResult(fallbackMp3, "audio/mpeg", TimeSpan.FromSeconds(3));
            }

            var currentEnv = _configuration["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            _logger.LogError("OpenAI API key is missing. Text-to-speech is unavailable in {Environment} mode.", currentEnv);
            throw new AiServiceException("Text-to-speech is currently unavailable. Please configure OPENAI_API_KEY for local development.");
        }

        var voice = options?.Voice ?? defaultVoice;
        var speed = options?.Speed ?? 1.0f;

        var requestBody = new
        {
            model = ttsModel,
            input = text,
            voice = voice,
            speed = speed,
            response_format = "mp3"
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSec));

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/speech");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = jsonContent;

            var response = await _httpClient.SendAsync(httpRequest, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogError("OpenAI TTS call failed with status code {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                throw new AiServiceException($"Text-to-speech service returned error status {response.StatusCode}.");
            }

            var audioBytes = await response.Content.ReadAsByteArrayAsync(cts.Token);
            if (audioBytes == null || audioBytes.Length == 0)
            {
                throw new AiServiceException("Text-to-speech service returned empty audio data.");
            }

            return new TextToSpeechResult(audioBytes, "audio/mpeg");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "OpenAI TTS request timed out after {Timeout} seconds", timeoutSec);
            throw new AiServiceException($"Text-to-speech timed out after {timeoutSec} seconds.", ex);
        }
        catch (Exception ex) when (ex is not AiServiceException)
        {
            _logger.LogError(ex, "Error communicating with OpenAI TTS service: {Message}", ex.Message);
            throw new AiServiceException("Failed to synthesize speech audio.", ex);
        }
    }

    private bool IsTestingEnvironment()
    {
        var env = _configuration["ASPNETCORE_ENVIRONMENT"] 
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                  
        return string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase) 
               || string.Equals(_configuration["AiSettings:EnableTestFallback"], "true", StringComparison.OrdinalIgnoreCase);
    }

    private static byte[] GenerateFallbackMp3Bytes()
    {
        // Minimal valid MPEG Audio Layer III frame header (sync word 0xFFFB)
        var frame = new byte[512];
        frame[0] = 0xFF;
        frame[1] = 0xFB;
        frame[2] = 0x90;
        frame[3] = 0x64;
        return frame;
    }
}
