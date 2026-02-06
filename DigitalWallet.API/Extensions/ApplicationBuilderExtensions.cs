using Microsoft.AspNetCore.Builder;
using DigitalWallet.API.Middleware;

namespace DigitalWallet.API.Extensions
{
    /// <summary>
    /// Extension methods on <see cref="WebApplication"/> (IApplicationBuilder) that
    /// register and order the custom middleware added by this project.
    ///
    /// Middleware order matters – the methods below should be called in the order
    /// shown in Program.cs.  The comments below explain why each piece sits where it does.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="ExceptionHandlingMiddleware"/> as the **outermost** layer so it
        /// can catch exceptions thrown by every other piece of middleware and controller.
        ///
        /// Must be called BEFORE any other Use* / Map* calls on the builder.
        /// </summary>
        public static WebApplication UseExceptionHandling(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }

        /// <summary>
        /// Adds <see cref="RequestLoggingMiddleware"/> immediately after exception handling.
        /// This position means:
        ///   • The stopwatch wraps the full downstream pipeline (auth + controller).
        ///   • Any 5xx from the controller is still logged before ExceptionHandlingMiddleware
        ///     serialises it, because RequestLogging logs on the *way back out*.
        /// </summary>
        public static WebApplication UseRequestLogging(this WebApplication app)
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
            return app;
        }

        /// <summary>
        /// Adds <see cref="JwtMiddleware"/> after logging so that:
        ///   1. The request log already has a correlation ID before auth runs.
        ///   2. If token validation throws (it shouldn't, but defensively) the exception
        ///      is caught by ExceptionHandlingMiddleware.
        ///
        /// This must be called BEFORE app.UseAuthorization() because [Authorize]
        /// attributes inspect HttpContext.User which this middleware populates.
        /// </summary>
        public static WebApplication UseJwtAuthentication(this WebApplication app)
        {
            app.UseMiddleware<JwtMiddleware>();
            return app;
        }

        /// <summary>
        /// Convenience method that registers the three custom middlewares in the
        /// correct order with a single call.  Use this OR the individual methods –
        /// not both.
        /// </summary>
        public static WebApplication UseDigitalWalletMiddleware(this WebApplication app)
        {
            app.UseExceptionHandling();   // 1st – outermost
            app.UseRequestLogging();      // 2nd – wraps everything below
            app.UseJwtAuthentication();   // 3rd – populates HttpContext.User
            return app;
        }
    }
}
