using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.OpenApi.Models;

namespace DigitalWallet.API.Extensions
{
    /// <summary>
    /// Swagger / OpenAPI configuration helpers.
    /// Keeps Program.cs clean and makes it easy to tweak the generated docs
    /// independently of the rest of the startup sequence.
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Configures the Swagger generator:
        ///   • Title, version, and description shown at the top of the Swagger UI page.
        ///   • A "Bearer" security definition so developers can paste their token once
        ///     and have it sent on every request.
        ///   • XML doc comments are included when the project is built with
        ///     &lt;GenerateDocumentationFile&gt;true&lt;/GenerateDocumentationFile&gt;.
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // ── Basic info ────────────────────────────────────────────────
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "DigitalWallet API",
                    Version = "v1",
                    Description = "RESTful API for the DigitalWallet graduation project. "
                                 + "Supports user registration/login, wallet management, "
                                 + "money transfers, bill payments, and admin operations.",
                    Contact = new OpenApiContact
                    {
                        Name = "Development Team",
                        Email = "dev@digitalwallet.local"
                    },
                    License = new OpenApiLicense { Name = "MIT" }
                });

                // ── Bearer token security scheme ─────────────────────────────
                // Tells Swagger UI to show an "Authorize" button that prepends
                // "Bearer <token>" to every request's Authorization header.
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your token in the text box below.\r\n\r\n"
                                + "Example: <b>eyJhbGci...</b>"
                });

                // Apply the Bearer scheme globally so every endpoint shows the lock icon.
                // Endpoints marked [AllowAnonymous] will still accept unauthenticated calls;
                // this just ensures the header is sent when the token is provided.
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // ── XML documentation comments ───────────────────────────────
                // Uncomment and set the correct path once GenerateDocumentationFile is enabled.
                // var xmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                //     $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                // if (File.Exists(xmlFile))
                //     options.IncludeXmlComments(xmlFile);

                // ── Avoid "duplicate schema" errors when two DTOs share a name ─
                options.UseFullyQualifiedSchemaNames();
            });

            return services;
        }

        /// <summary>
        /// Activates the Swagger middleware and its UI.
        /// Conditionally enabled: only runs when the environment is <b>Development</b>
        /// (controlled by the caller in Program.cs via <c>app.Environment.IsDevelopment()</c>).
        /// </summary>
        public static WebApplication UseSwaggerDocumentation(this WebApplication app)
        {
            app.UseSwagger(options =>
            {
                // Serve the spec at /swagger/v1/swagger.json (default)
                options.RoutePrefix = "swagger";
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "DigitalWallet API v1");
                options.RoutePrefix = "swagger"; // UI available at /swagger
                options.DocumentTitle = "DigitalWallet – Swagger UI";

                // Persist authorisation across page refreshes (token stays in the textbox)
                options.ConfigObject.AdditionalInterceptors = new Dictionary<string, object>
                {
                    ["persistAuthorization"] = true
                };
            });

            return app;
        }
    }
}
