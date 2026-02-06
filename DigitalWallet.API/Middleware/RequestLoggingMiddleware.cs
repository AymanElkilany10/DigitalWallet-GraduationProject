using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DigitalWallet.API.Middleware
{
    /// <summary>
    /// Logs every inbound HTTP request and its corresponding response in a structured format.
    ///
    /// Captured fields per request:
    ///   • CorrelationId  – unique per request (from X-Correlation-Id header, or auto-generated)
    ///   • Method / Path  – HTTP verb and URL path
    ///   • StatusCode     – HTTP status of the response
    ///   • ElapsedMs      – end-to-end processing time in milliseconds
    ///   • ClientIp       – remote address (first value of X-Forwarded-For when behind a reverse proxy)
    ///   • UserId         – extracted from the JWT claim after authentication (if present)
    ///
    /// The middleware also injects the correlation ID back into every response via the
    /// X-Correlation-Id header so that client-side code can reference it in bug reports.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-Id";
        private const string UserIdClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ── Resolve or generate correlation ID ────────────────────────────
            var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existing)
                ? existing.ToString()
                : Guid.NewGuid().ToString();

            // Attach to response so the client can correlate errors back to server logs
            context.Response.OnStarted(() =>
            {
                context.Response.Headers.Append(CorrelationIdHeader, correlationId);
                return Task.CompletedTask;
            });

            // ── Build the structured log scope ────────────────────────────────
            // Everything inside the scope automatically includes these properties
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["ClientIp"] = GetClientIp(context)
            }))
            {
                _logger.LogInformation(
                    "Request  START  | {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                var stopwatch = Stopwatch.StartNew();

                // ── Call the rest of the pipeline ────────────────────────────
                await _next(context);

                stopwatch.Stop();

                // ── Extract UserId after authentication has run ────────────
                var userId = context.User?.FindFirst(UserIdClaimType)?.Value ?? "anonymous";

                // ── Choose log level based on status code ───────────────────
                // 5xx = Error, 4xx = Warning, 2xx/3xx = Information
                var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error
                             : context.Response.StatusCode >= 400 ? LogLevel.Warning
                             : LogLevel.Information;

                _logger.Log(logLevel,
                    "Request  END    | {Method} {Path} | Status: {StatusCode} | Elapsed: {ElapsedMs}ms | UserId: {UserId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    userId);
            }
        }

        /// <summary>
        /// Returns the real client IP.
        /// When behind a load balancer / reverse proxy the true IP is usually in X-Forwarded-For.
        /// Falls back to RemoteIpAddress.
        /// </summary>
        private static string? GetClientIp(HttpContext context)
        {
            // X-Forwarded-For may contain multiple comma-separated IPs; the first is the original client.
            var forwardedFor = context.Request.Headers.TryGetValue("X-Forwarded-For", out var xff)
                ? xff.ToString().Split(',')[0].Trim()
                : null;

            return forwardedFor ?? context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
