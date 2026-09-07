using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NotificationService.Api.Filters;
using NotificationService.Core.FluentValidation;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Workers;
using DotNetEnv;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Services.Smtp;
using NotificationService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
// .env lives in repo root (next to docker-compose.yaml), but the process CWD
// is NotificationService.Api/ when launched from an IDE. Resolve explicitly
// so loading doesn't depend on where you pressed "run". In Docker no file
// exists (compose injects real env vars), so fall back gracefully.
var rootEnvPath = Path.Combine(builder.Environment.ContentRootPath, "..", ".env");
if (File.Exists(rootEnvPath))
    Env.Load(rootEnvPath);
else
    Env.Load();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationExceptionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "Firebase and Smpt push notification service with outbox pattern"
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// HttpClient for FcmSender
builder.Services.AddHttpClient<FcmSender>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<RegisterTokenRequestValidator>();

// Register Infra Services
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment()|| app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
      {
          options.SwaggerEndpoint(app.Environment.IsStaging()? "/api/notification/swagger/v1/swagger.json" : "/swagger/v1/swagger.json", "Notification Service v1");
          options.RoutePrefix = string.Empty; // swagger loads at root https://localhost:PORT/
      });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();