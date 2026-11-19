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
}
