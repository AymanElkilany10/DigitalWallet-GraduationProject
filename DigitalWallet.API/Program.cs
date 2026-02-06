using DigitalWallet.API.Extensions;
using DigitalWallet.API.Filters;

namespace DigitalWallet.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── 1.  Configuration & Logging ─────────────────────────────────
            // appsettings.json and appsettings.{Environment}.json are loaded
            // automatically by CreateBuilder.  Console logging is the default
            // sink; swap for Serilog / NLog as needed.
            builder.Logging.AddConsole();

            // ── 2.  CORS ────────────────────────────────────────────────────
            // Read allowed origins from config so the front-end can call the API
            // without CORS errors during local development.
            builder.Services.AddCors(options =>
            {
                var origins = builder.Configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>()
                    ?? Array.Empty<string>();

                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // ── 3.  Controllers + Filters ───────────────────────────────────
            // ValidationFilter is registered globally so every controller action
            // automatically returns a uniform 400 when ModelState is invalid.
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            });

            // ── 4.  Swagger / OpenAPI ───────────────────────────────────────
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerDocumentation();

            // ── 5.  Application & Infrastructure Services ──────────────────
            // Wires up every scoped service, AutoMapper, FluentValidation
            // validators, and the "AdminOnly" authorization policy.
            builder.Services.AddApplicationServices();

            // ── 6.  Build the WebApplication ────────────────────────────────
            var app = builder.Build();

            // ── 7.  Middleware pipeline (order is critical) ─────────────────
            //
            //   ┌─ ExceptionHandlingMiddleware   ← outermost: catches everything
            //   │  ┌─ RequestLoggingMiddleware   ← logs request start/end + timing
            //   │  │  ┌─ JwtMiddleware           ← populates HttpContext.User
            //   │  │  │  ┌─ CORS                 ← adds Access-Control-* headers
            //   │  │  │  │  ┌─ HTTPS Redirect    ← 301 http → https
            //   │  │  │  │  │  ┌─ Authorization  ← evaluates [Authorize] policies
            //   │  │  │  │  │  │  ┌─ Controllers ← action methods
            //
            app.UseDigitalWalletMiddleware();  // Exception → Logging → Jwt

            app.UseCors();
            app.UseHttpsRedirection();

            // Authorization must come AFTER JwtMiddleware has set HttpContext.User
            app.UseAuthorization();

            // ── 8.  Swagger UI (Development only) ──────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerDocumentation();
            }

            // ── 9.  Route controllers ───────────────────────────────────────
            app.MapControllers();

            // ── 10. Health-check endpoint ───────────────────────────────────
            // Quick smoke-test: GET /health → 200 OK
            app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

            app.Run();
        }
    }
}
