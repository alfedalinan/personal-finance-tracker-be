using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Entities;

namespace PersonalFinanceTracker.Tests.Data;

[TestClass]
public class DbInitializerTests
{
    [TestMethod]
    public void Initialize_SeedsAccounts_WhenDatabaseIsEmpty()
    {
        var services = new ServiceCollection();
        var options = new DbContextOptionsBuilder<FinanceTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        services.AddSingleton(options);
        services.AddScoped<FinanceTrackerDbContext>();
        var serviceProvider = services.BuildServiceProvider();

        // Run initializer
        using (var scope = serviceProvider.CreateScope())
        {
            DbInitializer.Initialize(scope.ServiceProvider);
        }

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FinanceTrackerDbContext>();
            var accounts = context.Accounts.OrderBy(a => a.Id).ToList();

            Assert.AreEqual(2, accounts.Count);
            
            Assert.AreEqual("Primary Savings", accounts[0].Name);
            Assert.AreEqual("Savings", accounts[0].Type);
            Assert.AreEqual(50000.00m, accounts[0].CurrentBalance);

            Assert.AreEqual("Main Credit Card", accounts[1].Name);
            Assert.AreEqual("Credit Card", accounts[1].Type);
            Assert.AreEqual(200000.00m, accounts[1].CurrentBalance);
        }
    }

    [TestMethod]
    public void Initialize_IsIdempotent_WhenAccountsAlreadyExist()
    {
        var services = new ServiceCollection();
        var options = new DbContextOptionsBuilder<FinanceTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        services.AddSingleton(options);
        services.AddScoped<FinanceTrackerDbContext>();
        var serviceProvider = services.BuildServiceProvider();

        // Pre-seed an account
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FinanceTrackerDbContext>();
            context.Database.EnsureCreated();
            context.Accounts.Add(new Account { Name = "Existing Account", Type = "Checking", CurrentBalance = 100m });
            context.SaveChanges();
        }

        // Run initializer
        using (var scope = serviceProvider.CreateScope())
        {
            DbInitializer.Initialize(scope.ServiceProvider);
        }

        using (var scope = serviceProvider.CreateScope())
        {
            var verifyContext = scope.ServiceProvider.GetRequiredService<FinanceTrackerDbContext>();
            var accounts = verifyContext.Accounts.ToList();

            // Should not seed the default accounts since Accounts table was not empty
            Assert.AreEqual(1, accounts.Count);
            Assert.AreEqual("Existing Account", accounts[0].Name);
        }
    }

    [TestMethod]
    public void Initialize_SeedsCategories_WhenDatabaseIsEmpty()
    {
        var services = new ServiceCollection();
        var options = new DbContextOptionsBuilder<FinanceTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        services.AddSingleton(options);
        services.AddScoped<FinanceTrackerDbContext>();
        var serviceProvider = services.BuildServiceProvider();

        // Run initializer
        using (var scope = serviceProvider.CreateScope())
        {
            DbInitializer.Initialize(scope.ServiceProvider);
        }

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FinanceTrackerDbContext>();
            var categories = context.Categories.OrderBy(c => c.Id).ToList();

            Assert.AreEqual(10, categories.Count);

            var expectedCategories = new[]
            {
                ("Utilities", CategoryType.Expense),
                ("Mortgage", CategoryType.Expense),
                ("Salary", CategoryType.Income),
                ("Bonuses", CategoryType.Income),
                ("Groceries", CategoryType.Expense),
                ("Baby Needs", CategoryType.Expense),
                ("Leisure", CategoryType.Expense),
                ("Repairs", CategoryType.Expense),
                ("Food Delivery", CategoryType.Expense),
                ("Others", CategoryType.Expense)
            };

            for (int i = 0; i < expectedCategories.Length; i++)
            {
                Assert.AreEqual(expectedCategories[i].Item1, categories[i].Name);
                Assert.AreEqual(expectedCategories[i].Item2, categories[i].Type);
            }
        }
    }

    [TestMethod]
    public void Initialize_IsIdempotent_WhenCategoriesAlreadyExist()
    {
        var services = new ServiceCollection();
        var options = new DbContextOptionsBuilder<FinanceTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        services.AddSingleton(options);
        services.AddScoped<FinanceTrackerDbContext>();
        var serviceProvider = services.BuildServiceProvider();

        // Pre-seed an account and a category
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FinanceTrackerDbContext>();
            context.Database.EnsureCreated();
            context.Accounts.Add(new Account { Name = "Existing Account", Type = "Checking", CurrentBalance = 100m });
            context.Categories.Add(new Category { Name = "Custom Category", Type = CategoryType.Income });
            context.SaveChanges();
        }

        // Run initializer
        using (var scope = serviceProvider.CreateScope())
        {
            DbInitializer.Initialize(scope.ServiceProvider);
        }

        using (var scope = serviceProvider.CreateScope())
        {
            var verifyContext = scope.ServiceProvider.GetRequiredService<FinanceTrackerDbContext>();
            var categories = verifyContext.Categories.ToList();

            // Should not seed the default categories since Categories table was not empty
            Assert.AreEqual(1, categories.Count);
            Assert.AreEqual("Custom Category", categories[0].Name);
        }
    }
}
