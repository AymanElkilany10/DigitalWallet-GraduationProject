using DigitalWallet.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Web.Http.Filters;

namespace DigitalWallet.API.Filters
{
    // ─────────────────────────────────────────────────────────────────────────
    // 1.  Attribute  –  applied to controllers / actions that demand a specific
    //     role claim.  If no roles are supplied the attribute simply enforces
    //     authentication (same as [Authorize]).
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Declarative attribute that triggers <see cref="AuthorizationFilter"/> at run-time.
    ///
    /// Usage examples:
    ///   [RoleAuthorize]                        // authenticated only
    ///   [RoleAuthorize("SuperAdmin")]          // must have SuperAdmin role claim
    ///   [RoleAuthorize("SuperAdmin","Support")]// either role is acceptable
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RoleAuthorizeAttribute : Attribute, IFilterFactory
    {
        /// <summary>Set of acceptable roles.  Empty means "any authenticated user".</summary>
        public string[] Roles { get; }

        public RoleAuthorizeAttribute(params string[] roles)
        {
            Roles = roles;
        }

        /// <summary>
        /// ASP.NET Core calls this to obtain the actual filter instance, injecting its dependencies.
        /// </summary>
        public bool IsReusable => true; // safe: the filter is stateless

        public IFilter CreateFilter(IServiceProvider serviceProvider)
        {
            return new AuthorizationFilter(
                Roles,
                serviceProvider.GetRequiredService<ILogger<AuthorizationFilter>>());
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2.  Filter  –  the actual logic executed for every request that has the
    //     attribute applied.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Checks that the request is authenticated AND (optionally) that the principal
    /// holds at least one of the roles specified by <see cref="RoleAuthorizeAttribute"/>.
    ///
    /// Response contract:
    ///   • Not authenticated          → 401  with ApiResponse error message
    ///   • Authenticated but no role  → 403  with ApiResponse error message
    ///   • All checks pass            → pipeline continues normally
    /// </summary>
    public class AuthorizationFilter : IActionFilter
    {
        private readonly string[] _roles;
        private readonly ILogger<AuthorizationFilter> _logger;

        public AuthorizationFilter(string[] roles, ILogger<AuthorizationFilter> logger)
        {
            _roles = roles;
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;

            // ── 1. Authentication check ──────────────────────────────────────
            if (user == null || !user.Identity!.IsAuthenticated)
            {
                _logger.LogWarning("AuthorizationFilter: Unauthenticated request to {Path}.",
                    context.HttpContext.Request.Path);

                context.Result = new UnauthorizedObjectResult(
                    ApiResponse<object>.ErrorResponse("Authentication is required."));
                return;
            }

            // ── 2. Role check (skipped when no roles were specified) ──────────
            if (_roles.Length == 0)
                return; // attribute used as a pure authentication gate

            // The role claim key used throughout the project
            const string roleClaimType = ClaimTypes.Role;

            var userRoles = user.Claims
                .Where(c => c.Type == roleClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var hasRequiredRole = _roles.Any(r => userRoles.Contains(r));

            if (!hasRequiredRole)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                _logger.LogWarning(
                    "AuthorizationFilter: UserId {UserId} lacks required role(s) [{Roles}] for {Path}.",
                    userId,
                    string.Join(", ", _roles),
                    context.HttpContext.Request.Path);

                context.Result = new ForbiddenObjectResult(
                    ApiResponse<object>.ErrorResponse(
                        $"Access denied. Required role(s): {string.Join(", ", _roles)}."));
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No post-action logic needed
        }
    }
}
