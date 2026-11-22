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

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        FinanceTrackerDbContext context)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _context = context;
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
        if (account == null)
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
            ? await _context.Database.BeginTransactionAsync()
            : null;

        try
        {
            ApplyTransactionEffect(account, category.Type, request.Amount);
            await _accountRepository.UpdateAccountAsync(account);

            var transaction = new Transaction
            {
                AccountId = request.AccountId,
                CategoryId = request.CategoryId,
                Description = request.Description,
                Amount = request.Amount,
                DateOfTransaction = request.DateOfTransaction ?? DateTime.UtcNow
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

        var oldAccount = await GetAndValidateAccountAsync(existingTransaction.AccountId);
        var oldCategory = await GetAndValidateCategoryAsync(existingTransaction.CategoryId);

        var newAccount = request.AccountId == existingTransaction.AccountId
            ? oldAccount
            : await GetAndValidateAccountAsync(request.AccountId);

        var newCategory = request.CategoryId == existingTransaction.CategoryId
            ? oldCategory
            : await GetAndValidateCategoryAsync(request.CategoryId);

        using var dbTransaction = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync()
            : null;

        try
        {
            ReverseTransactionEffect(oldAccount, oldCategory.Type, existingTransaction.Amount);
            if (oldAccount.Id != newAccount.Id)
            {
                await _accountRepository.UpdateAccountAsync(oldAccount);
            }

            ApplyTransactionEffect(newAccount, newCategory.Type, request.Amount);
            if (oldAccount.Id != newAccount.Id)
            {
                await _accountRepository.UpdateAccountAsync(newAccount);
            }
            else
            {
                await _accountRepository.UpdateAccountAsync(oldAccount);
            }

            existingTransaction.AccountId = request.AccountId;
            existingTransaction.CategoryId = request.CategoryId;
            existingTransaction.Description = request.Description;
            existingTransaction.Amount = request.Amount;
            if (request.DateOfTransaction.HasValue)
            {
                existingTransaction.DateOfTransaction = request.DateOfTransaction.Value;
            }

            await _transactionRepository.UpdateTransactionAsync(existingTransaction);

            if (dbTransaction != null)
            {
                await dbTransaction.CommitAsync();
            }

            return new TransactionSummaryDto(
                existingTransaction.Id,
                existingTransaction.DateOfTransaction,
                existingTransaction.Amount,
                existingTransaction.Description,
                newCategory.Name,
                newCategory.Type.ToString()
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

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _transactionRepository.GetTransactionByIdAsync(id);
        if (transaction == null)
        {
            throw new ArgumentException($"Transaction with ID {id} not found.");
        }

        var account = await GetAndValidateAccountAsync(transaction.AccountId);
        var category = await GetAndValidateCategoryAsync(transaction.CategoryId);

        using var dbTransaction = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync()
            : null;

        try
        {
            ReverseTransactionEffect(account, category.Type, transaction.Amount);
            await _accountRepository.UpdateAccountAsync(account);

            await _transactionRepository.DeleteTransactionAsync(id);

            if (dbTransaction != null)
            {
                await dbTransaction.CommitAsync();
            }
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

    public async Task<AccountTransactionsResponseDto?> GetAccountTransactionsAndBalanceAsync(int accountId)
    {
        var account = await _accountRepository.GetAccountByIdAsync(accountId);
        if (account == null)
        {
            return null;
        }

        var transactions = await _transactionRepository.GetTransactionsByAccountIdAsync(accountId);
        var remainingBalance = account.CurrentBalance;

        var transactionDtos = transactions.Select(t => new TransactionSummaryDto(
            t.Id,
            t.DateOfTransaction,
            t.Amount,
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
