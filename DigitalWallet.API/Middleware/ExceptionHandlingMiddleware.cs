using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using DigitalWallet.Application.Common;
using DigitalWallet.Domain.Exceptions;

namespace DigitalWallet.API.Middleware
{
    /// <summary>
    /// Global exception handler that sits at the top of the middleware pipeline.
    /// Catches any unhandled exception thrown by downstream middleware or controllers
    /// and converts it into a uniform JSON <see cref="ApiResponse{T}"/> so the client
    /// always receives a predictable error shape, even on 500 errors.
    ///
    /// Mapping table
    /// ─────────────────────────────────────────────────────────────
    /// Exception type                  │ HTTP status  │ Log level
    /// ─────────────────────────────────────────────────────────────
    /// UnauthorizedAccessException      │ 401          │ Warning
    /// DomainException (base)           │ 400          │ Warning
    ///   └─ InsufficientBalanceEx.      │ 400          │ Warning
    ///   └─ InvalidOtpException         │ 400          │ Warning
    ///   └─ InvalidTransferException    │ 400          │ Warning
    ///   └─ UserNotFoundException       │ 404          │ Warning
    ///   └─ WalletLimitExceededException│ 400          │ Warning
    /// ArgumentException                │ 400          │ Warning
    /// OperationCanceledException       │ 499 / 408    │ Info
    /// Exception (everything else)      │ 500          │ Error   (details hidden)
    /// ─────────────────────────────────────────────────────────────
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode;
            string message;
            string logCategory; // used only for structured logging

            switch (exception)
            {
                // ── 401  Unauthorized ─────────────────────────────────────────
                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = "Unauthorized access.";
                    logCategory = "Unauthorized";
                    _logger.LogWarning(exception, "[{Category}] {Message}", logCategory, message);
                    break;

                // ── 404  Not Found (specific domain exceptions) ──────────────
                case UserNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    message = exception.Message;
                    logCategory = "NotFound";
                    _logger.LogWarning(exception, "[{Category}] {Message}", logCategory, message);
                    break;

                // ── 400  Business-rule violations (domain exceptions) ─────────
                case InsufficientBalanceException
                    or InvalidOtpException
                    or InvalidTransferException
                    or WalletLimitExceededException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = exception.Message;
                    logCategory = "BusinessRule";
                    _logger.LogWarning(exception, "[{Category}] {Message}", logCategory, message);
                    break;

                // ── 400  Generic domain exception base (catch-all for domain) ─
                case DomainException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = exception.Message;
                    logCategory = "DomainError";
                    _logger.LogWarning(exception, "[{Category}] {Message}", logCategory, message);
                    break;

                // ── 400  Bad argument (programming / input errors) ───────────
                case ArgumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = exception.Message;
                    logCategory = "ArgumentError";
                    _logger.LogWarning(exception, "[{Category}] {Message}", logCategory, message);
                    break;

                // ── 408  Request timeout (client cancelled) ───────────────────
                case OperationCanceledException:
                    statusCode = StatusCodes.Status408RequestTimeout;
                    message = "The request was cancelled or timed out.";
                    logCategory = "Cancelled";
                    _logger.LogInformation("[{Category}] {Message}", logCategory, message);
                    break;

                // ── 500  Unhandled – hide internals from the client ──────────
                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "An unexpected error occurred. Please try again later.";
                    logCategory = "Unhandled";
                    _logger.LogError(exception, "[{Category}] Unhandled exception.", logCategory);
                    break;
            }

            // Prevent double-writing if the response has already started (e.g. streaming)
            if (context.Response.HasStarted)
            {
                _logger.LogCritical("[ExceptionMiddleware] Response already started; cannot write error body.");
                return;
            }

            // ── Write the uniform JSON error response ─────────────────────────
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.ErrorResponse(message);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteGetterOnly = true
            };

            await context.Response.WriteAsJsonAsync(response, jsonOptions);
        }
    }
}
