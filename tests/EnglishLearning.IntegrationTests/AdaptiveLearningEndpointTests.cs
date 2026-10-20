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

public class AdaptiveLearningEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AdaptiveLearningEndpointTests(WebApplicationFactory<Program> factory)
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

    private async Task<(string token, Guid userId)> RegisterAndLoginAsync(string email = "adaptive_user@example.com")
    {
        var regPayload = new RegisterRequest(
            "Adaptive Tester",
            email,
            "SecurePassword123!",
            "SecurePassword123!",
            "B1",
            "SpeakingFluency"
        );

        var regRes = await _client.PostAsJsonAsync("/api/v1/auth/register", regPayload);
        regRes.EnsureSuccessStatusCode();

        var loginPayload = new LoginRequest(email, "SecurePassword123!");
        var loginRes = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);
        loginRes.EnsureSuccessStatusCode();

        var loginBody = await loginRes.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginBody, _jsonOptions);
        return (apiResponse!.Data!.AccessToken, apiResponse.Data.User.Id);
    }

    [Fact]
    public async Task GetAssessmentQuestions_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/adaptivelearning/assessment/questions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAssessmentQuestions_WithAuth_ShouldReturnQuestionsList()
    {
        // Arrange
        var (token, _) = await RegisterAndLoginAsync("adaptive_q@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/adaptivelearning/assessment/questions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiRes = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<AssessmentQuestionDto>>>(_jsonOptions);
        apiRes.Should().NotBeNull();
        var questions = apiRes!.Data;
        questions.Should().NotBeNullOrEmpty();
        questions.Should().Contain(q => q.SkillCategory == "Grammar");
        questions.Should().Contain(q => q.SkillCategory == "Vocabulary");
    }

    [Fact]
    public async Task SubmitAssessment_And_GetLatest_ShouldPersistAndReturnResult()
    {
        // Arrange
        var (token, _) = await RegisterAndLoginAsync("adaptive_sub@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var qRes = await _client.GetAsync("/api/v1/adaptivelearning/assessment/questions");
        var apiQuestions = await qRes.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<AssessmentQuestionDto>>>(_jsonOptions);
        var questions = apiQuestions!.Data!;

        // Submit only non-speaking questions to evaluate cleanly
        var nonSpeaking = questions.Where(q => q.SkillCategory != "Speaking").ToList();
        var answers = nonSpeaking.Select(q => new QuestionAnswerDto(
            q.Id,
            q.CorrectOptionIndex,
            null,
            10
        )).ToList();

        var submitReq = new SubmitAssessmentRequest(answers, null, null);

        // Act 1: Submit Assessment
        var submitRes = await _client.PostAsJsonAsync("/api/v1/adaptivelearning/assessment/submit", submitReq);
        submitRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiSubmit = await submitRes.Content.ReadFromJsonAsync<ApiResponse<LearnerAssessmentResultDto>>(_jsonOptions);
        var submitResult = apiSubmit!.Data;

        submitResult.Should().NotBeNull();
        submitResult!.Category.Should().Be("Advanced");
        submitResult.OverallScore.Should().Be(100m);

        // Act 2: Fetch Latest Assessment
        var latestRes = await _client.GetAsync("/api/v1/adaptivelearning/assessment/latest");
        latestRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiLatest = await latestRes.Content.ReadFromJsonAsync<ApiResponse<LearnerAssessmentResultDto>>(_jsonOptions);
        var latestResult = apiLatest!.Data;

        latestResult.Should().NotBeNull();
        latestResult!.Id.Should().Be(submitResult.Id);
        latestResult.Category.Should().Be("Advanced");
    }

    [Fact]
    public async Task GetTodayPlan_ShouldReturnFourPhases_AndBeIdempotentOnSubsequentCalls()
    {
        // Arrange
        var (token, _) = await RegisterAndLoginAsync("adaptive_plan@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act 1: First retrieval generates plan
        var planRes1 = await _client.GetAsync("/api/v1/adaptivelearning/plan/today");
        planRes1.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiPlan1 = await planRes1.Content.ReadFromJsonAsync<ApiResponse<DailyStudyPlanDto>>(_jsonOptions);
        var plan1 = apiPlan1!.Data;

        plan1.Should().NotBeNull();
        plan1!.Phases.Should().HaveCount(4);
        plan1.Phases[0].PhaseType.Should().Be("GrammarStudy");
        plan1.Phases[1].PhaseType.Should().Be("SentenceMaking");
        plan1.Phases[2].PhaseType.Should().Be("Practice");
        plan1.Phases[3].PhaseType.Should().Be("AiSpeaking");

        // Act 2: Second retrieval on same date should return the exact same plan idempotently
        var planRes2 = await _client.GetAsync("/api/v1/adaptivelearning/plan/today");
        planRes2.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiPlan2 = await planRes2.Content.ReadFromJsonAsync<ApiResponse<DailyStudyPlanDto>>(_jsonOptions);
        var plan2 = apiPlan2!.Data;

        plan2.Should().NotBeNull();
        plan2!.Id.Should().Be(plan1.Id);
        plan2.ObjectiveCode.Should().Be(plan1.ObjectiveCode);
        plan2.IsIdempotentCached.Should().BeTrue();
    }

    [Fact]
    public async Task CompletePhase_ShouldUnlockNextPhase_AndRecordInHistory()
    {
        // Arrange
        var (token, _) = await RegisterAndLoginAsync("adaptive_flow@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get plan
        var planRes = await _client.GetAsync("/api/v1/adaptivelearning/plan/today");
        var apiPlan = await planRes.Content.ReadFromJsonAsync<ApiResponse<DailyStudyPlanDto>>(_jsonOptions);
        var plan = apiPlan!.Data;

        var phase1 = plan!.Phases[0];
        phase1.IsLocked.Should().BeFalse();
        plan.Phases[1].IsLocked.Should().BeTrue();

        // Build correct task answers
        var answers = new Dictionary<string, string>();
        foreach (var task in phase1.Tasks)
        {
            if (task.CorrectAnswer != null)
            {
                answers[task.Id] = task.CorrectAnswer;
            }
        }

        var completeReq = new CompletePhaseRequest(answers, 120);

        // Act: Complete Phase 1
        var completeRes = await _client.PostAsJsonAsync($"/api/v1/adaptivelearning/plan/phase/{phase1.Id}/complete", completeReq);
        completeRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiComplete = await completeRes.Content.ReadFromJsonAsync<ApiResponse<PhaseCompletionResultDto>>(_jsonOptions);
        var completeResult = apiComplete!.Data;

        completeResult.Should().NotBeNull();
        completeResult!.NextPhaseUnlocked.Should().BeTrue();

        // Re-fetch today's plan to confirm Phase 2 is now unlocked
        var updatedPlanRes = await _client.GetAsync("/api/v1/adaptivelearning/plan/today");
        var apiUpdated = await updatedPlanRes.Content.ReadFromJsonAsync<ApiResponse<DailyStudyPlanDto>>(_jsonOptions);
        var updatedPlan = apiUpdated!.Data;

        updatedPlan!.Phases[0].Status.Should().Be("Completed");
        updatedPlan.Phases[1].IsLocked.Should().BeFalse();

        // Check History endpoint
        var historyRes = await _client.GetAsync("/api/v1/adaptivelearning/plan/history");
        historyRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiHistory = await historyRes.Content.ReadFromJsonAsync<ApiResponse<LearningPlanHistoryDto>>(_jsonOptions);
        var history = apiHistory!.Data;

        history.Should().NotBeNull();
        history!.RecentPlans.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CompletePhase_BelongingToAnotherUser_ShouldReturn403Forbidden()
    {
        // Arrange: User 1 creates plan
        var (token1, _) = await RegisterAndLoginAsync("user1_plan@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var planRes1 = await _client.GetAsync("/api/v1/adaptivelearning/plan/today");
        var apiPlan1 = await planRes1.Content.ReadFromJsonAsync<ApiResponse<DailyStudyPlanDto>>(_jsonOptions);
        var phase1Id = apiPlan1!.Data!.Phases[0].Id;

        // User 2 logs in and attempts to complete User 1's phase
        var (token2, _) = await RegisterAndLoginAsync("user2_plan@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var completeReq = new CompletePhaseRequest(new Dictionary<string, string>(), 60);

        // Act
        var res = await _client.PostAsJsonAsync($"/api/v1/adaptivelearning/plan/phase/{phase1Id}/complete", completeReq);

        // Assert: Unauthorized access to another user's plan is rejected
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
