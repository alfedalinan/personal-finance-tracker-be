using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Services.Dtos;

namespace PersonalFinanceTracker.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionSummaryDto> CreateTransactionAsync(CreateTransactionRequestDto request);
    Task<AccountTransactionsResponseDto?> GetAccountTransactionsAndBalanceAsync(int accountId);
}
