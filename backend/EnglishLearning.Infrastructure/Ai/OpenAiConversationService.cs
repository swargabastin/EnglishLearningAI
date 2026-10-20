using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Domain.Exceptions;
using EnglishLearning.Infrastructure.Ai.Configuration;
using EnglishLearning.Infrastructure.Ai.Prompts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Infrastructure.Ai;

public class OpenAiConversationService : IAiConversationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiConversationService> _logger;

    public OpenAiConversationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAiConversationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AiConversationResponse> GenerateResponseAsync(
        AiConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["AiSettings:ApiKey"] 
                     ?? _configuration["AI__APIKEY"] 
                     ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        var model = _configuration["AiSettings:Model"] ?? "gpt-4o-mini";
        var maxTokens = int.TryParse(_configuration["AiSettings:MaxTokens"], out var mt) ? mt : 500;
        var temperature = double.TryParse(_configuration["AiSettings:Temperature"], out var temp) ? temp : 0.7;
        var timeoutSec = int.TryParse(_configuration["AiSettings:TimeoutSeconds"], out var ts) ? ts : 30;

        // Fallback permitted ONLY in explicit Testing environment (e.g. automated test runners)
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            if (IsTestingEnvironment())
            {
                _logger.LogInformation("Running in Testing environment without API key. Using deterministic test tutor response engine for topic '{Topic}' (CEFR {Level})", 
                    request.TopicName, request.EnglishLevel);
                return GenerateSimulatedResponse(request);
            }

            var currentEnv = _configuration["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            _logger.LogError("OpenAI API key is missing. AI conversation is unavailable in {Environment} mode.", currentEnv);
            throw new AiServiceException("AI conversation is currently unavailable. Please configure OPENAI_API_KEY for local development.");
        }

        var systemPrompt = TutorPromptBuilder.BuildSystemPrompt(request);

        var messagesList = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        if (request.Messages.Count == 0)
        {
            // First turn greeting request
            messagesList.Add(new
            {
                role = "user",
                content = $"Please initiate our English speaking session on \"{request.TopicName}\" with a warm greeting and an opening question suitable for my {request.EnglishLevel} level."
            });
        }
        else
        {
            foreach (var msg in request.Messages)
            {
                messagesList.Add(new
                {
                    role = msg.Role.ToLowerInvariant() == "user" ? "user" : "assistant",
                    content = msg.Content
                });
            }
        }

        var requestBody = new
        {
            model = model,
            messages = messagesList,
            max_tokens = maxTokens,
            temperature = temperature
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
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = jsonContent;

            var response = await _httpClient.SendAsync(httpRequest, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogError("OpenAI API call failed with status code {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                throw new AiServiceException($"AI service returned error status {response.StatusCode}.");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(responseJson);

            var root = doc.RootElement;
            var choices = root.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                throw new AiServiceException("AI service returned an empty response choices array.");
            }

            var reply = choices[0].GetProperty("message").GetProperty("content").GetString();
            if (string.IsNullOrWhiteSpace(reply))
            {
                throw new AiServiceException("AI service returned an empty response string.");
            }

            string? pedagogicalTip = null;
            var tipIndex = reply.IndexOf("[TIP:", StringComparison.OrdinalIgnoreCase);
            if (tipIndex >= 0)
            {
                var tipEnd = reply.IndexOf(']', tipIndex);
                if (tipEnd > tipIndex)
                {
                    pedagogicalTip = reply.Substring(tipIndex + 5, tipEnd - (tipIndex + 5)).Trim();
                    reply = reply.Remove(tipIndex, (tipEnd - tipIndex) + 1).Trim();
                }
            }

            return new AiConversationResponse(reply.Trim(), pedagogicalTip);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "OpenAI API request timed out after {Timeout} seconds", timeoutSec);
            throw new AiServiceException($"AI service request timed out after {timeoutSec} seconds.", ex);
        }
        catch (Exception ex) when (ex is not AiServiceException)
        {
            _logger.LogError(ex, "Error communicating with AI service: {Message}", ex.Message);
            throw new AiServiceException("Failed to communicate with the AI tutor service.", ex);
        }
    }

    private bool IsTestingEnvironment()
    {
        var env = _configuration["ASPNETCORE_ENVIRONMENT"] 
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                  
        return string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase) 
               || string.Equals(_configuration["AiSettings:EnableTestFallback"], "true", StringComparison.OrdinalIgnoreCase);
    }

    private static AiConversationResponse GenerateSimulatedResponse(AiConversationRequest request)
    {
        if (request.Messages.Count == 0)
        {
            if (!string.IsNullOrWhiteSpace(request.TargetObjective))
            {
                return new AiConversationResponse(
                    $"Hello! I'm Emma, your English speaking coach. Today we are focusing on: {request.TargetObjective}! To start our conversation, could you tell me a little bit about your experience with this topic?",
                    $"Learning Goal: Practice using {request.TargetObjective} in full conversational sentences."
                );
            }

            var initialGreeting = request.TopicName switch
            {
                "Job Interview" => "Hello! Welcome to your interview practice session. I'm Emma, your interviewer today. To get started, could you briefly introduce yourself and tell me about your background?",
                "Travel" => "Hello! Ready for some travel conversation practice? Imagine you just arrived at your dream destination. Where are we traveling to today?",
                "Shopping" => "Hi there! Welcome to our shopping practice dialogue. What item are you looking to buy today?",
                "Restaurant" => "Good evening! Welcome to our restaurant. We have a table ready for you. Would you like to see the menu or start with a drink?",
                "Workplace" => "Hi! Thanks for joining today's team status meeting. What is the main update from your project this week?",
                "Introducing Yourself" => "Hello! I'm Emma, your English speaking coach. It's lovely to meet you! What is your name and where are you from?",
                _ => "Hello! I'm Emma, your English speaking coach. I'm excited to practice with you today! How has your day been so far?"
            };

            return new AiConversationResponse(initialGreeting);
        }

        var lastUserMsg = request.Messages.LastOrDefault(m => m.Role.ToLowerInvariant() == "user")?.Content?.Trim() ?? "";
        var lower = lastUserMsg.ToLowerInvariant();
        var words = lastUserMsg.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // 1. Detect ESL Grammar / Phrasing Mistakes
        string? tip = null;
        if (lower.Contains("am agree") || lower.Contains("i'm agree"))
        {
            tip = "Say \"I agree\" instead of \"I am agree\" — in English, \"agree\" is already a verb.";
        }
        else if (lower.Contains("years experience") && !lower.Contains("years of experience"))
        {
            tip = "Natural phrasing: Use \"years of experience\" with the preposition \"of\".";
        }
        else if (lower.Contains("good in ") || lower.Contains("good in english"))
        {
            tip = "Use \"good at\" for abilities and skills (e.g., \"I am good at communication\").";
        }
        else if (lower.Contains("discuss about"))
        {
            tip = "Say \"discuss the issue\" directly without \"about\" (discuss is a transitive verb).";
        }
        else if (lower.StartsWith("myself ") || lower.Contains(" myself is "))
        {
            tip = "In professional English, introduce yourself as \"I am [Name]\" or \"My name is [Name]\".";
        }
        else if (lower.Contains("explain me"))
        {
            tip = "Use \"explain to me\" — explain requires the preposition \"to\" before a person.";
        }
        else if (lower.Contains("passed out") && (lower.Contains("college") || lower.Contains("university") || lower.Contains("school") || lower.Contains("batch")))
        {
            tip = "Use \"graduated from\" instead of \"passed out\" — in English, \"passed out\" means lost consciousness!";
        }
        else if (lower.Contains("revert back"))
        {
            tip = "Say \"revert\" or \"reply\" — \"revert back\" is redundant since revert already means to return.";
        }

        // 2. Detect Greetings ("hi", "hello", "hey", "good morning", etc.)
        bool isGreeting = IsGreetingPhrase(lower);
        if (isGreeting)
        {
            var greetingReply = request.TopicName switch
            {
                "Job Interview" => "Hello! Nice to meet you. Thanks for joining our interview preparation session today. To start off, could you tell me a little bit about yourself and your professional background?",
                "Travel" => "Hello! It's great to connect. Imagine we've just met at the travel information desk. Where are you planning to travel next?",
                "Restaurant" => "Hello and welcome! It's wonderful to have you with us today. Would you like a table inside or on the terrace?",
                "Workplace" => "Good morning! Good to see you on the team call. How is your week going so far?",
                _ => "Hello! It's so nice to meet you. I'm Emma, your English speaking coach. How are you doing today, and what would you like to practice?"
            };

            // For pure greetings, no tip is needed unless an error was made
            return new AiConversationResponse(greetingReply, tip);
        }

        // 3. User Asking Questions
        if (lower.Contains("?") || lower.StartsWith("can you") || lower.StartsWith("what should") || lower.StartsWith("how do") || lower.StartsWith("how to"))
        {
            var questionReply = request.TopicName switch
            {
                "Job Interview" => "That's a thoughtful question! In an interview setting, concise structure is key. A proven technique is the STAR method (Situation, Task, Action, Result). Could you share a project or accomplishment you'd like to frame using that approach?",
                _ => "That's a great question! Practice and confidence are the most important steps. What specific situation would you like to focus on right now?"
            };

            return new AiConversationResponse(questionReply, tip);
        }

        // 4. Topic-Specific Multi-Turn Conversational Progression
        if (request.TopicName.Equals("Job Interview", StringComparison.OrdinalIgnoreCase))
        {
            // Self-introduction / background
            if (lower.Contains("engineer") || lower.Contains("developer") || lower.Contains("working as") || lower.Contains("work as") ||
                lower.Contains("student") || lower.Contains("graduate") || lower.Contains("my name") || lower.Contains("i am a") || lower.Contains("years of experience"))
            {
                tip ??= "Tip: In interviews, follow the 'Present, Past, Future' structure for introductions: what you do now, past accomplishments, and why you want this role.";
                return new AiConversationResponse(
                    "Thank you for that introduction! It sounds like you have a solid foundation. In this role, problem-solving is critical. What would you say is one of your greatest professional strengths, and how have you applied it recently?",
                    tip
                );
            }

            // Strengths / Skills
            if (lower.Contains("strength") || lower.Contains("skill") || lower.Contains("good at") || lower.Contains("responsible") ||
                lower.Contains("passion") || lower.Contains("leadership") || lower.Contains("solve") || lower.Contains("learning"))
            {
                tip ??= "Vocabulary tip: Use powerful action verbs like 'spearheaded', 'orchestrated', or 'streamlined' to highlight your impact.";
                return new AiConversationResponse(
                    "That's a valuable strength to bring to a team. In any workplace, challenges and tight deadlines are inevitable. Could you walk me through a difficult problem you encountered in a past project, and how you managed to resolve it?",
                    tip
                );
            }

            // Challenges / Problem Solving
            if (lower.Contains("problem") || lower.Contains("challenge") || lower.Contains("difficult") || lower.Contains("issue") ||
                lower.Contains("deadline") || lower.Contains("bug") || lower.Contains("fixed") || lower.Contains("solved") || lower.Contains("pressure"))
            {
                tip ??= "Tip: Emphasize the 'Result' phase of your answer — recruiters love quantifiable outcomes like 'reduced latency by 20%' or 'delivered on time'.";
                return new AiConversationResponse(
                    "That demonstrates strong resilience and critical thinking. Team collaboration is equally important. When you encounter a disagreement with a team member or stakeholder, how do you handle it to keep the project moving forward?",
                    tip
                );
            }

            // Teamwork / Conflict
            if (lower.Contains("team") || lower.Contains("colleague") || lower.Contains("manager") || lower.Contains("disagree") ||
                lower.Contains("communicate") || lower.Contains("listen") || lower.Contains("feedback") || lower.Contains("compromise"))
            {
                return new AiConversationResponse(
                    "That shows great emotional intelligence and communication skills. Lastly, where do you see yourself growing professionally over the next two to three years, and what excites you most about this opportunity?",
                    tip
                );
            }

            // Career Aspirations / Wrap-up
            if (lower.Contains("goal") || lower.Contains("future") || lower.Contains("grow") || lower.Contains("learn") || lower.Contains("lead") || lower.Contains("years"))
            {
                return new AiConversationResponse(
                    "That's an inspiring vision for your career path! You presented your thoughts with clarity and confidence. Do you have any questions for me about the company or team culture?",
                    tip
                );
            }
        }

        // 5. Very Short Answers (under 4 words)
        if (words.Length <= 3)
        {
            tip ??= "Fluency tip: Try speaking in complete sentences with more detail (e.g. \"Yes, I have worked with...\" instead of a brief answer).";
            return new AiConversationResponse(
                $"I understand! To help build your speaking confidence, could you elaborate a bit more on that? For instance, what was the most interesting part of that experience?",
                tip
            );
        }

        // 6. Natural General Dialogue Continuation
        tip ??= "Vocabulary upgrade: Try using discourse markers like \"In my experience,\" or \"On the other hand,\" to transition smoothly between ideas.";
        return new AiConversationResponse(
            "That's a clear explanation! You articulated your point well. What aspect of that did you find most rewarding, or is there another example you'd like to share?",
            tip
        );
    }

    private static bool IsGreetingPhrase(string lower)
    {
        var cleaned = lower.Trim('.', '!', ',', '?', ' ', '\'', '"');
        if (cleaned is "hi" or "hello" or "hey" or "good morning" or "good afternoon" or "good evening" or "hey emma" or "hi emma" or "hello emma")
        {
            return true;
        }

        return cleaned.StartsWith("hi ") 
               || cleaned.StartsWith("hello ") 
               || cleaned.StartsWith("hey ") 
               || cleaned.StartsWith("good morning") 
               || cleaned.StartsWith("good afternoon")
               || cleaned.StartsWith("good evening")
               || cleaned.StartsWith("how are you")
               || cleaned.StartsWith("nice to meet you");
    }
}
