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

        InitializeAccounts(context);
        InitializeCategories(context);

        context.SaveChanges();
    }

    public static void InitializeAccounts(FinanceTrackerDbContext context)
    {
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
    }

    public static void InitializeCategories(FinanceTrackerDbContext context)
    {
        if (context.Categories.Any())
        {
            return;
        }

        var categories = new Category[]
        {
            new Category { Name = "Utilities", Type = CategoryType.Expense },
            new Category { Name = "Mortgage", Type = CategoryType.Expense },
            new Category { Name = "Salary", Type = CategoryType.Income },
            new Category { Name = "Bonuses", Type = CategoryType.Income },
            new Category { Name = "Groceries", Type = CategoryType.Expense },
            new Category { Name = "Baby Needs", Type = CategoryType.Expense },
            new Category { Name = "Leisure", Type = CategoryType.Expense },
            new Category { Name = "Repairs", Type = CategoryType.Expense },
            new Category { Name = "Food Delivery", Type = CategoryType.Expense },
            new Category { Name = "Others", Type = CategoryType.Expense }
        };

        context.Categories.AddRange(categories);
    }
}
