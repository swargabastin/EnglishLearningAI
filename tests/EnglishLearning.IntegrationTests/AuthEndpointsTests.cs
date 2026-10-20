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

public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
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

    [Fact]
    public async Task Register_WithValidData_ShouldReturn_200OkAndToken()
    {
        // Arrange
        var request = new RegisterRequest(
            "Samantha Ray",
            "samantha@example.com",
            "StrongPass123",
            "StrongPass123",
            "B2",
            "WorkplaceEnglish"
        );

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(responseBody, _jsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        apiResponse.Data.User.Email.Should().Be("samantha@example.com");
        apiResponse.Data.User.Name.Should().Be("Samantha Ray");
        apiResponse.Data.User.EnglishLevel.Should().Be("B2");

        // Verify password is not in response payload
        responseBody.Should().NotContain("PasswordHash");
        responseBody.Should().NotContain("StrongPass123");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn_409Conflict()
    {
        // Arrange
        var request = new RegisterRequest(
            "Samantha Ray",
            "duplicate@example.com",
            "StrongPass123",
            "StrongPass123",
            "B1",
            "SpeakingFluency"
        );

        var content1 = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var content2 = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act: Register first time
        var response1 = await _client.PostAsync("/api/v1/auth/register", content1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act: Register duplicate
        var response2 = await _client.PostAsync("/api/v1/auth/register", content2);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn_200OkAndToken()
    {
        // Arrange: Register user first
        var registerRequest = new RegisterRequest(
            "Michael Scott",
            "michael@dundermifflin.com",
            "ThatsWhatSheSaid1",
            "ThatsWhatSheSaid1",
            "B1",
            "WorkplaceEnglish"
        );

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        await _client.PostAsync("/api/v1/auth/register", registerContent);

        // Act: Login
        var loginRequest = new LoginRequest("michael@dundermifflin.com", "ThatsWhatSheSaid1");
        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", loginContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(responseBody, _jsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        apiResponse.Data.User.Email.Should().Be("michael@dundermifflin.com");
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ShouldReturn_401Unauthorized()
    {
        // Arrange: Register user first
        var registerRequest = new RegisterRequest(
            "Jim Halpert",
            "jim@dundermifflin.com",
            "PrankDwight123",
            "PrankDwight123",
            "B2",
            "WorkplaceEnglish"
        );

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        await _client.PostAsync("/api/v1/auth/register", registerContent);

        // Act: Login with wrong password
        var loginRequest = new LoginRequest("jim@dundermifflin.com", "WrongPassword999");
        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", loginContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ShouldReturn_401Unauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ShouldReturn_200OkUserProfile()
    {
        // Arrange: Register user and get token
        var registerRequest = new RegisterRequest(
            "Pam Beesly",
            "pam@dundermifflin.com",
            "ArtGallery2026!",
            "ArtGallery2026!",
            "B2",
            "SpeakingFluency"
        );

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        var registerResponse = await _client.PostAsync("/api/v1/auth/register", registerContent);
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        var registerData = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(registerBody, _jsonOptions);
        var token = registerData!.Data!.AccessToken;

        // Act: Request /api/v1/auth/me with Bearer token
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserProfileDto>>(responseBody, _jsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.Email.Should().Be("pam@dundermifflin.com");
        apiResponse.Data.Name.Should().Be("Pam Beesly");
        apiResponse.Data.EnglishLevel.Should().Be("B2");
    }
}
