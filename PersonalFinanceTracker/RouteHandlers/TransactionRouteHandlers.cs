using Microsoft.AspNetCore.Http;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.RouteHandlers;

public static class TransactionRouteHandlers
{
    public static async Task<IResult> GetAccountTransactionsAndBalance(int accountId, string? startDate, string? endDate, ITransactionService transactionService)
    {
        try
        {
            var result = await transactionService.GetAccountTransactionsAndBalanceAsync(accountId, startDate, endDate);
            if (result == null)
            {
                return Results.NotFound(new { message = $"Account with ID {accountId} not found." });
            }
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
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

    public static async Task<IResult> UpdateTransaction(int id, CreateTransactionRequestDto request, ITransactionService transactionService)
    {
        try
        {
            var updatedTransaction = await transactionService.UpdateTransactionAsync(id, request);
            return Results.Ok(updatedTransaction);
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }

    public static async Task<IResult> DeleteTransaction(int id, ITransactionService transactionService)
    {
        try
        {
            await transactionService.DeleteTransactionAsync(id);
            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }
}
