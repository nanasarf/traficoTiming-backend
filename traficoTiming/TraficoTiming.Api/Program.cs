using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TraficoTiming.Api.ExceptionHandling;
using TraficoTiming.Api.Validation;
using TraficoTiming.Database;
using TraficoTiming.Repository;
using TraficoTiming.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.Configure<RequestValidationOptions>(
    builder.Configuration.GetSection(RequestValidationOptions.SectionName));
builder.Services.AddValidatorsFromAssemblyContaining<CreateTimingSessionRequestValidator>();
builder.Services.AddScoped<RequestValidationFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<RequestValidationFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = ValidationResponse.Create;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TraficoTiming API",
        Version = "v1",
        Description = "Timing System API for managing race timing sessions, devices, and results",
        Contact = new OpenApiContact
        {
            Name = "TraficoTiming Team"
        }
    });
});

// Configure CORS for frontend applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5174", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<TraficoTimingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRepositories();
builder.Services.AddServices();

var app = builder.Build();

app.UseExceptionHandler();

// Enable Swagger in all environments for easier testing
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TraficoTiming API v1");
    options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root (http://localhost:port/)
    options.DocumentTitle = "TraficoTiming API";
});

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
