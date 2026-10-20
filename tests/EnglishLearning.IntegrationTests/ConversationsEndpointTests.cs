using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishLearning.IntegrationTests;

public class ConversationsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ConversationsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
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

        _client = _factory.CreateClient();
    }

    private async Task<string> RegisterAndGetTokenAsync(string email)
    {
        var registerRequest = new RegisterRequest(
            "Learner " + Guid.NewGuid().ToString().Substring(0, 5),
            email,
            "Password123!",
            "Password123!",
            "B1",
            "SpeakingFluency"
        );

        var content = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/register", content);
        var body = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(body, _jsonOptions);
        return data!.Data!.AccessToken;
    }

    [Fact]
    public async Task StartConversation_WithoutAuth_ShouldReturn_401Unauthorized()
    {
        var request = new StartConversationRequest(Guid.Parse("11111111-1111-1111-1111-111111111111"), "A1");
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/conversations", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task StartConversation_WithAuth_ShouldReturn_200OkAndInitialGreeting()
    {
        // Arrange
        var token = await RegisterAndGetTokenAsync("start_test@example.com");
        var topicId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Daily Conversation

        var request = new StartConversationRequest(topicId, "A1");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(body, _jsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.TopicName.Should().Be("Daily Conversation");
        apiResponse.Data.EnglishLevel.Should().Be("A1");
        apiResponse.Data.Messages.Should().HaveCount(1);
        apiResponse.Data.Messages![0].Role.Should().Be("assistant");
    }

    [Fact]
    public async Task SendMessage_WithAuthOwner_ShouldReturn_200OkAndAiReply()
    {
        // Arrange
        var token = await RegisterAndGetTokenAsync("send_msg_test@example.com");
        var topicId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Job Interview

        // Start session
        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = new StringContent(JsonSerializer.Serialize(new StartConversationRequest(topicId, "B2")), Encoding.UTF8, "application/json")
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var startResponse = await _client.SendAsync(startRequest);
        var startBody = await startResponse.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // Act: Send message
        var msgRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(new SendMessageRequest("I have 5 years of software engineering experience.")), Encoding.UTF8, "application/json")
        };
        msgRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var msgResponse = await _client.SendAsync(msgRequest);

        var msgBody = await msgResponse.Content.ReadAsStringAsync();
        msgResponse.StatusCode.Should().Be(HttpStatusCode.OK, msgBody);
        var msgData = JsonSerializer.Deserialize<ApiResponse<SendMessageResponseDto>>(msgBody, _jsonOptions);

        msgData.Should().NotBeNull();
        msgData!.Success.Should().BeTrue();
        msgData.Data!.UserMessage.MessageText.Should().Be("I have 5 years of software engineering experience.");
        msgData.Data.AiMessage.Role.Should().Be("assistant");
        msgData.Data.Session.Messages.Should().HaveCount(3); // Initial greeting + User msg + AI reply
    }

    [Fact]
    public async Task SendMessage_FromDifferentUser_ShouldReturn_404NotFound()
    {
        // Arrange: User 1 starts session
        var tokenUser1 = await RegisterAndGetTokenAsync("user1@example.com");
        var tokenUser2 = await RegisterAndGetTokenAsync("user2@example.com");
        var topicId = Guid.Parse("33333333-3333-3333-3333-333333333333"); // Travel

        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = new StringContent(JsonSerializer.Serialize(new StartConversationRequest(topicId, "B1")), Encoding.UTF8, "application/json")
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenUser1);
        var startResponse = await _client.SendAsync(startRequest);
        var startBody = await startResponse.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // Act: User 2 tries to send message to User 1's session
        var msgRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(new SendMessageRequest("Attempting intrusion")), Encoding.UTF8, "application/json")
        };
        msgRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenUser2);

        var msgResponse = await _client.SendAsync(msgRequest);

        // Assert
        msgResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EndConversation_ShouldCompleteSession()
    {
        // Arrange
        var token = await RegisterAndGetTokenAsync("end_session_test@example.com");
        var topicId = Guid.Parse("44444444-4444-4444-4444-444444444444"); // Shopping

        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = new StringContent(JsonSerializer.Serialize(new StartConversationRequest(topicId, "A2")), Encoding.UTF8, "application/json")
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var startResponse = await _client.SendAsync(startRequest);
        var startBody = await startResponse.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        var sessionId = startData!.Data!.Id;

        // Act: End session
        var endRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/end");
        endRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var endResponse = await _client.SendAsync(endRequest);

        // Assert
        endResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var endBody = await endResponse.Content.ReadAsStringAsync();
        var endData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(endBody, _jsonOptions);

        endData!.Data!.Status.Should().Be("Completed");
    }
}
