using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Moq;
using TraficoTiming.Api.ExceptionHandling;
using TraficoTiming.Api.Validation;
using Xunit;

namespace TraficoTiming.Tests;

public sealed class GlobalExceptionHandlerTests
{
    [Theory]
    [MemberData(nameof(MappedExceptions))]
    public async Task Mapped_exception_returns_expected_problem_details(
        Exception exception,
        int expectedStatus,
        string expectedTitle,
        string expectedType)
    {
        var (context, handled) = await HandleAsync(exception);

        Assert.True(handled);
        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        var problem = await ReadResponseAsync(context);
        Assert.Equal(expectedStatus, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(expectedTitle, problem.RootElement.GetProperty("title").GetString());
        Assert.EndsWith(expectedType, problem.RootElement.GetProperty("type").GetString());
        Assert.Equal("/tests/exceptions", problem.RootElement.GetProperty("instance").GetString());
        Assert.Equal("test-trace-id", problem.RootElement.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task Unexpected_exception_returns_sanitized_production_problem_details()
    {
        const string secret = "Host=db;Password=super-secret";
        var exception = new Exception($"{secret}\n   at Internal.Service.DoWork()");

        var (context, handled) = await HandleAsync(exception);
        var body = await ReadResponseTextAsync(context);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains("\"detail\":\"An unexpected error occurred.\"", body);
        Assert.DoesNotContain(secret, body);
        Assert.DoesNotContain("Internal.Service", body);
        Assert.DoesNotContain("stackTrace", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Fluent_validation_exception_includes_field_level_errors()
    {
        var exception = new ValidationException(
            [new ValidationFailure("Lane", "Lane must be greater than zero.")]);

        var (context, handled) = await HandleAsync(exception);
        var problem = await ReadResponseAsync(context);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var errors = problem.RootElement.GetProperty("errors");
        Assert.Equal(
            "Lane must be greater than zero.",
            errors.GetProperty("Lane")[0].GetString());
    }

    [Fact]
    public void Request_validation_response_preserves_field_errors_and_trace_id()
    {
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "validation-trace-id"
        };
        httpContext.Request.Path = "/tests/validation";
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());
        actionContext.ModelState.AddModelError("Lane", "Lane is required.");

        var result = Assert.IsType<BadRequestObjectResult>(
            ValidationResponse.Create(actionContext));
        var details = Assert.IsType<ValidationProblemDetails>(result.Value);

        Assert.Equal(["Lane is required."], details.Errors["Lane"]);
        Assert.Equal("/tests/validation", details.Instance);
        Assert.Equal(
            "validation-trace-id",
            details.Extensions["traceId"]);
    }

    [Fact]
    public async Task Request_cancellation_is_not_converted_to_problem_details()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var (context, handled) = await HandleAsync(
            new OperationCanceledException(cancellation.Token),
            cancellation.Token);

        Assert.False(handled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    public static TheoryData<Exception, int, string, string> MappedExceptions => new()
    {
        {
            new ArgumentException("The argument is invalid."),
            StatusCodes.Status400BadRequest,
            "Invalid argument",
            "invalid-argument"
        },
        {
            new KeyNotFoundException("The resource was not found."),
            StatusCodes.Status404NotFound,
            "Resource not found",
            "not-found"
        },
        {
            new InvalidOperationException("The operation conflicts with current state."),
            StatusCodes.Status409Conflict,
            "Operation conflict",
            "invalid-operation"
        },
        {
            new DbUpdateConcurrencyException("SQL/internal concurrency detail"),
            StatusCodes.Status409Conflict,
            "Concurrency conflict",
            "concurrency-conflict"
        }
    };

    private static async Task<(DefaultHttpContext Context, bool Handled)> HandleAsync(
        Exception exception,
        CancellationToken requestAborted = default)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(value => value.EnvironmentName)
            .Returns(Environments.Production);
        services.AddSingleton(environment.Object);
        await using var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = provider,
            TraceIdentifier = "test-trace-id",
            RequestAborted = requestAborted
        };
        context.Request.Path = "/tests/exceptions";
        context.Request.Headers.Accept = "application/problem+json";
        context.Response.Body = new MemoryStream();

        var handler = provider
            .GetServices<IExceptionHandler>()
            .OfType<GlobalExceptionHandler>()
            .Single();
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        return (context, handled);
    }

    private static async Task<JsonDocument> ReadResponseAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }

    private static async Task<string> ReadResponseTextAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}
