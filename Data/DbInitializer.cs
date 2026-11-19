using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Entities;

namespace PersonalFinanceTracker.Data;

public static class DbInitializer
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new FinanceTrackerDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<FinanceTrackerDbContext>>());

        context.Database.EnsureCreated();

        if (context.Accounts.Any())
        {
            return;
        }

        var accounts = new Account[]
        {
            new Account
            {
                Name = "Primary Savings",
                Type = "Savings",
                CurrentBalance = 50000.00m
            },
            new Account
            {
                Name = "Main Credit Card",
                Type = "Credit Card",
                CurrentBalance = 200000.00m
            }
        };

        context.Accounts.AddRange(accounts);
        context.SaveChanges();
    }
}
