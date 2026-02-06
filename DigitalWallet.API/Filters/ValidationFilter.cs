using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Filters
{
    /// <summary>
    /// Action filter that intercepts requests with ModelState errors
    /// (populated either by the default [ApiController] model-validation or by
    /// FluentValidation's model-validation integration) and returns a uniform
    /// 400 response shaped like <see cref="ApiResponse{T}"/>.
    ///
    /// This replaces the default ASP.NET Core error response so every validation
    /// failure across the entire API has the same JSON structure.
    ///
    /// How to register (one-time, in Program.cs or ServiceCollectionExtensions):
    ///   builder.Services.AddControllers(options =>
    ///       options.Filters.Add<ValidationFilter>());
    /// </summary>
    public class ValidationFilter : IActionFilter
    {
        private readonly ILogger<ValidationFilter> _logger;

        public ValidationFilter(ILogger<ValidationFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>No-op; we only need the post-binding hook.</summary>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Intentionally empty – see OnActionExecuted
        }

        /// <summary>No-op; validation is checked before the action runs.</summary>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Intentionally empty
        }

        /// <summary>
        /// Runs before the action method is invoked.  If ModelState is invalid the action
        /// is short-circuited and a 400 <see cref="ApiResponse{object}"/> is returned.
        /// </summary>
        /// <remarks>
        /// We use the non-async overload because there is no I/O here; it is purely
        /// in-memory inspection of the ModelState dictionary.
        /// </remarks>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
                return; // nothing to do

            // ── Collect all error messages in a flat list ─────────────────────
            var errors = new List<string>();

            foreach (var kvp in context.ModelState)
            {
                if (kvp.Value == null) continue;

                foreach (var error in kvp.Value.Errors)
                {
                    // Prefer the developer-supplied message; fall back to the exception text
                    var message = !string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? error.ErrorMessage
                        : error.Exception?.Message ?? "Validation error";

                    // Prepend the property name when available so the client knows which field failed
                    if (!string.IsNullOrWhiteSpace(kvp.Key))
                        errors.Add($"{kvp.Key}: {message}");
                    else
                        errors.Add(message);
                }
            }

            _logger.LogWarning("Validation failed. Errors: [{Errors}]", string.Join("; ", errors));

            // ── Short-circuit: set the result and stop the pipeline ───────────
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.ErrorResponse(errors));
        }
    }
}
