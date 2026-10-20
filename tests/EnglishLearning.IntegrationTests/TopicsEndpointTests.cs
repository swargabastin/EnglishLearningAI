using System.Net;
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

public class TopicsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TopicsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace PostgreSQL DbContext with InMemory DbContext for isolated integration testing
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

                // Seed in-memory database
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllTopics_ShouldReturn_200OkWithSeedTopics()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/topics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<TopicDto>>>(content, jsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Should().HaveCount(7);
        apiResponse.Data!.Select(t => t.Name).Should().Contain(new[]
        {
            "Daily Conversation",
            "Job Interview",
            "Travel",
            "Shopping",
            "Restaurant",
            "Workplace",
            "Introducing Yourself"
        });
    }

    [Fact]
    public async Task GetTopicById_WithValidId_ShouldReturn_200Ok()
    {
        // Arrange: Daily Conversation static seed GUID
        var dailyConversationId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var response = await _client.GetAsync($"/api/v1/topics/{dailyConversationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<TopicDto>>(content, jsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.Id.Should().Be(dailyConversationId);
        apiResponse.Data.Name.Should().Be("Daily Conversation");
        apiResponse.Data.Difficulty.Should().Be("A1");
    }

    [Fact]
    public async Task GetTopicById_WithInvalidId_ShouldReturn_404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/topics/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(404);
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
    }
}
