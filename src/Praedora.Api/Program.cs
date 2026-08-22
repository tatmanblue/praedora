using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Praedora.Api.Endpoints;
using Praedora.Api.Workers;
using Praedora.Application.Services;
using Praedora.Core.Interfaces;
using Praedora.Infrastructure.Data;
using Praedora.Infrastructure.Data.Repositories;
using Praedora.Infrastructure.Email;
using Praedora.Infrastructure.Llm;
using Praedora.Infrastructure.Logging;
using Praedora.Infrastructure.Notifications;
using Serilog;

Env.TraversePath().Load();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/praedora-.log", rollingInterval: RollingInterval.Day));

builder.AddServiceDefaults();

string dbProvider = builder.Configuration["PRAEDORA_DB_PROVIDER"] ?? "Sqlite";
string dbConnection = builder.Configuration["PRAEDORA_DB_CONNECTION"]
    ?? throw new InvalidOperationException("PRAEDORA_DB_CONNECTION is not set. Copy .env.example to .env and fill it in.");

builder.Services.AddDbContext<PraedoraDbContext>(options =>
{
    switch (dbProvider)
    {
        case "Sqlite":
            // MaxBatchSize(1): the Sqlite provider's batching can misreport affected-row counts
            // when an INSERT (e.g. a new StatusEvent) and an UPDATE (its parent Application) are
            // batched together, raising a spurious DbUpdateConcurrencyException.
            options.UseSqlite(dbConnection, sqlite => sqlite.MaxBatchSize(1));
            break;
        default:
            throw new NotSupportedException(
                $"Database provider '{dbProvider}' is not supported yet. Only Sqlite is implemented (build sequence step 6 adds Postgres/SQL Server).");
    }
});

builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<CaptureService>();
builder.Services.AddScoped<EmailSyncService>();
builder.Services.AddScoped<ClassificationService>();
builder.Services.AddScoped<MatchingService>();
builder.Services.AddScoped<ReviewQueueService>();

builder.Services.AddScoped<IEmailProvider, GmailEmailProvider>();
builder.Services.AddScoped<ILlmClassifier, ClaudeClassifier>();
builder.Services.AddScoped<IJobDescriptionExtractor, ClaudeJobDescriptionExtractor>();
builder.Services.AddScoped<IApplicationMatcher, MatchingService>();
builder.Services.AddScoped<INotifier, SignalRNotifier>();
builder.Services.AddScoped<ILogSink, DatabaseLogSink>();

builder.Services.AddHostedService<EmailSyncWorker>();

// The Chrome extension calls /api/capture directly from a chrome-extension:// origin. This is
// the only browser-originated cross-origin call in the system (Blazor Server talks to the API
// server-to-server), and AllowAnyOrigin matches the existing no-auth, local-trust posture
// (decision #6) rather than tracking per-install extension IDs.
builder.Services.AddCors(options => options.AddPolicy(CaptureEndpoints.CorsPolicyName, policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    PraedoraDbContext dbContext = scope.ServiceProvider.GetRequiredService<PraedoraDbContext>();
    dbContext.Database.Migrate();
}

app.MapDefaultEndpoints();

app.UseCors();

app.MapApplicationEndpoints();
app.MapCaptureEndpoints();
app.MapReviewQueueEndpoints();
app.MapLogEndpoints();

app.Run();

// Exposes the top-level Program for Praedora.IntegrationTests' WebApplicationFactory<Program>.
public partial class Program;
