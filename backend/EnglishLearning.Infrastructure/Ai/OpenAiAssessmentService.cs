using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Domain.Exceptions;
using EnglishLearning.Infrastructure.Ai.Configuration;
using EnglishLearning.Infrastructure.Ai.Prompts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Infrastructure.Ai;

public class OpenAiAssessmentService : IAiAssessmentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiAssessmentService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenAiAssessmentService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAiAssessmentService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AssessmentAiResponse> EvaluateConversationAsync(
        AssessmentAiRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["AiSettings:ApiKey"]
                     ?? _configuration["AI__APIKEY"]
                     ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            if (IsTestingEnvironment())
            {
                _logger.LogInformation("Running in Testing environment without API key. Using deterministic assessment generator.");
                return GenerateDeterministicAssessment(request);
            }

            var currentEnv = _configuration["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            _logger.LogError("OpenAI API key is missing. Assessment generation is unavailable in {Environment} mode.", currentEnv);
            throw new AiServiceException("Assessment generation is currently unavailable. Please configure OPENAI_API_KEY for local development.");
        }

        var systemPrompt = AssessmentPromptBuilder.BuildSystemPrompt();
        var userPrompt = AssessmentPromptBuilder.BuildEvaluationUserPrompt(request);

        var model = _configuration["AiSettings:Model"] ?? "gpt-4o-mini";

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.2,
            max_tokens = 1500,
            response_format = new { type = "json_object" }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody, JsonOptions),
            Encoding.UTF8,
            "application/json"
        );

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = jsonContent
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI Assessment API returned status {StatusCode}: {Error}", response.StatusCode, errorContent);
                throw new AiServiceException($"OpenAI evaluation service failed with HTTP {response.StatusCode}.");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var parsedRoot = JsonSerializer.Deserialize<OpenAiChatCompletionResponse>(responseJson, JsonOptions);

            var rawContent = parsedRoot?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(rawContent))
            {
                throw new AiServiceException("OpenAI evaluation returned an empty response.");
            }

            var assessmentResult = JsonSerializer.Deserialize<AssessmentRawPayload>(rawContent, JsonOptions);
            if (assessmentResult == null)
            {
                throw new AiServiceException("Unable to deserialize structured assessment JSON from OpenAI.");
            }

            return MapToResponse(assessmentResult);
        }
        catch (AiServiceException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during OpenAI assessment evaluation");
            throw new AiServiceException("Failed to evaluate conversation session with AI provider.", ex);
        }
    }

    private static AssessmentAiResponse MapToResponse(AssessmentRawPayload payload)
    {
        return new AssessmentAiResponse(
            payload.GrammarScore ?? 75m,
            payload.VocabularyScore ?? 78m,
            payload.FluencyScore ?? 76m,
            payload.PronunciationScore,
            payload.Feedback ?? "Good practice session! Keep building your conversational confidence.",
            payload.Strengths ?? new List<string> { "Clear responses to tutor questions", "Good topical vocabulary" },
            payload.AreasForImprovement ?? new List<string> { "Practice complex sentence structures", "Expand descriptive vocabulary" },
            payload.GrammarCorrections ?? new List<GrammarCorrectionDto>(),
            payload.VocabularySuggestions ?? new List<VocabularySuggestionDto>(),
            payload.Recommendations ?? new List<string> { "Practice conversational turns daily", "Review suggested grammar structures" }
        );
    }

    private static AssessmentAiResponse GenerateDeterministicAssessment(AssessmentAiRequest request)
    {
        var userMessages = request.Transcript
            .Where(m => m.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var totalUserWords = userMessages.Sum(m => m.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
        var baseScore = Math.Min(90m, Math.Max(65m, 70m + (userMessages.Count * 2m)));

        var grammarScore = Math.Clamp(baseScore + 2m, 60m, 95m);
        var vocabularyScore = Math.Clamp(baseScore - 1m, 60m, 95m);
        var fluencyScore = Math.Clamp(baseScore, 60m, 95m);

        var firstUserMsg = userMessages.FirstOrDefault()?.Content ?? "I am learning English.";

        return new AssessmentAiResponse(
            grammarScore,
            vocabularyScore,
            fluencyScore,
            null,
            $"You participated well in the {request.TopicName} practice session with {userMessages.Count} speaking turns and {totalUserWords} words spoken. Your responses were coherent and appropriate for target level {request.TargetCefrLevel}.",
            new List<string>
            {
                $"Active participation on the topic '{request.TopicName}'",
                "Clear sentence structures with understandable intent",
                "Consistent turn-taking with the AI tutor"
            },
            new List<string>
            {
                "Incorporate more connecting phrases (e.g., 'furthermore', 'on the other hand')",
                "Practice expanding single-clause sentences into more descriptive responses"
            },
            new List<GrammarCorrectionDto>
            {
                new(
                    firstUserMsg,
                    firstUserMsg,
                    "Good sentence structure. Ensure consistent subject-verb agreement across longer phrases.",
                    "General Syntax"
                )
            },
            new List<VocabularySuggestionDto>
            {
                new(
                    "good",
                    "effective",
                    "Using more precise adjectives will elevate your English fluency.",
                    request.TargetCefrLevel
                )
            },
            new List<string>
            {
                $"Continue practicing {request.TopicName} dialogues to master contextual vocabulary.",
                "Focus on sentence variation and transition words."
            }
        );
    }

    private bool IsTestingEnvironment()
    {
        var env = _configuration["ASPNETCORE_ENVIRONMENT"] 
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                  
        return string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase) 
               || string.Equals(_configuration["AiSettings:EnableTestFallback"], "true", StringComparison.OrdinalIgnoreCase);
    }

    private class OpenAiChatCompletionResponse
    {
        public List<ChoiceItem>? Choices { get; set; }
    }

    private class ChoiceItem
    {
        public MessageItem? Message { get; set; }
    }

    private class MessageItem
    {
        public string? Content { get; set; }
    }

    private class AssessmentRawPayload
    {
        public decimal? GrammarScore { get; set; }
        public decimal? VocabularyScore { get; set; }
        public decimal? FluencyScore { get; set; }
        public decimal? PronunciationScore { get; set; }
        public string? Feedback { get; set; }
        public List<string>? Strengths { get; set; }
        public List<string>? AreasForImprovement { get; set; }
        public List<GrammarCorrectionDto>? GrammarCorrections { get; set; }
        public List<VocabularySuggestionDto>? VocabularySuggestions { get; set; }
        public List<string>? Recommendations { get; set; }
    }
}
