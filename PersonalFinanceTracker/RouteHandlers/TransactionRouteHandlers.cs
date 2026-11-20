using Microsoft.AspNetCore.Http;
using PersonalFinanceTracker.Services.Dtos;
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

    public static async Task<IResult> CreateTransaction(CreateTransactionRequestDto request, ITransactionService transactionService)
    {
        try
        {
            var createdTransaction = await transactionService.CreateTransactionAsync(request);
            return Results.Created($"/api/transactions/{createdTransaction.Id}", createdTransaction);
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }
}
