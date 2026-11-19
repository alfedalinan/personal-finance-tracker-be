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

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Transaction> CreateTransactionAsync(CreateTransactionRequestDto request)
    {
        var account = await _accountRepository.GetAccountByIdAsync(request.AccountId);
        if (account == null)
        {
            throw new ArgumentException($"Account with ID {request.AccountId} not found.");
        }

        var category = await _categoryRepository.GetCategoryByIdAsync(request.CategoryId);
        if (category == null)
        {
            throw new ArgumentException($"Category with ID {request.CategoryId} not found.");
        }

        var transaction = new Transaction
        {
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            Description = request.Description,
            Amount = request.Amount,
            DateOfTransaction = request.DateOfTransaction ?? DateTime.UtcNow
        };

        return await _transactionRepository.CreateTransactionAsync(transaction);
    }

    public async Task<AccountTransactionsResponseDto?> GetAccountTransactionsAndBalanceAsync(int accountId)
    {
        var account = await _accountRepository.GetAccountByIdAsync(accountId);
        if (account == null)
        {
            return null;
        }

        var transactions = await _transactionRepository.GetTransactionsByAccountIdAsync(accountId);
        var transactionSum = transactions.Sum(t => t.Amount);
        
        // Remaining balance dynamically computed: account.CurrentBalance + sum of transactions (or assuming CurrentBalance is baseline/starting balance)
        // Let's compute remaining balance as account.CurrentBalance + transactionSum (or account.CurrentBalance if CurrentBalance is current total, but standard finance trackers: RemainingBalance = CurrentBalance + sum of transactions, or CurrentBalance is starting balance + sum). Let's check how accounts are initialized or used.
        // Let's check DbInitializer.cs or account repositories to see what CurrentBalance means.
        var remainingBalance = account.CurrentBalance + transactionSum;

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
