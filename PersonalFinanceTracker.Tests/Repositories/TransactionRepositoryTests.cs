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

    [TestMethod]
    public async Task GetTransactionsByAccountIdAsync_ReturnsFilteredAndOrderedTransactions()
    {
        using var context = GetInMemoryDbContext();
        var account1 = new Account { Id = 1, Name = "Checking", Type = "Bank", CurrentBalance = 1000m };
        var account2 = new Account { Id = 2, Name = "Savings", Type = "Bank", CurrentBalance = 5000m };
        var category = new Category { Id = 1, Name = "Food", Type = CategoryType.Expense };
        context.Accounts.AddRange(account1, account2);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var t1 = new Transaction { AccountId = 1, CategoryId = 1, Amount = -10m, Description = "Old", DateOfTransaction = DateTime.UtcNow.AddDays(-2) };
        var t2 = new Transaction { AccountId = 1, CategoryId = 1, Amount = -20m, Description = "New", DateOfTransaction = DateTime.UtcNow.AddDays(-1) };
        var t3 = new Transaction { AccountId = 2, CategoryId = 1, Amount = -30m, Description = "Other", DateOfTransaction = DateTime.UtcNow };
        context.Transactions.AddRange(t1, t2, t3);
        await context.SaveChangesAsync();

        var repo = new TransactionRepository(context);
        var result = (await repo.GetTransactionsByAccountIdAsync(1, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(10))).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("New", result[0].Description); // Ordered descending by date
        Assert.AreEqual("Old", result[1].Description);
    }
}
