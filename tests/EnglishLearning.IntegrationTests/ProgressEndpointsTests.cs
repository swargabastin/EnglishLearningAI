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

public class ProgressEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProgressEndpointsTests(WebApplicationFactory<Program> factory)
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

    private async Task<(string token, Guid userId)> RegisterAndLoginAsync(string email = "progress_user@example.com")
    {
        var regPayload = new RegisterRequest(
            "Progress Tester",
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
    public async Task GetDashboard_WithAuthenticatedUser_ShouldReturn_200OkAndDashboardData()
    {
        // Arrange
        var (token, _) = await RegisterAndLoginAsync("dashboard_test@example.com");

        // 1. Complete a session with assessment
        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = JsonContent.Create(new StartConversationRequest(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "B2"
            ))
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var startResp = await _client.SendAsync(startRequest);
        var startBody = await startResp.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // Send message
        var msgReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/messages")
        {
            Content = JsonContent.Create(new SendMessageRequest("I am discussing daily routines and habits."))
        };
        msgReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(msgReq);

        // End session
        var endReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/end");
        endReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(endReq);

        // Assess session
        var assessReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/assessment");
        assessReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(assessReq);

        // Act: GET dashboard
        var dashReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/progress/dashboard");
        dashReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var dashResp = await _client.SendAsync(dashReq);

        // Assert
        dashResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashBody = await dashResp.Content.ReadAsStringAsync();
        var dashData = JsonSerializer.Deserialize<ApiResponse<DashboardResponseDto>>(dashBody, _jsonOptions);

        dashData.Should().NotBeNull();
        dashData!.Success.Should().BeTrue();
        dashData.Data!.User.Name.Should().Be("Progress Tester");
        dashData.Data.Stats.TotalCompletedSessions.Should().Be(1);
        dashData.Data.Stats.AverageOverallScore.Should().BeGreaterThan(0);
        dashData.Data.RecentSessions.Should().HaveCount(1);
        dashData.Data.ScoreProgression.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetHistory_WithPagination_ShouldReturn_200OkAndPaginatedList()
    {
        // Arrange
        var (token, _) = await RegisterAndLoginAsync("history_test@example.com");

        // Act
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/progress/history?page=1&pageSize=10");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = await _client.SendAsync(req);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ApiResponse<PaginatedListDto<HistoryItemDto>>>(body, _jsonOptions);

        data.Should().NotBeNull();
        data!.Data!.Page.Should().Be(1);
        data.Data.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetSessionDetail_WithOwnedSession_ShouldReturn_200OkAndDetail()
    {
        // Arrange
        var (token, _) = await RegisterAndLoginAsync("detail_test@example.com");

        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = JsonContent.Create(new StartConversationRequest(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "B1"
            ))
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var startResp = await _client.SendAsync(startRequest);
        var startBody = await startResp.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // Act: GET session detail
        var detailReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/progress/sessions/{sessionId}");
        detailReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var detailResp = await _client.SendAsync(detailReq);

        // Assert
        detailResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var detailBody = await detailResp.Content.ReadAsStringAsync();
        var detailData = JsonSerializer.Deserialize<ApiResponse<SessionDetailDto>>(detailBody, _jsonOptions);

        detailData.Should().NotBeNull();
        detailData!.Data!.SessionId.Should().Be(sessionId);
        detailData.Data.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDashboard_WithoutAuth_ShouldReturn_401Unauthorized()
    {
        // Act
        var resp = await _client.GetAsync("/api/v1/progress/dashboard");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSessionDetail_FromDifferentUser_ShouldReturn_404NotFound()
    {
        // Arrange
        var (token1, _) = await RegisterAndLoginAsync("owner1@example.com");
        var (token2, _) = await RegisterAndLoginAsync("owner2@example.com");

        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = JsonContent.Create(new StartConversationRequest(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "B1"
            ))
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var startResp = await _client.SendAsync(startRequest);
        var startBody = await startResp.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // Act: User 2 requests User 1's session detail
        var detailReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/progress/sessions/{sessionId}");
        detailReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var detailResp = await _client.SendAsync(detailReq);

        // Assert
        detailResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
