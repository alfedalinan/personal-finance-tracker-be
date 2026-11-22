using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Interfaces;

namespace PersonalFinanceTracker.Repositories.Implementations;

public class TransactionRepository : ITransactionRepository
{
    private readonly FinanceTrackerDbContext _context;

    public TransactionRepository(FinanceTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId, DateTime startDate, DateTime endDate)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId && t.DateOfTransaction >= startDate && t.DateOfTransaction <= endDate)
            .Include(t => t.Category)
            .Include(t => t.Account)
            .OrderByDescending(t => t.DateOfTransaction)
            .ToListAsync();
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }
}
