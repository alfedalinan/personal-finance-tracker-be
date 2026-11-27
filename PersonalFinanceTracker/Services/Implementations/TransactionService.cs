using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Interfaces;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.Services.Implementations;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly FinanceTrackerDbContext _context;
    private readonly ITenantContext _tenantContext;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        FinanceTrackerDbContext context,
        ITenantContext? tenantContext = null)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _context = context;
        _tenantContext = tenantContext ?? new TenantContext { TenantId = "11111111-1111-1111-1111-111111111111" };
    }

    private async Task<Category> GetAndValidateCategoryAsync(int categoryId)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            throw new ArgumentException($"Category with ID {categoryId} not found.");
        }
        return category;
    }

    private async Task<Account> GetAndValidateAccountAsync(int accountId)
    {
        var account = await _accountRepository.GetAccountByIdAsync(accountId);
        if (account == null || (!string.IsNullOrEmpty(account.TenantId) && !string.Equals(account.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Account with ID {accountId} not found.");
        }
        return account;
    }

    private void ApplyTransactionEffect(Account account, CategoryType categoryType, decimal amount)
    {
        var magnitude = Math.Abs(amount);
        if (categoryType == CategoryType.Income)
        {
            account.CurrentBalance += magnitude;
        }
        else
        {
            account.CurrentBalance -= magnitude;
        }
    }

    private void ReverseTransactionEffect(Account account, CategoryType categoryType, decimal amount)
    {
        var magnitude = Math.Abs(amount);
        if (categoryType == CategoryType.Income)
        {
            account.CurrentBalance -= magnitude;
        }
        else
        {
            account.CurrentBalance += magnitude;
        }
    }

    public async Task<TransactionSummaryDto> CreateTransactionAsync(CreateTransactionRequestDto request)
    {
        var account = await GetAndValidateAccountAsync(request.AccountId);
        var category = await GetAndValidateCategoryAsync(request.CategoryId);

        using var dbTransaction = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;

        try
        {
            var beforeBalance = account.CurrentBalance;
            ApplyTransactionEffect(account, category.Type, request.Amount);
            var afterBalance = account.CurrentBalance;

            await _accountRepository.UpdateAccountAsync(account);

            var transactionDate = request.DateOfTransaction ?? DateTime.UtcNow;
            if (transactionDate.Kind == DateTimeKind.Unspecified)
            {
                transactionDate = DateTime.SpecifyKind(transactionDate, DateTimeKind.Utc);
            }
            else
            {
                transactionDate = transactionDate.ToUniversalTime();
            }

            var transaction = new Transaction
            {
                AccountId = request.AccountId,
                CategoryId = request.CategoryId,
                Description = request.Description,
                Amount = request.Amount,
                BeforeBalance = beforeBalance,
                AfterBalance = afterBalance,
                DateOfTransaction = transactionDate
            };

            var createdTransaction = await _transactionRepository.CreateTransactionAsync(transaction);

            if (dbTransaction != null)
            {
                await dbTransaction.CommitAsync();
            }

            return new TransactionSummaryDto(
                createdTransaction.Id,
                createdTransaction.DateOfTransaction,
                createdTransaction.Amount,
                createdTransaction.BeforeBalance,
                createdTransaction.AfterBalance,
                createdTransaction.Description,
                category.Name,
                category.Type.ToString()
            );
        }
        catch
        {
            if (dbTransaction != null)
            {
                await dbTransaction.RollbackAsync();
            }
            throw;
        }
    }

    public async Task<TransactionSummaryDto> UpdateTransactionAsync(int id, CreateTransactionRequestDto request)
    {
        var existingTransaction = await _transactionRepository.GetTransactionByIdAsync(id);
        if (existingTransaction == null)
        {
            throw new ArgumentException($"Transaction with ID {id} not found.");
        }

        throw new InvalidOperationException("Direct updates on existing ledger transactions are disabled to preserve historical audit integrity. Please post a compensating/reversing transaction instead.");
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _transactionRepository.GetTransactionByIdAsync(id);
        if (transaction == null)
        {
            throw new ArgumentException($"Transaction with ID {id} not found.");
        }

        throw new InvalidOperationException("Direct deletions on existing ledger transactions are disabled to preserve historical audit integrity. Please post a compensating/reversing transaction instead.");
    }

    private (DateTime Start, DateTime End) ParseAndValidateDateRange(string? startDateStr, string? endDateStr)
    {
        if (string.IsNullOrWhiteSpace(startDateStr))
        {
            throw new ArgumentException("startDate is required.");
        }

        DateTime startDate;
        if (DateTime.TryParseExact(startDateStr.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnlyStart))
        {
            startDate = new DateTime(dateOnlyStart.Year, dateOnlyStart.Month, dateOnlyStart.Day, 0, 0, 0, 0, DateTimeKind.Utc);
        }
        else if (DateTime.TryParse(startDateStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsedStart))
        {
            startDate = parsedStart.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(parsedStart, DateTimeKind.Utc)
                : parsedStart.ToUniversalTime();
        }
        else
        {
            throw new ArgumentException("Invalid startDate format.");
        }

        DateTime endDate;
        if (string.IsNullOrWhiteSpace(endDateStr))
        {
            var nowUtc = DateTime.UtcNow;
            endDate = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 23, 59, 59, 999, DateTimeKind.Utc);
        }
        else if (DateTime.TryParseExact(endDateStr.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnlyEnd))
        {
            endDate = new DateTime(dateOnlyEnd.Year, dateOnlyEnd.Month, dateOnlyEnd.Day, 23, 59, 59, 999, DateTimeKind.Utc);
        }
        else if (DateTime.TryParse(endDateStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsedEnd))
        {
            endDate = parsedEnd.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(parsedEnd, DateTimeKind.Utc)
                : parsedEnd.ToUniversalTime();
        }
        else
        {
            throw new ArgumentException("Invalid endDate format.");
        }

        var utcNow = DateTime.UtcNow;
        if (endDate > utcNow)
        {
            endDate = utcNow;
        }

        if (startDate > endDate)
        {
            throw new ArgumentException("startDate cannot be later than endDate.");
        }

        return (startDate, endDate);
    }

    public async Task<AccountTransactionsResponseDto?> GetAccountTransactionsAndBalanceAsync(int accountId, string? startDate, string? endDate)
    {
        var account = await _accountRepository.GetAccountByIdAsync(accountId);
        if (account == null || (!string.IsNullOrEmpty(account.TenantId) && !string.Equals(account.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var (parsedStart, parsedEnd) = ParseAndValidateDateRange(startDate, endDate);

        var transactions = await _transactionRepository.GetTransactionsByAccountIdAsync(accountId, parsedStart, parsedEnd);
        var remainingBalance = account.CurrentBalance;

        var transactionDtos = transactions.Select(t => new TransactionSummaryDto(
            t.Id,
            t.DateOfTransaction,
            t.Amount,
            t.BeforeBalance,
            t.AfterBalance,
            t.Description,
            t.Category?.Name ?? string.Empty,
            t.Category?.Type.ToString() ?? string.Empty
        ));

        return new AccountTransactionsResponseDto(
            account.Id,
            account.Name,
            account.Type,
            account.CurrentBalance,
            remainingBalance,
            transactionDtos
        );
    }
}
