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
    public async Task UpdateTransactionAsync_ReversesOldAndAppliesNew_UpdatesBalance()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var account = await accountRepo.CreateAccountAsync(new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m });
        var expenseCategory = await categoryRepo.CreateCategoryAsync(new Category { Name = "Food", Type = CategoryType.Expense });
        var incomeCategory = await categoryRepo.CreateCategoryAsync(new Category { Name = "Bonus", Type = CategoryType.Income });

        var created = await service.CreateTransactionAsync(new CreateTransactionRequestDto(account.Id, expenseCategory.Id, "Lunch", 100.00m));
        // Balance should be 900m

        var updateRequest = new CreateTransactionRequestDto(account.Id, incomeCategory.Id, "Corrected Bonus", 300.00m);
        var updated = await service.UpdateTransactionAsync(created.Id, updateRequest);

        Assert.IsNotNull(updated);
        var updatedAccount = await accountRepo.GetAccountByIdAsync(account.Id);
        // Old expense 100 reversed -> 1000, new income 300 applied -> 1300
        Assert.AreEqual(1300.00m, updatedAccount!.CurrentBalance);
    }

    [TestMethod]
    public async Task DeleteTransactionAsync_ReversesTransactionEffect_UpdatesBalance()
    {
        using var context = GetInMemoryDbContext();
        var accountRepo = new AccountRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var transactionRepo = new TransactionRepository(context);
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context);

        var account = await accountRepo.CreateAccountAsync(new Account { Name = "Checking", Type = "Bank", CurrentBalance = 1000m });
        var category = await categoryRepo.CreateCategoryAsync(new Category { Name = "Utilities", Type = CategoryType.Expense });

        var created = await service.CreateTransactionAsync(new CreateTransactionRequestDto(account.Id, category.Id, "Power Bill", 150.00m));
        // Balance should be 850m

        await service.DeleteTransactionAsync(created.Id);

        var updatedAccount = await accountRepo.GetAccountByIdAsync(account.Id);
        // Balance should return to 1000m
        Assert.AreEqual(1000.00m, updatedAccount!.CurrentBalance);
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
}
