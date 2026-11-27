using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Implementations;
using PersonalFinanceTracker.Services.Implementations;

namespace PersonalFinanceTracker.Tests.Services;

[TestClass]
public class TenantIsolationTests
{
    private FinanceTrackerDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new FinanceTrackerDbContext(options);
    }

    [TestMethod]
    public async Task GetAccountTransactionsAndBalanceAsync_ReturnsNull_WhenTenantMismatch()
    {
        using var context = GetInMemoryDbContext();
        var tenantContextA = new TenantContext { TenantId = "tenant-a-guid" };
        var accountRepoA = new AccountRepository(context, tenantContextA);
        var transactionRepo = new TransactionRepository(context);
        var categoryRepo = new CategoryRepository(context);

        // Account belongs to Tenant A
        var account = await accountRepoA.CreateAccountAsync(new Account
        {
            Name = "Tenant A Account",
            Type = "Bank",
            TenantId = "tenant-a-guid",
            CurrentBalance = 1000m
        });

        // Request executed with Tenant B context
        var tenantContextB = new TenantContext { TenantId = "tenant-b-guid" };
        var accountRepoB = new AccountRepository(context, tenantContextB);
        var service = new TransactionService(transactionRepo, accountRepoB, categoryRepo, context, tenantContextB);

        var result = await service.GetAccountTransactionsAndBalanceAsync(account.Id, "2026-01-01", null);

        Assert.IsNull(result, "Cross-tenant access should return null (resulting in 404 Not Found)");
    }

    [TestMethod]
    public async Task GetAccountTransactionsAndBalanceAsync_ReturnsAccount_WhenTenantMatches()
    {
        using var context = GetInMemoryDbContext();
        var tenantContextA = new TenantContext { TenantId = "tenant-a-guid" };
        var accountRepo = new AccountRepository(context, tenantContextA);
        var transactionRepo = new TransactionRepository(context);
        var categoryRepo = new CategoryRepository(context);

        // Account belongs to Tenant A
        var account = await accountRepo.CreateAccountAsync(new Account
        {
            Name = "Tenant A Account",
            Type = "Bank",
            TenantId = "tenant-a-guid",
            CurrentBalance = 1000m
        });

        // Request executed with Tenant A context
        var service = new TransactionService(transactionRepo, accountRepo, categoryRepo, context, tenantContextA);

        var result = await service.GetAccountTransactionsAndBalanceAsync(account.Id, "2026-01-01", null);

        Assert.IsNotNull(result, "Matching tenant should successfully retrieve account transactions and balance");
        Assert.AreEqual(account.Id, result.AccountId);
    }
}
