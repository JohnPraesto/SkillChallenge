using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkillChallenge.Data;
using SkillChallenge.Interfaces;
using SkillChallenge.Repositories;
using SkillChallenge.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                Environment.GetEnvironmentVariable("SqlConnectionString")));
        services.AddScoped<IArchivedChallengeRepository, ArchivedChallengeRepository>();
        services.AddScoped<IRatingEntityRepository, RatingEntityRepository>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<EloRatingService>();
    })
    .Build();

host.Run();
