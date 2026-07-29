using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TraficoTiming.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    private const string ProblemTypeBase = "https://traficotiming.dev/problems";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException
            && httpContext.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        var mapping = MapException(exception);
        LogException(httpContext, exception, mapping.LogLevel);

        var problemDetails = new ProblemDetails
        {
            Type = $"{ProblemTypeBase}/{mapping.Type}",
            Title = mapping.Title,
            Status = mapping.Status,
            Detail = mapping.Detail,
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = mapping.Status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static ExceptionMapping MapException(Exception exception) => exception switch
    {
        ValidationException => new(
            StatusCodes.Status400BadRequest,
            "Validation failed",
            "validation-error",
            "One or more validation errors occurred.",
            LogLevel.Information),
        ArgumentException argumentException => new(
            StatusCodes.Status400BadRequest,
            "Invalid argument",
            "invalid-argument",
            argumentException.Message,
            LogLevel.Information),
        KeyNotFoundException notFoundException => new(
            StatusCodes.Status404NotFound,
            "Resource not found",
            "not-found",
            notFoundException.Message,
            LogLevel.Information),
        InvalidOperationException invalidOperationException => new(
            StatusCodes.Status409Conflict,
            "Operation conflict",
            "invalid-operation",
            invalidOperationException.Message,
            LogLevel.Information),
        DbUpdateConcurrencyException => new(
            StatusCodes.Status409Conflict,
            "Concurrency conflict",
            "concurrency-conflict",
            "The resource was changed by another operation. Refresh it and try again.",
            LogLevel.Warning),
        _ => new(
            StatusCodes.Status500InternalServerError,
            "Internal server error",
            "internal-server-error",
            "An unexpected error occurred.",
            LogLevel.Error)
    };

    private void LogException(HttpContext context, Exception exception, LogLevel level)
    {
        if (environment.IsDevelopment())
        {
            logger.Log(
                level,
                exception,
                "Request failed with trace ID {TraceId} on path {RequestPath}",
                context.TraceIdentifier,
                context.Request.Path);
        }
        else
        {
            logger.Log(
                level,
                "Request failed with trace ID {TraceId} on path {RequestPath}",
                context.TraceIdentifier,
                context.Request.Path);
        }
    }

    private sealed record ExceptionMapping(
        int Status,
        string Title,
        string Type,
        string Detail,
        LogLevel LogLevel);
}
