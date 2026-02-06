using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using DigitalWallet.API.Filters;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Services;

namespace DigitalWallet.API.Extensions
{
    /// <summary>
    /// Centralised DI wiring for every layer the API depends on.
    /// Call <see cref="AddApplicationServices"/> from Program.cs after
    /// <c>builder.Services</c> is available.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all Application-layer services, validators, mapping profiles,
        /// and the custom authorization policies required by the API.
        /// </summary>
        /// <param name="services">The service collection to extend.</param>
        /// <returns>The same collection for chaining.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // ── 1.  Application Services (business logic) ─────────────────────
            // Each interface lives in DigitalWallet.Application.Interfaces.Services
            // and its implementation in DigitalWallet.Application.Services.
            // Scoped lifetime: one instance per HTTP request, shared across all
            // controllers / middleware that participate in that request.

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITransferService, TransferService>();
            services.AddScoped<IMoneyRequestService, MoneyRequestService>();
            services.AddScoped<IFakeBankService, FakeBankService>();
            services.AddScoped<IBillPaymentService, BillPaymentService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAdminService, AdminService>();

            // ── 2.  AutoMapper ────────────────────────────────────────────────
            // Scans DigitalWallet.Application for any class that inherits AutoMapper.Profile
            // (currently MappingProfile.cs) and registers it automatically.
            services.AddAutoMapper(typeof(DigitalWallet.Application.Mappings.MappingProfile).Assembly);

            // ── 3.  FluentValidation ──────────────────────────────────────────
            // Scans the Application assembly for every AbstractValidator<T> subclass
            // and registers them as transient.  The ValidationFilter (or the built-in
            // FluentValidation model validation pipeline) will resolve them at request time.
            //
            // NOTE: If you add FluentValidation.AspNetCore to the API .csproj you can
            // replace this block with:
            //     services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(...));
            // For now we register validators manually to keep the .csproj minimal.

            services.AddTransient<
                FluentValidation.IValidator<DigitalWallet.Application.DTOs.Auth.RegisterRequestDto>,
                DigitalWallet.Application.Validators.RegisterRequestValidator>();

            services.AddTransient<
                FluentValidation.IValidator<DigitalWallet.Application.DTOs.Auth.LoginRequestDto>,
                DigitalWallet.Application.Validators.LoginRequestValidator>();

            services.AddTransient<
                FluentValidation.IValidator<DigitalWallet.Application.DTOs.Transfer.SendMoneyRequestDto>,
                DigitalWallet.Application.Validators.SendMoneyRequestValidator>();

            services.AddTransient<
                FluentValidation.IValidator<DigitalWallet.Application.DTOs.BillPayment.PayBillRequestDto>,
                DigitalWallet.Application.Validators.PayBillRequestValidator>();

            services.AddTransient<
                FluentValidation.IValidator<DigitalWallet.Application.DTOs.FakeBank.DepositRequestDto>,
                DigitalWallet.Application.Validators.DepositRequestValidator>();

            // ── 4.  Authorization Policies ────────────────────────────────────
            // The AdminController uses [Authorize(Policy = "AdminOnly")].
            // This policy requires the ClaimTypes.Role claim to contain "SuperAdmin"
            // OR "Support".  Adjust the allowed roles list as your RBAC model evolves.
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        System.Security.Claims.ClaimTypes.Role,
                        "SuperAdmin", "Support");
                });
            });

            return services;
        }
    }
}
