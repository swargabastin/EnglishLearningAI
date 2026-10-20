using System.Net;
using System.Text.Json;
using EnglishLearning.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught by global handler: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path
        };

        switch (exception)
        {
            case EntityNotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = notFoundEx.Message;
                problemDetails.Type = "https://errors.englishlearning.ai/not-found";
                break;

            case UnauthorizedSessionAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = "The requested conversation session was not found or access is not permitted.";
                problemDetails.Type = "https://errors.englishlearning.ai/not-found";
                break;

            case InvalidSessionStateException sessionStateEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Invalid Session State";
                problemDetails.Detail = sessionStateEx.Message;
                problemDetails.Type = "https://errors.englishlearning.ai/invalid-session-state";
                break;

            case InvalidAudioException audioEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Invalid Audio Input";
                problemDetails.Detail = audioEx.Message;
                problemDetails.Type = "https://errors.englishlearning.ai/invalid-audio";
                break;

            case InvalidCredentialsException credentialsEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = credentialsEx.Message;
                problemDetails.Type = "https://errors.englishlearning.ai/unauthorized";
                break;

            case UnauthorizedAccessException unauthEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                problemDetails.Status = (int)HttpStatusCode.Forbidden;
                problemDetails.Title = "Forbidden";
                problemDetails.Detail = unauthEx.Message;
                problemDetails.Type = "https://errors.englishlearning.ai/forbidden";
                break;

            case DuplicateEmailException duplicateEmailEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                problemDetails.Status = (int)HttpStatusCode.Conflict;
                problemDetails.Title = "Conflict";
                problemDetails.Detail = duplicateEmailEx.Message;
                problemDetails.Type = "https://errors.englishlearning.ai/conflict";
                break;

            case AiServiceException aiEx:
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                problemDetails.Status = (int)HttpStatusCode.ServiceUnavailable;
                problemDetails.Title = "AI Service Unavailable";
                problemDetails.Detail = aiEx.Message;
                problemDetails.Type = "https://errors.englishlearning.ai/ai-service-unavailable";
                break;

            case FluentValidation.ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Validation Failed";
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Type = "https://errors.englishlearning.ai/validation-error";
                problemDetails.Extensions["errors"] = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An unexpected error occurred while processing your request. Please try again later.";
                problemDetails.Type = "https://errors.englishlearning.ai/internal-error";
                break;
        }

        var json = JsonSerializer.Serialize(problemDetails);
        return context.Response.WriteAsync(json);
    }
}
