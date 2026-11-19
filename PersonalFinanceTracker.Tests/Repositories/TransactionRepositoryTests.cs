using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Implementations;

namespace PersonalFinanceTracker.Tests.Repositories;

[TestClass]
public class TransactionRepositoryTests
{
    private FinanceTrackerDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new FinanceTrackerDbContext(options);
    }

    [TestMethod]
    public async Task GetTransactionByIdAsync_ReturnsTransaction_WhenExists()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m };
        var category = new Category { Name = "Food", Type = CategoryType.Expense };
        context.Accounts.Add(account);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var transaction = new Transaction
        {
            Amount = 25.50m,
            Description = "Lunch",
            AccountId = account.Id,
            CategoryId = category.Id,
            DateOfTransaction = DateTime.UtcNow
        };
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var repo = new TransactionRepository(context);
        var result = await repo.GetTransactionByIdAsync(transaction.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(25.50m, result.Amount);
        Assert.AreEqual("Lunch", result.Description);
        Assert.IsNotNull(result.Account);
        Assert.IsNotNull(result.Category);
    }

    [TestMethod]
    public async Task GetTransactionByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = GetInMemoryDbContext();
        var repo = new TransactionRepository(context);
        var result = await repo.GetTransactionByIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllTransactionsAsync_ReturnsAllTransactions()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m };
        var category = new Category { Name = "Food", Type = CategoryType.Expense };
        context.Accounts.Add(account);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        context.Transactions.Add(new Transaction { Amount = 10m, Description = "Coffee", AccountId = account.Id, CategoryId = category.Id });
        context.Transactions.Add(new Transaction { Amount = 50m, Description = "Dinner", AccountId = account.Id, CategoryId = category.Id });
        await context.SaveChangesAsync();

        var repo = new TransactionRepository(context);
        var transactions = (await repo.GetAllTransactionsAsync()).ToList();

        Assert.AreEqual(2, transactions.Count);
    }

    [TestMethod]
    public async Task CreateTransactionAsync_AddsTransaction()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m };
        var category = new Category { Name = "Salary", Type = CategoryType.Income };
        context.Accounts.Add(account);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var repo = new TransactionRepository(context);
        var transaction = new Transaction
        {
            Amount = 1500m,
            Description = "Monthly Salary",
            AccountId = account.Id,
            CategoryId = category.Id
        };

        var created = await repo.CreateTransactionAsync(transaction);

        Assert.IsTrue(created.Id > 0);
        var inDb = await context.Transactions.FindAsync(created.Id);
        Assert.IsNotNull(inDb);
        Assert.AreEqual(1500m, inDb.Amount);
    }

    [TestMethod]
    public async Task UpdateTransactionAsync_UpdatesTransaction()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m };
        var category = new Category { Name = "Food", Type = CategoryType.Expense };
        context.Accounts.Add(account);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var transaction = new Transaction
        {
            Amount = 20m,
            Description = "Snack",
            AccountId = account.Id,
            CategoryId = category.Id
        };
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var repo = new TransactionRepository(context);
        transaction.Description = "Groceries Snack";
        transaction.Amount = 25m;
        await repo.UpdateTransactionAsync(transaction);

        var updated = await context.Transactions.FindAsync(transaction.Id);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Groceries Snack", updated.Description);
        Assert.AreEqual(25m, updated.Amount);
    }

    [TestMethod]
    public async Task DeleteTransactionAsync_RemovesTransaction()
    {
        using var context = GetInMemoryDbContext();
        var account = new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m };
        var category = new Category { Name = "Food", Type = CategoryType.Expense };
        context.Accounts.Add(account);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var transaction = new Transaction
        {
            Amount = 20m,
            Description = "Snack",
            AccountId = account.Id,
            CategoryId = category.Id
        };
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var repo = new TransactionRepository(context);
        await repo.DeleteTransactionAsync(transaction.Id);

        var deleted = await context.Transactions.FindAsync(transaction.Id);
        Assert.IsNull(deleted);
    }
}
