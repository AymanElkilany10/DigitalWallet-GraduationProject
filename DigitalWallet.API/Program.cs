using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using DigitalWallet.Infrastructure.Data;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Infrastructure.Repositories;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Services;
using DigitalWallet.Application.Helpers;
using DigitalWallet.API.Middleware;
using DigitalWallet.API.Filters;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════════
// SECTION 1: Service Configuration
// ═══════════════════════════════════════════════════════════════════════════

// ── 1.1 Database Context ────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("DigitalWallet.Infrastructure")
    ));

// ── 1.2 Repository Registration ────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<IBillPaymentRepository, BillPaymentRepository>();
builder.Services.AddScoped<IBillerRepository, BillerRepository>();
builder.Services.AddScoped<IFakeBankAccountRepository, FakeBankAccountRepository>();
builder.Services.AddScoped<IFakeBankTransactionRepository, FakeBankTransactionRepository>();
builder.Services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

// ── 1.3 Service Registration ───────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IBillPaymentService, BillPaymentService>();
builder.Services.AddScoped<IFakeBankService, FakeBankService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<JwtTokenGenerator>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var secretKey = configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey not found");

    var issuer = configuration["Jwt:Issuer"] ?? "DigitalWallet";
    var audience = configuration["Jwt:Audience"] ?? "DigitalWalletUsers";
    var expirationHours = int.Parse(configuration["Jwt:ExpirationHours"] ?? "24");

    return new JwtTokenGenerator(secretKey, issuer, audience, expirationHours);
});

// ── 1.4 Helper Services ─────────────────────────────────────────────────────
// Note: JwtTokenGenerator should be properly implemented in Application.Helpers
// For now, we skip registration as it's used directly in services

// ── 1.5 AutoMapper ──────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ── 1.6 Controllers with Custom Filters ─────────────────────────────────────
builder.Services.AddControllers(options =>
{
    // Add custom validation filter for uniform error responses
    options.Filters.Add<ValidationFilter>();
})
.AddJsonOptions(options =>
{
    // Configure JSON serialization to handle enums as strings
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// ── 1.7 CORS Configuration ──────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("X-Correlation-Id");
    });
});

// ── 1.8 Swagger/OpenAPI Documentation ───────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Digital Wallet API",
        Description = "RESTful API for Digital Wallet system with authentication, wallet management, transfers, and bill payments",
        Contact = new OpenApiContact
        {
            Name = "Digital Wallet Team",
            Email = "support@digitalwallet.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ── 1.9 HTTP Client for External Services (if needed) ──────────────────────
builder.Services.AddHttpClient();

// ── 1.10 Health Checks ──────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("Database");

// ═══════════════════════════════════════════════════════════════════════════
// SECTION 2: Application Pipeline Configuration
// ═══════════════════════════════════════════════════════════════════════════

var app = builder.Build();

// ── 2.1 Development Environment Configuration ───────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Wallet API V1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

// ── 2.2 Global Error Handler ────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ── 2.3 Request Logging ─────────────────────────────────────────────────────
app.UseMiddleware<RequestLoggingMiddleware>();

// ── 2.4 HTTPS Redirection ───────────────────────────────────────────────────
app.UseHttpsRedirection();

// ── 2.5 CORS ────────────────────────────────────────────────────────────────
app.UseCors("AllowSpecificOrigins");

// ── 2.6 JWT Authentication Middleware ───────────────────────────────────────
app.UseMiddleware<JwtAuthenticationMiddleware>();

// ── 2.7 Authorization ───────────────────────────────────────────────────────
app.UseAuthorization();

// ── 2.8 Controllers ─────────────────────────────────────────────────────────
app.MapControllers();

// ── 2.9 Health Check Endpoint ───────────────────────────────────────────────
app.MapHealthChecks("/health");

// ── 2.10 Welcome Endpoint ───────────────────────────────────────────────────
app.MapGet("/", () => Results.Ok(new
{
    Service = "Digital Wallet API",
    Version = "1.0.0",
    Status = "Running",
    Documentation = "/swagger",
    Health = "/health",
    Timestamp = DateTime.UtcNow
}));

// ═══════════════════════════════════════════════════════════════════════════
// TEMPORARILY DISABLED - Database is on Ayman device
// ═══════════════════════════════════════════════════════════════════════════
/*
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
    }
}
*/

// ═══════════════════════════════════════════════════════════════════════════
// SECTION 4: Application Startup
// ═══════════════════════════════════════════════════════════════════════════

app.Logger.LogInformation("Digital Wallet API starting...");
app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Listening on: {Urls}", string.Join(", ", app.Urls));

app.Run();

app.Logger.LogInformation("Digital Wallet API stopped");