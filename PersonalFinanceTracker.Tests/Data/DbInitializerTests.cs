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
}
