using PersonalFinanceTracker.Entities;

namespace PersonalFinanceTracker.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetTransactionByIdAsync(int id);
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
    Task<Transaction> CreateTransactionAsync(Transaction transaction);
    Task UpdateTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(int id);
}
