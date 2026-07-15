using System.Text.Json;
using LeadPilot.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace LeadPilot.Server.Extensions;

public sealed class ProblemDetailsMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly Action<ILogger, string, Exception?> LogUnhandledException = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(1, nameof(ProblemDetailsMiddleware)),
        "Unhandled exception while processing request {Path}");
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (ProblemDetailsException exception)
        {
            await WriteProblemAsync(context, exception.StatusCode, exception.Title, exception.Detail).ConfigureAwait(false);
        }
        catch (BadHttpRequestException exception)
        {
            await WriteProblemAsync(context, exception.StatusCode, "Bad request", exception.Message).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogUnhandledException(_logger, context.Request.Path, exception);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred.").ConfigureAwait(false);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        ProblemDetails problemDetails = new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, problemDetails, SerializerOptions).ConfigureAwait(false);
    }
}