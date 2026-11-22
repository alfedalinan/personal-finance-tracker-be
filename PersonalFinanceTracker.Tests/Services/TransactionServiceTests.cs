using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Implementations;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Implementations;

namespace PersonalFinanceTracker.Tests.Services;

[TestClass]
public class TransactionServiceTests
{
    private FinanceTrackerDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new FinanceTrackerDbContext(options);
    }

    [TestMethod]
    public async Task CreateTransactionAsync_Income_IncreasesAccountBalance()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var account = await accountRepo.CreateAccountAsync(new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m });
        var category = await categoryRepo.CreateCategoryAsync(new Category { Name = "Salary", Type = CategoryType.Income });

        var request = new CreateTransactionRequestDto(account.Id, category.Id, "Monthly Pay", 500.00m);

        var result = await service.CreateTransactionAsync(request);

        Assert.IsNotNull(result);
        var updatedAccount = await accountRepo.GetAccountByIdAsync(account.Id);
        Assert.AreEqual(1500.00m, updatedAccount!.CurrentBalance);
    }

    [TestMethod]
    public async Task CreateTransactionAsync_Expense_DecreasesAccountBalance()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var account = await accountRepo.CreateAccountAsync(new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m });
        var category = await categoryRepo.CreateCategoryAsync(new Category { Name = "Groceries", Type = CategoryType.Expense });

        var request = new CreateTransactionRequestDto(account.Id, category.Id, "Supermarket", 250.00m);

        var result = await service.CreateTransactionAsync(request);

        Assert.IsNotNull(result);
        var updatedAccount = await accountRepo.GetAccountByIdAsync(account.Id);
        Assert.AreEqual(750.00m, updatedAccount!.CurrentBalance);
    }

    [TestMethod]
    public async Task CreateTransactionAsync_SetsBeforeAndAfterBalancesCorrectly()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var account = await accountRepo.CreateAccountAsync(new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m });
        var incomeCategory = await categoryRepo.CreateCategoryAsync(new Category { Name = "Salary", Type = CategoryType.Income });
        var expenseCategory = await categoryRepo.CreateCategoryAsync(new Category { Name = "Groceries", Type = CategoryType.Expense });

        var t1 = await service.CreateTransactionAsync(new CreateTransactionRequestDto(account.Id, incomeCategory.Id, "Pay", 500m));
        Assert.AreEqual(1000m, t1.BeforeBalance);
        Assert.AreEqual(1500m, t1.AfterBalance);

        var t2 = await service.CreateTransactionAsync(new CreateTransactionRequestDto(account.Id, expenseCategory.Id, "Food", 200m));
        Assert.AreEqual(1500m, t2.BeforeBalance);
        Assert.AreEqual(1300m, t2.AfterBalance);
    }

    [TestMethod]
    public async Task UpdateTransactionAsync_ThrowsInvalidOperationException_EnforcingImmutability()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var account = await accountRepo.CreateAccountAsync(new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m });
        var expenseCategory = await categoryRepo.CreateCategoryAsync(new Category { Name = "Food", Type = CategoryType.Expense });

        var created = await service.CreateTransactionAsync(new CreateTransactionRequestDto(account.Id, expenseCategory.Id, "Lunch", 100.00m));

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            service.UpdateTransactionAsync(created.Id, new CreateTransactionRequestDto(account.Id, expenseCategory.Id, "Updated Lunch", 120.00m)));
    }

    [TestMethod]
    public async Task DeleteTransactionAsync_ThrowsInvalidOperationException_EnforcingImmutability()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var account = await accountRepo.CreateAccountAsync(new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m });
        var category = await categoryRepo.CreateCategoryAsync(new Category { Name = "Utilities", Type = CategoryType.Expense });

        var created = await service.CreateTransactionAsync(new CreateTransactionRequestDto(account.Id, category.Id, "Power Bill", 150.00m));

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            service.DeleteTransactionAsync(created.Id));
    }

    [TestMethod]
    public async Task CreateTransactionAsync_ThrowsException_WhenAccountNotFound()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var category = await categoryRepo.CreateCategoryAsync(new Category { Name = "Salary", Type = CategoryType.Income });

        var request = new CreateTransactionRequestDto(999, category.Id, "Test", 100m);

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.CreateTransactionAsync(request));
    }

    [TestMethod]
    public async Task GetAccountTransactionsAndBalanceAsync_ReturnsCorrectCurrentBalance()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var account = await accountRepo.CreateAccountAsync(new Account { Name = "Savings", Type = "Bank", CurrentBalance = 2000m });
        var category = await categoryRepo.CreateCategoryAsync(new Category { Name = "Gift", Type = CategoryType.Income });

        await service.CreateTransactionAsync(new CreateTransactionRequestDto(account.Id, category.Id, "Birthday", 200m));

        var result = await service.GetAccountTransactionsAndBalanceAsync(account.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(2200.00m, result.CurrentBalance);
        Assert.AreEqual(2200.00m, result.RemainingBalance);
    }

    [TestMethod]
    public async Task CreateTransactionAsync_OmittedDate_AssignsUtcNowInUtc()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var account = await accountRepo.CreateAccountAsync(new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m });
        var category = await categoryRepo.CreateCategoryAsync(new Category { Name = "Salary", Type = CategoryType.Income });

        var beforeCall = DateTime.UtcNow;
        var request = new CreateTransactionRequestDto(account.Id, category.Id, "Pay", 500m);
        var result = await service.CreateTransactionAsync(request);
        var afterCall = DateTime.UtcNow;

        Assert.IsNotNull(result);
        Assert.AreEqual(DateTimeKind.Utc, result.DateOfTransaction.Kind);
        Assert.IsTrue(result.DateOfTransaction >= beforeCall.AddSeconds(-1) && result.DateOfTransaction <= afterCall.AddSeconds(1));
    }

    [TestMethod]
    public async Task CreateTransactionAsync_ExplicitDate_PreservesDateInUtc()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var account = await accountRepo.CreateAccountAsync(new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m });
        var category = await categoryRepo.CreateCategoryAsync(new Category { Name = "Salary", Type = CategoryType.Income });

        var explicitDate = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Unspecified);
        var request = new CreateTransactionRequestDto(account.Id, category.Id, "Pay", 500m, explicitDate);
        var result = await service.CreateTransactionAsync(request);

        Assert.IsNotNull(result);
        Assert.AreEqual(DateTimeKind.Utc, result.DateOfTransaction.Kind);
        Assert.AreEqual(new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc), result.DateOfTransaction);
    }
}
