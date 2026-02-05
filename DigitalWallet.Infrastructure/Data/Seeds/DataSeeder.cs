using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Data.Seeds
{
    public static class DataSeeder
    {
        public static void SeedData(ApplicationDbContext context)
        {
            // Seed Billers
            if (!context.Billers.Any())
            {
                var billers = new List<Biller>
                {
                    new Biller
                    {
                        Id = Guid.NewGuid(),
                        Name = "Egyptian Electricity Company",
                        Category = BillCategory.Electricity,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Biller
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cairo Water Authority",
                        Category = BillCategory.Water,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Biller
                    {
                        Id = Guid.NewGuid(),
                        Name = "Telecom Egypt",
                        Category = BillCategory.Internet,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Biller
                    {
                        Id = Guid.NewGuid(),
                        Name = "Vodafone Egypt",
                        Category = BillCategory.Internet,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Biller
                    {
                        Id = Guid.NewGuid(),
                        Name = "Netflix",
                        Category = BillCategory.Subscription,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Biller
                    {
                        Id = Guid.NewGuid(),
                        Name = "Spotify",
                        Category = BillCategory.Subscription,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Billers.AddRange(billers);
                context.SaveChanges();
            }
        }
    }
}