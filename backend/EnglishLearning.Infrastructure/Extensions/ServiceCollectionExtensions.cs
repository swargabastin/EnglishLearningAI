using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Infrastructure.Ai;
using EnglishLearning.Infrastructure.Auth;
using EnglishLearning.Infrastructure.Persistence;
using EnglishLearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishLearning.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                options.UseNpgsql(connectionString);
            }
        });

        // Register Unit of Work and Repositories
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITopicRepository, TopicRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConversationSessionRepository, ConversationSessionRepository>();
        services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        services.AddScoped<IVocabularyRepository, VocabularyRepository>();
        services.AddScoped<IAdaptiveLearningRepository, AdaptiveLearningRepository>();

        // Register Auth Infrastructure
        services.AddHttpContextAccessor();
        services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register AI, STT, TTS, and Assessment Services with typed HttpClients
        services.AddHttpClient<IAiConversationService, OpenAiConversationService>();
        services.AddHttpClient<ISpeechToTextService, WhisperSpeechToTextService>();
        services.AddHttpClient<ITextToSpeechService, OpenAiTextToSpeechService>();
        services.AddHttpClient<IAiAssessmentService, OpenAiAssessmentService>();

        return services;
    }
}
