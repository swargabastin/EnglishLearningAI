using System.Net;
using System.Net.Http.Headers;
using System.Text;
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

public class SpeechEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SpeechEndpointTests(WebApplicationFactory<Program> factory)
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
            "Speech Learner " + Guid.NewGuid().ToString().Substring(0, 5),
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

    private async Task<Guid> StartSessionAsync(string token, Guid topicId)
    {
        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/conversations")
        {
            Content = new StringContent(JsonSerializer.Serialize(new StartConversationRequest(topicId, "B1")), Encoding.UTF8, "application/json")
        };
        startRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var startResponse = await _client.SendAsync(startRequest);
        var startBody = await startResponse.Content.ReadAsStringAsync();
        var startData = JsonSerializer.Deserialize<ApiResponse<ConversationSessionDto>>(startBody, _jsonOptions);
        return startData!.Data!.Id;
    }

    [Fact]
    public async Task SendSpeech_WithoutAuth_ShouldReturn_401Unauthorized()
    {
        var sessionId = Guid.NewGuid();
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent(new byte[100]), "audio", "test.webm");

        var response = await _client.PostAsync($"/api/v1/conversations/{sessionId}/speech", multipart);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendSpeech_WithValidAudio_ShouldReturn_200OkAndSpeechResponse()
    {
        // Arrange
        var token = await RegisterAndGetTokenAsync("speech_test@example.com");
        var topicId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Daily Conversation
        var sessionId = await StartSessionAsync(token, topicId);

        using var multipart = new MultipartFormDataContent();
        var audioBytes = Encoding.UTF8.GetBytes("RIFF1234WAVEfmt mock audio data content");
        var audioContent = new ByteArrayContent(audioBytes);
        audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");
        multipart.Add(audioContent, "audio", "recording.webm");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/speech")
        {
            Content = multipart
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<SpeechConversationResponseDto>>(body, _jsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.RecognizedText.Should().NotBeNullOrWhiteSpace();
        apiResponse.Data.UserMessage.Should().NotBeNull();
        apiResponse.Data.AiMessage.Should().NotBeNull();
        apiResponse.Data.AiMessage.Role.Should().Be("assistant");
        apiResponse.Data.AudioContentType.Should().Be("audio/mpeg");
    }

    [Fact]
    public async Task SendSpeech_WithoutAudioField_ShouldReturn_400BadRequest()
    {
        // Arrange
        var token = await RegisterAndGetTokenAsync("no_audio_test@example.com");
        var topicId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Job Interview
        var sessionId = await StartSessionAsync(token, topicId);

        using var multipart = new MultipartFormDataContent();
        // Missing "audio" field
        multipart.Add(new StringContent("not an audio file"), "notes");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/speech")
        {
            Content = multipart
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendSpeech_FromDifferentUser_ShouldReturn_404NotFound()
    {
        // Arrange: User 1 starts session
        var tokenUser1 = await RegisterAndGetTokenAsync("user1_speech@example.com");
        var tokenUser2 = await RegisterAndGetTokenAsync("user2_speech@example.com");
        var topicId = Guid.Parse("33333333-3333-3333-3333-333333333333"); // Travel
        var sessionId = await StartSessionAsync(tokenUser1, topicId);

        // User 2 attempts speech upload to User 1's session
        using var multipart = new MultipartFormDataContent();
        var audioBytes = new byte[100];
        multipart.Add(new ByteArrayContent(audioBytes), "audio", "intrude.webm");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/speech")
        {
            Content = multipart
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenUser2);

        // Act
        var response = await _client.SendAsync(request);

        // Assert: Rejection with 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendSpeech_OnCompletedSession_ShouldReturn_400BadRequest()
    {
        // Arrange
        var token = await RegisterAndGetTokenAsync("completed_speech@example.com");
        var topicId = Guid.Parse("44444444-4444-4444-4444-444444444444"); // Shopping
        var sessionId = await StartSessionAsync(token, topicId);

        // End session
        var endRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/end");
        endRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(endRequest);

        // Try speech upload on ended session
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent(new byte[100]), "audio", "late.webm");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/conversations/{sessionId}/speech")
        {
            Content = multipart
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
