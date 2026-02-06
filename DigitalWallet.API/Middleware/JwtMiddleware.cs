using System.Security.Claims;
using System.Text;

namespace DigitalWallet.API.Middleware
{
    /// <summary>
    /// Custom JWT middleware that complements (or replaces) the default ASP.NET Core
    /// JwtBearer authentication scheme.
    ///
    /// Why it exists:
    ///   The AuthService currently issues a simplified Base64 token of the form
    ///   "{UserId}:{Ticks}".  Until a proper JWT library (e.g. Microsoft.AspNetCore.Authentication.JwtBearer)
    ///   with real signing is wired up, this middleware decodes that token and populates
    ///   HttpContext.User so that [Authorize] and ClaimsPrincipal work correctly.
    ///
    /// Token lifecycle:
    ///   1. Extract the raw token from the Authorization header (Bearer scheme).
    ///   2. Decode Base64 → "{UserId}:{Ticks}".
    ///   3. Parse and validate UserId (must be a valid GUID) and Ticks (must not be in the future,
    ///      and must be within the configured expiry window – default 24 h).
    ///   4. Build a ClaimsPrincipal with NameIdentifier = UserId.
    ///   5. Set HttpContext.User.
    ///
    /// Migration note:
    ///   Once AuthService emits real JWT tokens signed with a secret key, replace the
    ///   Decode/Validate logic below with standard AddJwtBearer() configuration and remove
    ///   this middleware entirely.
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;

        /// <summary>Token lifetime.  Must match the expiry set in AuthService.RegisterAsync / LoginAsync.</summary>
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = ExtractToken(context);

            if (!string.IsNullOrWhiteSpace(token))
            {
                var principal = ValidateAndBuildPrincipal(token);

                if (principal != null)
                {
                    context.User = principal;
                    _logger.LogDebug("JWT middleware: Authenticated UserId {UserId}.",
                        principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                }
                else
                {
                    _logger.LogWarning("JWT middleware: Token present but validation failed.");
                }
            }

            await _next(context);
        }

        // ───────────────────────────── private helpers ─────────────────────────

        /// <summary>
        /// Pulls the raw token string out of the Authorization: Bearer header.
        /// Returns null when the header is absent or does not follow the Bearer scheme.
        /// </summary>
        private static string? ExtractToken(HttpContext context)
        {
            var authHeader = context.Request.Headers.Authorization;

            if (string.IsNullOrWhiteSpace(authHeader))
                return null;

            // Expected format: "Bearer <token>"
            var parts = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                return null;

            return parts[1];
        }

        /// <summary>
        /// Decodes the simplified token and returns a ClaimsPrincipal on success, or null on any failure.
        /// Failures include: malformed Base64, non-GUID UserId, missing Ticks, expired token, or future-dated token.
        /// </summary>
        private ClaimsPrincipal? ValidateAndBuildPrincipal(string token)
        {
            try
            {
                // ── 1. Decode ─────────────────────────────────────────────────
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));

                // Expected decoded format: "{Guid}:{Ticks}"
                var colonIndex = decoded.IndexOf(':');
                if (colonIndex < 0)
                {
                    _logger.LogWarning("JWT middleware: Decoded token missing ':' separator.");
                    return null;
                }

                var userIdRaw = decoded[..colonIndex];
                var ticksRaw = decoded[(colonIndex + 1)..];

                // ── 2. Validate UserId ────────────────────────────────────────
                if (!Guid.TryParse(userIdRaw, out var userId))
                {
                    _logger.LogWarning("JWT middleware: UserId is not a valid GUID.");
                    return null;
                }

                // ── 3. Validate Ticks / expiry ────────────────────────────────
                if (!long.TryParse(ticksRaw, out var ticks))
                {
                    _logger.LogWarning("JWT middleware: Ticks value is not a valid long.");
                    return null;
                }

                var issuedAt = new DateTime(ticks, DateTimeKind.Utc);
                var now = DateTime.UtcNow;

                // Reject tokens issued in the future (clock skew tolerance: 5 minutes)
                if (issuedAt > now.AddMinutes(5))
                {
                    _logger.LogWarning("JWT middleware: Token issued in the future. IssuedAt: {IssuedAt}", issuedAt);
                    return null;
                }

                // Reject tokens past their lifetime
                if (now > issuedAt.Add(TokenLifetime))
                {
                    _logger.LogWarning("JWT middleware: Token expired. IssuedAt: {IssuedAt}", issuedAt);
                    return null;
                }

                // ── 4. Build ClaimsPrincipal ──────────────────────────────────
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim("iat", issuedAt.ToString("O")) // issued-at for debugging
                };

                var identity = new ClaimsIdentity(claims, "SimplifiedJwt");
                var principal = new ClaimsPrincipal(identity);

                return principal;
            }
            catch (Exception ex)
            {
                // Covers FormatException from Base64, OverflowException from DateTime, etc.
                _logger.LogWarning(ex, "JWT middleware: Exception during token validation.");
                return null;
            }
        }
    }
}
