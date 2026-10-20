using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EnglishLearning.IntegrationTests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturn_200OkWithHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseContent);
        
        var root = jsonDoc.RootElement;
        root.GetProperty("success").GetBoolean().Should().BeTrue();
        
        var data = root.GetProperty("data");
        data.GetProperty("status").GetString().Should().Be("healthy");
        data.GetProperty("version").GetString().Should().Be("1.0.0");
    }
}
