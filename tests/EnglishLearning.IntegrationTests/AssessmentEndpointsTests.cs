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

public class AssessmentEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AssessmentEndpointsTests(WebApplicationFactory<Program> factory)
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

    private async Task<(string token, Guid userId)> RegisterAndLoginAsync(string email = "assessmentuser@example.com")
    {
        var regPayload = new RegisterRequest(
            "Assessment Tester",
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

    [Fact]
    public async Task GenerateAssessment_WithCompletedSession_ShouldReturn_200OkAndAssessment()
    {
        // Arrange
        var (token, _) = await RegisterAndLoginAsync("completed_assessment@example.com");

        // 1. Start session
        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = JsonContent.Create(new StartConversationRequest(
                Guid.Parse("11111111-1111-1111-1111-111111111111"), // Daily Conversation
                "B2"
            ))
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var startResponse = await _client.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();

        var startBody = await startResponse.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // 2. Send user message
        var msgRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/messages")
        {
            Content = JsonContent.Create(new SendMessageRequest("I have been practicing English conversation everyday."))
        };
        msgRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var msgResponse = await _client.SendAsync(msgRequest);
        msgResponse.EnsureSuccessStatusCode();

        // 3. End session
        var endRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/end");
        endRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var endResponse = await _client.SendAsync(endRequest);
        endResponse.EnsureSuccessStatusCode();

        // 4. Generate assessment
        var assessRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/assessment");
        assessRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var assessResponse = await _client.SendAsync(assessRequest);

        // Assert
        var assessBody = await assessResponse.Content.ReadAsStringAsync();
        assessResponse.StatusCode.Should().Be(HttpStatusCode.OK, assessBody);

        var assessData = JsonSerializer.Deserialize<ApiResponse<AssessmentResponseDto>>(assessBody, _jsonOptions);
        assessData.Should().NotBeNull();
        assessData!.Success.Should().BeTrue();
        assessData.Data!.SessionId.Should().Be(sessionId);
        assessData.Data.OverallScore.Should().BeGreaterThan(0);
        assessData.Data.GrammarScore.Should().BeGreaterThan(0);
        assessData.Data.VocabularyScore.Should().BeGreaterThan(0);
        assessData.Data.FluencyScore.Should().BeGreaterThan(0);
        assessData.Data.Feedback.Should().NotBeNullOrWhiteSpace();
        assessData.Data.Strengths.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAssessment_WithExistingAssessment_ShouldReturn_200Ok()
    {
        // Arrange
        var (token, _) = await RegisterAndLoginAsync("get_assessment@example.com");

        // Start session
        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = JsonContent.Create(new StartConversationRequest(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "B1"
            ))
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var startResponse = await _client.SendAsync(startRequest);
        var startBody = await startResponse.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // Message & End
        var msgRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/messages")
        {
            Content = JsonContent.Create(new SendMessageRequest("I like to travel to new cities during holidays."))
        };
        msgRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(msgRequest);

        var endRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/end");
        endRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(endRequest);

        // Generate Assessment first
        var genRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/assessment");
        genRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(genRequest);

        // Act: GET assessment
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/conversations/{sessionId}/assessment");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getResponse = await _client.SendAsync(getRequest);

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var getData = JsonSerializer.Deserialize<ApiResponse<AssessmentResponseDto>>(getBody, _jsonOptions);
        getData.Should().NotBeNull();
        getData!.Data!.SessionId.Should().Be(sessionId);
        getData.Data.OverallScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateAssessment_WithoutAuth_ShouldReturn_401Unauthorized()
    {
        // Act
        var response = await _client.PostAsync($"/api/v1/conversations/{Guid.NewGuid()}/assessment", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateAssessment_OnActiveSession_ShouldReturn_400BadRequest()
    {
        // Arrange
        var (token, _) = await RegisterAndLoginAsync("active_session_assess@example.com");

        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = JsonContent.Create(new StartConversationRequest(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "A2"
            ))
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var startResponse = await _client.SendAsync(startRequest);
        var startBody = await startResponse.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // Act: try generating assessment while still Active
        var assessRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/assessment");
        assessRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var assessResponse = await _client.SendAsync(assessRequest);

        // Assert
        assessResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateAssessment_FromDifferentUser_ShouldReturn_404NotFound()
    {
        // Arrange: User 1 creates and completes session
        var (token1, _) = await RegisterAndLoginAsync("owner_user@example.com");
        var (token2, _) = await RegisterAndLoginAsync("other_user@example.com");

        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = JsonContent.Create(new StartConversationRequest(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "B1"
            ))
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var startResponse = await _client.SendAsync(startRequest);
        var startBody = await startResponse.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // End session as User 1
        var endRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/end");
        endRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        await _client.SendAsync(endRequest);

        // Act: User 2 attempts to assess User 1's session
        var assessRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/assessment");
        assessRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var assessResponse = await _client.SendAsync(assessRequest);

        // Assert: 404 Not Found (privacy protection)
        assessResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
