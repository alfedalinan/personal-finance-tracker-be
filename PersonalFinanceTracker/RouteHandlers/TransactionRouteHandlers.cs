using Microsoft.AspNetCore.Http;
using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.RouteHandlers;

public static class TransactionRouteHandlers
{
    public static async Task<IResult> GetAccountTransactionsAndBalance(int accountId, ITransactionService transactionService)
    {
        var result = await transactionService.GetAccountTransactionsAndBalanceAsync(accountId);
        if (result == null)
        {
            return Results.NotFound(new { message = $"Account with ID {accountId} not found." });
        }
        return Results.Ok(result);
    }
}
