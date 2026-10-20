using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishLearning.IntegrationTests;

public class SecurityAndHardeningTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public SecurityAndHardeningTests(WebApplicationFactory<Program> factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Environment", "Testing");
            builder.UseSetting("AiSettings:EnableTestFallback", "true");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var dbName = Guid.NewGuid().ToString();
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        });

        _client = customFactory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private async Task<(string token, Guid userId)> RegisterAndLoginAsync(string email, string name = "Security Tester")
    {
        var regPayload = new RegisterRequest(
            name,
            email,
            "SecurePassword123!",
            "SecurePassword123!",
            "B2",
            "SpeakingFluency"
        );

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", regPayload);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var apiResult = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(body, _jsonOptions);

        return (apiResult!.Data!.AccessToken, apiResult.Data.User.Id);
    }

    [Theory]
    [InlineData("/api/v1/auth/me", "GET")]
    [InlineData("/api/v1/conversations", "POST")]
    [InlineData("/api/v1/conversations/11111111-1111-1111-1111-111111111111", "GET")]
    [InlineData("/api/v1/conversations/11111111-1111-1111-1111-111111111111/messages", "POST")]
    [InlineData("/api/v1/conversations/11111111-1111-1111-1111-111111111111/speech", "POST")]
    [InlineData("/api/v1/conversations/11111111-1111-1111-1111-111111111111/end", "POST")]
    [InlineData("/api/v1/conversations/11111111-1111-1111-1111-111111111111/assessment", "POST")]
    [InlineData("/api/v1/conversations/11111111-1111-1111-1111-111111111111/assessment", "GET")]
    [InlineData("/api/v1/progress/dashboard", "GET")]
    [InlineData("/api/v1/progress/history", "GET")]
    [InlineData("/api/v1/progress/sessions/11111111-1111-1111-1111-111111111111", "GET")]
    public async Task UnauthenticatedRequest_ToAllProtectedEndpoints_Returns_401Unauthorized(string endpoint, string method)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), endpoint);
        if (endpoint.Contains("/speech"))
        {
            var form = new MultipartFormDataContent();
            var audio = new ByteArrayContent(new byte[16]);
            audio.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");
            form.Add(audio, "audio", "test.webm");
            request.Content = form;
        }
        else if (method == "POST")
        {
            request.Content = JsonContent.Create(new { });
        }

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"Endpoint {endpoint} must require authentication.");
    }

    [Fact]
    public async Task MalformedJwtToken_ShouldReturn_401Unauthorized()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/progress/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "malformed.invalid.jwt.token");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UserIsolation_UserBCannotAccess_UserASessionData()
    {
        // Arrange
        var (tokenA, userAId) = await RegisterAndLoginAsync("user_a@example.com", "User Alpha");
        var (tokenB, _) = await RegisterAndLoginAsync("user_b@example.com", "User Beta");

        // 1. User A starts session
        var startReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = JsonContent.Create(new StartConversationRequest(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "B2"
            ))
        };
        startReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var startResp = await _client.SendAsync(startReq);
        var startBody = await startResp.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // 2. User A sends message & completes
        var msgReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/messages")
        {
            Content = JsonContent.Create(new SendMessageRequest("Learning English daily."))
        };
        msgReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        await _client.SendAsync(msgReq);

        var endReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/end");
        endReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        await _client.SendAsync(endReq);

        // 3. User B attempts to access User A's session:
        // - GET session
        var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/conversations/{sessionId}");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var getResp = await _client.SendAsync(getReq);
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // - POST assessment
        var assessReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/assessment");
        assessReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var assessResp = await _client.SendAsync(assessReq);
        assessResp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // - GET session detail
        var detailReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/progress/sessions/{sessionId}");
        detailReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var detailResp = await _client.SendAsync(detailReq);
        detailResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteE2EFlow_ScenarioA_TextConversation_End_Assess_Dashboard_History()
    {
        // 1. Register & Login
        var (token, _) = await RegisterAndLoginAsync("e2e_text@example.com", "E2E Text Learner");

        // 2. View topics
        var topicsResp = await _client.GetAsync("/api/v1/topics");
        topicsResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var topicsBody = await topicsResp.Content.ReadAsStringAsync();
        var topics = JsonSerializer.Deserialize<ApiResponse<IReadOnlyList<TopicDto>>>(topicsBody, _jsonOptions);
        var topicId = topics!.Data![0].Id;

        // 3. Start conversation
        var startReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = JsonContent.Create(new StartConversationRequest(topicId, "B2"))
        };
        startReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var startResp = await _client.SendAsync(startReq);
        startResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(await startResp.Content.ReadAsStringAsync(), _jsonOptions)!.Data!;

        // 4. Exchange 2 messages
        for (int i = 1; i <= 2; i++)
        {
            var msgReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{session.Id}/messages")
            {
                Content = JsonContent.Create(new SendMessageRequest($"This is message number {i} discussing English topics."))
            };
            msgReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var msgResp = await _client.SendAsync(msgReq);
            msgResp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // 5. End conversation
        var endReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{session.Id}/end");
        endReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var endResp = await _client.SendAsync(endReq);
        endResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Generate Assessment
        var assessReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{session.Id}/assessment");
        assessReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var assessResp = await _client.SendAsync(assessReq);
        assessResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var assessment = JsonSerializer.Deserialize<ApiResponse<AssessmentResponseDto>>(await assessResp.Content.ReadAsStringAsync(), _jsonOptions)!.Data!;
        assessment.OverallScore.Should().BeGreaterThan(0);
        assessment.GrammarScore.Should().BeGreaterThan(0);

        // 7. Verify Dashboard
        var dashReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/progress/dashboard");
        dashReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var dashResp = await _client.SendAsync(dashReq);
        dashResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var dash = JsonSerializer.Deserialize<ApiResponse<DashboardResponseDto>>(await dashResp.Content.ReadAsStringAsync(), _jsonOptions)!.Data!;
        dash.Stats.TotalCompletedSessions.Should().Be(1);
        dash.Stats.AverageOverallScore.Should().Be(assessment.OverallScore);

        // 8. Verify History
        var histReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/progress/history?page=1&pageSize=10");
        histReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var histResp = await _client.SendAsync(histReq);
        histResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var hist = JsonSerializer.Deserialize<ApiResponse<PaginatedListDto<HistoryItemDto>>>(await histResp.Content.ReadAsStringAsync(), _jsonOptions)!.Data!;
        hist.TotalCount.Should().Be(1);
        hist.Items[0].OverallScore.Should().Be(assessment.OverallScore);

        // 9. Verify Session Detail
        var detailReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/progress/sessions/{session.Id}");
        detailReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var detailResp = await _client.SendAsync(detailReq);
        detailResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = JsonSerializer.Deserialize<ApiResponse<SessionDetailDto>>(await detailResp.Content.ReadAsStringAsync(), _jsonOptions)!.Data!;
        detail.Messages.Should().HaveCountGreaterThanOrEqualTo(4); // 1 initial + 2 user + 2 ai
        detail.Assessment.Should().NotBeNull();
        detail.Assessment!.OverallScore.Should().Be(assessment.OverallScore);
    }

    [Fact]
    public async Task CompleteE2EFlow_ScenarioB_SpeechConversation_End_Assess()
    {
        // 1. Register & Login
        var (token, _) = await RegisterAndLoginAsync("e2e_speech@example.com", "E2E Speech Learner");

        // 2. Start conversation
        var startReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = JsonContent.Create(new StartConversationRequest(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "B1"
            ))
        };
        startReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var startResp = await _client.SendAsync(startReq);
        var session = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(await startResp.Content.ReadAsStringAsync(), _jsonOptions)!.Data!;

        // 3. Send Speech multipart/form-data
        using var form = new MultipartFormDataContent();
        var fakeAudioBytes = new byte[256];
        var audioContent = new ByteArrayContent(fakeAudioBytes);
        audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");
        form.Add(audioContent, "audio", "mic_recording.webm");

        var speechReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{session.Id}/speech")
        {
            Content = form
        };
        speechReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var speechResp = await _client.SendAsync(speechReq);
        speechResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var speechData = JsonSerializer.Deserialize<ApiResponse<SpeechConversationResponseDto>>(await speechResp.Content.ReadAsStringAsync(), _jsonOptions)!.Data!;
        speechData.RecognizedText.Should().NotBeNullOrWhiteSpace();
        speechData.AiMessage.MessageText.Should().NotBeNullOrWhiteSpace();

        // 4. End conversation
        var endReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{session.Id}/end");
        endReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(endReq);

        // 5. Generate Assessment
        var assessReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{session.Id}/assessment");
        assessReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var assessResp = await _client.SendAsync(assessReq);
        assessResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
