using System.Text;
using System.Threading.RateLimiting;
using EnglishLearning.Api.Hubs;
using EnglishLearning.Api.Middlewares;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Application.Services;
using EnglishLearning.Application.Validators;
using EnglishLearning.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add controllers and API explorer
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Bearer support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "English Learning AI API",
        Version = "v1",
        Description = "Backend API for AI-Powered English Learning Application with JWT Authentication"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Infrastructure layer (EF Core, DB Context, Repositories, Auth Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure Application services & validators
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<ISpeechConversationService, SpeechConversationService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IVocabularyService, VocabularyService>();
builder.Services.AddScoped<IAdaptiveLearningService, AdaptiveLearningService>();
builder.Services.AddScoped<IPronunciationAssessmentService, PronunciationAssessmentService>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// Configure JWT Authentication
const string DevelopmentJwtSecret = "EnglishLearningAI_DevelopmentSecretKey_MustBeAtLeast32BytesLong_2026!";
var jwtSecret = builder.Configuration["Jwt:Secret"];

if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Testing")
{
    if (string.IsNullOrWhiteSpace(jwtSecret))
    {
        jwtSecret = DevelopmentJwtSecret;
    }
}
else
{
    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret == DevelopmentJwtSecret)
    {
        throw new InvalidOperationException("Fatal configuration error: A unique, secure 'Jwt:Secret' must be provided in non-development environments. The known development secret is forbidden in production.");
    }

    if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    {
        throw new InvalidOperationException("Fatal configuration error: 'Jwt:Secret' must be at least 256 bits (32 bytes) long.");
    }
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EnglishLearningAI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EnglishLearningClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Configure Rate Limiting: Dedicated Auth protection (10 req/min) + General Global limiter (60 req/min)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthRateLimit", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 60,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "English Learning API v1");
    });
}

app.UseCors("AllowFrontend");

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ConversationHub>("/hubs/conversation");

app.Run();

// Required for WebApplicationFactory in Integration Tests
public partial class Program { }
