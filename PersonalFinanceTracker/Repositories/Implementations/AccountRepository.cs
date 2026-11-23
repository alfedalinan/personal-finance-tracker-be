using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Interfaces;

namespace PersonalFinanceTracker.Repositories.Implementations;

public class AccountRepository : IAccountRepository
{
    private readonly FinanceTrackerDbContext _context;

    public AccountRepository(FinanceTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetAccountByIdAsync(int id)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetAccountBySlugAsync(string slug)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Slug.ToLower() == slug.ToLower());
    }

    public async Task<Account?> GetAccountByTypeAsync(string type)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Type.ToLower() == type.ToLower() || a.Slug.ToLower() == type.ToLower());
    }

    public async Task<IEnumerable<Account>> GetAllAccountsAsync()
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetAllAccountsWithRecentTransactionsAsync()
    {
        var accounts = await _context.Accounts
            .Include(a => a.Transactions)
                .ThenInclude(t => t.Category)
            .ToListAsync();

        foreach (var account in accounts)
        {
            account.Transactions = account.Transactions
                .OrderByDescending(t => t.DateOfTransaction)
                .Take(5)
                .ToList();
        }

        return accounts;
    }

    public async Task<Account> CreateAccountAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAccountAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }
}
