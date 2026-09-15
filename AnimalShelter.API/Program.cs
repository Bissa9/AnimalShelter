using AnimalShelter.Application.DTOs;
using AnimalShelter.Application.Interfaces;
using AnimalShelter.Application.Services;
using AnimalShelter.Application.Validators;
using AnimalShelter.Domain;
using AnimalShelter.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using AnimalShelter.Infrastructure.Data;
using AnimalShelter.Infrastructure.Repositories;
using AnimalShelter.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using AnimalShelter.Infrastructure.BackgroundServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Services
// --------------------------------------------------

builder.Services.AddProblemDetails();

builder.Services.AddValidation();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration["Redis:Connection"]
        ?? "localhost:6379";

    options.InstanceName = "AnimalShelter:";
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecurityRequirementTransformer>();
});

// --------------------------------------------------
// Authentication
// --------------------------------------------------

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        )
                    )
            };
    });

// --------------------------------------------------
// Authorization
// --------------------------------------------------

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("admin");
    });
});

// --------------------------------------------------
// Database
// --------------------------------------------------

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));


// --------------------------------------------------
// Dependency Injection
// --------------------------------------------------

builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IAnimalRepository, AnimalRepository>();

builder.Services.AddScoped<IShelterRepository, ShelterRepository>();

builder.Services.AddScoped<IAdoptionRepository, AdoptionRepository>();
builder.Services.AddScoped<IAdoptionService, AdoptionService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

builder.Services.AddScoped<
    IPasswordHasherService,
    PasswordHasherService>();

builder.Services.AddScoped<
    ITokenService,
    JwtTokenService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IRefreshTokenService,
    RefreshTokenService>();

builder.Services.AddScoped<
    IRefreshTokenRepository,
    RefreshTokenRepository>();

builder.Services.AddScoped<
    AnimalQueryValidator>();

builder.Services.AddScoped<
    IUnitOfWork,
    UnitOfWork>();

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

builder.Services.AddHostedService<BackgroundTaskWorker>();

// --------------------------------------------------
// Global Exception Handling
// --------------------------------------------------

builder.Services.AddExceptionHandler<
    AnimalShelter.API.GlobalExceptionHandler>();

builder.Services.AddHostedService<
    RefreshTokenCleanupService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    await DbSeeder.SeedAsync(db);
}

// --------------------------------------------------
// Middleware
// --------------------------------------------------

app.UseExceptionHandler();

app.UseAuthentication();
app.UseCors("Frontend");
app.UseAuthorization();

app.UseRateLimiter();

// We are using HTTP for now while learning.
// app.UseHttpsRedirection();

// --------------------------------------------------
// OpenAPI
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Animal Shelter API");
    });
}

// --------------------------------------------------
// Test endpoint
// --------------------------------------------------

app.MapGet("/hello", () =>
{
    return "Hello from my Animal Shelter!";
});

// --------------------------------------------------
// GET all animals
// Supports:
// pagination
// filtering
// searching
// sorting
// --------------------------------------------------

app.MapGet("/animals", async (
    [AsParameters] AnimalQueryDto query,
    IAnimalService service,
    AnimalQueryValidator validator) =>
{
    var errors = validator.Validate(query);

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var result =
        await service.GetPagedAsync(query);

    return Results.Ok(result);
})
.RequireAuthorization();

// --------------------------------------------------
// GET animal by id
// --------------------------------------------------

app.MapGet("/animals/{id:int}", async (
    [FromRoute] int id,
    IAnimalService service) =>
{
    var animal =
        await service.GetByIdAsync(id);

    return Results.Ok(animal);
})
.RequireAuthorization();

// --------------------------------------------------
// CREATE animal
// Admin only
// --------------------------------------------------

app.MapPost("/animals", async (
    CreateAnimalDto dto,
    IAnimalService service) =>
{
    var createdAnimal =
        await service.CreateAsync(dto);

    return Results.Created(
        $"/animals/{createdAnimal.Id}",
        createdAnimal);
})
.RequireAuthorization("AdminOnly");

// --------------------------------------------------
// ADOPT animal
// Authenticated users
// --------------------------------------------------

app.MapPost("/animals/{id:int}/adoptions", async (
    [FromRoute] int id,
    AdoptAnimalDto dto,
    IAdoptionService service) =>
{
    var adoption =
        await service.AdoptAsync(id, dto);

    return Results.Created(
        $"/animals/{id}/adoptions/{adoption.Id}",
        adoption);
})
.RequireAuthorization();

// --------------------------------------------------
// REGISTER
// Public
// --------------------------------------------------

app.MapPost("/register", async (
    RegisterDto dto,
    IAuthService authService) =>
{
    var user =
        await authService.RegisterAsync(dto);

    return Results.Created(
        $"/users/{user.Id}",
        user);
});

// --------------------------------------------------
// LOGIN
// Public
// --------------------------------------------------

app.MapPost("/login", async (
    LoginDto dto,
    IAuthService authService) =>
{
    var result =
        await authService.LoginAsync(dto);

    if (result == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(result);
})
.RequireRateLimiting("api");

// --------------------------------------------------
// REFRESH TOKEN
// Public
// --------------------------------------------------

app.MapPost("/refresh", async (
    RefreshTokenRequestDto dto,
    IAuthService authService) =>
{
    var result =
        await authService.RefreshAsync(dto);

    if (result == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(result);
});


app.MapPost("/test-background", async (
    IBackgroundTaskQueue queue) =>
{
    await queue.QueueAsync(async cancellationToken =>
    {
        await Task.Delay(1000, cancellationToken);

        Console.WriteLine(
            "🔥 Background job executed!");
    });

    return Results.Accepted();
});


// --------------------------------------------------
// UPDATE animal
// Admin only
// --------------------------------------------------

app.MapPut("/animals/{id:int}", async (
    [FromRoute] int id,
    UpdateAnimalDto dto,
    IAnimalService service) =>
{
    var updated =
        await service.UpdateAsync(id, dto);

    if (!updated)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
})
.RequireAuthorization("AdminOnly");

// --------------------------------------------------
// CURRENT USER
// --------------------------------------------------

app.MapGet("/me", (HttpContext context) =>
{
    var username =
        context.User.Identity?.Name;

    var role =
        context.User.FindFirst(
            System.Security.Claims.ClaimTypes.Role
        )?.Value;

    return Results.Ok(new
    {
        username,
        role
    });
})
.RequireAuthorization();

// --------------------------------------------------
// DELETE animal
// Admin only
// --------------------------------------------------

app.MapDelete("/animals/{id:int}", async (
    [FromRoute] int id,
    IAnimalService service) =>
{
    await service.DeleteAsync(id);

    return Results.NoContent();
})
.RequireAuthorization("AdminOnly");

// --------------------------------------------------
// Program
// --------------------------------------------------

app.Run();

internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes =
            await authenticationSchemeProvider.GetAllSchemesAsync();

        if (authenticationSchemes.Any(
            scheme => scheme.Name == "Bearer"))
        {
            document.Components ??=
                new OpenApiComponents();

            document.Components.SecuritySchemes =
                new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["Bearer"] =
                        new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            In = ParameterLocation.Header,
                            BearerFormat = "JWT"
                        }
                };
        }
    }
}

internal sealed class BearerSecurityRequirementTransformer
    : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata =
            context.Description.ActionDescriptor.EndpointMetadata;

        var requiresAuthorization =
            metadata.OfType<IAuthorizeData>().Any();

        if (!requiresAuthorization)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(
                    "Bearer",
                    context.Document)] = []
            });

        return Task.CompletedTask;
    }
}

public partial class Program
{
}