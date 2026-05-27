using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TraficoTiming.Database;
using TraficoTiming.Repository;
using TraficoTiming.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

builder.Services.AddDbContext<TraficoTimingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRepositories();
builder.Services.AddServices();

var app = builder.Build();

// Enable Swagger in all environments for easier testing
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TraficoTiming API v1");
    options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root (http://localhost:port/)
    options.DocumentTitle = "TraficoTiming API";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
