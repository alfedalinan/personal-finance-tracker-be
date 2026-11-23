using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Implementations;

namespace PersonalFinanceTracker.Tests.Repositories;

[TestClass]
public class AccountRepositoryTests
{
    private FinanceTrackerDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new FinanceTrackerDbContext(options);
    }

    [TestMethod]
    public async Task GetAccountByIdAsync_ReturnsAccount_WhenExists()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Checking", Type = "Bank", Slug = "checking", CurrentBalance = 1000m };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var repo = new AccountRepository(context);
        var result = await repo.GetAccountByIdAsync(account.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual("Checking", result.Name);
        Assert.AreEqual(1000m, result.CurrentBalance);
    }

    [TestMethod]
    public async Task GetAccountBySlugAsync_ReturnsAccount_WhenExists()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Savings", Type = "Bank", Slug = "primary-savings", CurrentBalance = 1000m };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var repo = new AccountRepository(context);
        var result = await repo.GetAccountBySlugAsync("PRIMARY-SAVINGS");

        Assert.IsNotNull(result);
        Assert.AreEqual("Savings", result.Name);
        Assert.AreEqual("primary-savings", result.Slug);
    }

    [TestMethod]
    public async Task GetAccountBySlugAsync_ReturnsNull_WhenNotExists()
    {
        using var context = GetInMemoryDbContext();
        var repo = new AccountRepository(context);
        var result = await repo.GetAccountBySlugAsync("non-existent");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAccountByTypeAsync_ReturnsAccount_WhenExists()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Savings", Type = "Savings", Slug = "primary-savings", CurrentBalance = 1000m };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var repo = new AccountRepository(context);
        var result = await repo.GetAccountByTypeAsync("savings");

        Assert.IsNotNull(result);
        Assert.AreEqual("Savings", result.Type);
    }

    [TestMethod]
    public async Task GetAccountByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = GetInMemoryDbContext();
        var repo = new AccountRepository(context);
        var result = await repo.GetAccountByIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllAccountsAsync_ReturnsAllAccounts()
    {
        using var context = GetInMemoryDbContext();
        context.Accounts.Add(new Account { Name = "Checking", Type = "Bank", CurrentBalance = 500m });
        context.Accounts.Add(new Account { Name = "Savings", Type = "Bank", CurrentBalance = 5000m });
        await context.SaveChangesAsync();

        var repo = new AccountRepository(context);
        var accounts = (await repo.GetAllAccountsAsync()).ToList();

        Assert.AreEqual(2, accounts.Count);
    }

    [TestMethod]
    public async Task CreateAccountAsync_AddsAccount()
    {
        using var context = GetInMemoryDbContext();
        var repo = new AccountRepository(context);
        var account = new Account { Name = "Wallet", Type = "Cash", CurrentBalance = 50m };

        var created = await repo.CreateAccountAsync(account);

        Assert.IsTrue(created.Id > 0);
        var inDb = await context.Accounts.FindAsync(created.Id);
        Assert.IsNotNull(inDb);
        Assert.AreEqual("Wallet", inDb.Name);
    }

    [TestMethod]
    public async Task UpdateAccountAsync_UpdatesAccount()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Checking", Type = "Bank", CurrentBalance = 100m };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var repo = new AccountRepository(context);
        account.CurrentBalance = 250m;
        await repo.UpdateAccountAsync(account);

        var updated = await context.Accounts.FindAsync(account.Id);
        Assert.IsNotNull(updated);
        Assert.AreEqual(250m, updated.CurrentBalance);
    }

    [TestMethod]
    public async Task DeleteAccountAsync_RemovesAccount()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Checking", Type = "Bank", CurrentBalance = 100m };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var repo = new AccountRepository(context);
        await repo.DeleteAccountAsync(account.Id);

        var deleted = await context.Accounts.FindAsync(account.Id);
        Assert.IsNull(deleted);
    }

    [TestMethod]
    public async Task GetAllAccountsWithRecentTransactionsAsync_ReturnsTop5OrderedTransactions()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var category = new Category { Name = "Food", Type = CategoryType.Expense };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        for (int i = 1; i <= 7; i++)
        {
            context.Transactions.Add(new Transaction
            {
                AccountId = account.Id,
                CategoryId = category.Id,
                Amount = 10m * i,
                BeforeBalance = 1000m,
                AfterBalance = 990m,
                Description = $"Tx {i}",
                DateOfTransaction = DateTime.UtcNow.AddDays(-i)
            });
        }
        await context.SaveChangesAsync();

        var repo = new AccountRepository(context);
        var result = (await repo.GetAllAccountsWithRecentTransactionsAsync()).ToList();

        Assert.AreEqual(1, result.Count);
        var txs = result[0].Transactions.ToList();
        Assert.AreEqual(5, txs.Count);
        // Should be ordered descending by date (most recent first, i.e., -1 day, -2 days, -3 days, -4 days, -5 days)
        Assert.AreEqual("Tx 1", txs[0].Description);
        Assert.AreEqual("Tx 5", txs[4].Description);
    }
}
