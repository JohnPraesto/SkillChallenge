using ASPNET_VisualStudio_Tutorial.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SkillChallenge.Data;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;
using SkillChallenge.Repositories;
using SkillChallenge.Services;
using System.Threading.RateLimiting;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // In use when anonymous voting
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (ctx, _) =>
            {
                return ValueTask.CompletedTask;
            };

            options.AddPolicy("votes", httpContext =>
            {
                // Prefer header token (works on Safari)
                if (httpContext.Request.Headers.TryGetValue("X-Voter", out var hdr) && !string.IsNullOrWhiteSpace(hdr))
                {
                    return RateLimitPartition.GetTokenBucketLimiter(
                        hdr.ToString(),
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 10,
                            TokensPerPeriod = 10,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(60),
                            AutoReplenishment = true,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                }

                // Fallback to cookie or IP
                var clientId = httpContext.Request.Cookies.TryGetValue("voter_id", out var c) ? c : null;
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
                var key = clientId ?? $"ip:{ip}";

                return RateLimitPartition.GetTokenBucketLimiter(
                    key,
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 10,
                        TokensPerPeriod = 10,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(60),
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
        });
        // Stop

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowFrontend",
                policy =>
                    policy.WithOrigins("http://localhost:5173", "https://skillchallenge.net")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
            );
        });

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();

        builder.Services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
            option.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer",
                }
            );
            option.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        new string[] { }
                    },
                }
            );
        });

        var useSqlite = builder.Environment.IsEnvironment("CI");

        if (useSqlite)
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
            );
        }
        else
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions => sqlOptions.EnableRetryOnFailure())
            );
        }

        builder
            .Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        var signingKey = builder.Configuration["JWT:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
            throw new InvalidOperationException("JWT:SigningKey is not configured.");

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    options.DefaultChallengeScheme =
                    options.DefaultForbidScheme =
                    options.DefaultScheme =
                    options.DefaultSignInScheme =
                    options.DefaultSignOutScheme =
                        JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(signingKey)
                    ),
                    ValidateLifetime = true,
                    // ClockSkew = TimeSpan.Zero, // endast för local testing
                };
            });

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
        builder.Services.AddScoped<IArchivedChallengeRepository, ArchivedChallengeRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
        builder.Services.AddScoped<IMediaService, MediaService>();
        builder.Services.AddScoped<IRatingEntityRepository, RatingEntityRepository>();
        builder.Services.AddScoped<EloRatingService>();
        builder.Services.AddScoped<IEmailService, EmailService>();

        var storageType = builder.Configuration["Storage:Type"];
        if (storageType == "AzureBlob")
        {
            builder.Services.AddScoped<IMediaService, AzureBlobMediaService>();
        }
        else
        {
            builder.Services.AddScoped<IMediaService, MediaService>();
        }

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseDefaultFiles(); // ny
        app.UseStaticFiles();
        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        app.MapControllers();

        app.MapFallbackToFile("index.html");

        // Fallback to serve React app for non-API routes
        app.MapFallback(context =>
        {
            // Only serve index.html if the path does not start with /api
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.ContentType = "text/html";
                return context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath!, "index.html"));
            }

            context.Response.StatusCode = 404;
            return Task.CompletedTask;
        });

        await app.RunAsync();
    }
}
