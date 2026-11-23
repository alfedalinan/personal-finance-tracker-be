using Microsoft.AspNetCore.Http;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.RouteHandlers;

public static class AccountRouteHandlers
{
    public static async Task<IResult> GetAllAccounts(IAccountService accountService)
    {
        var accounts = await accountService.GetAllAccountsAsync();
        return Results.Ok(accounts);
    }

    public static async Task<IResult> GetAccountDropdownList(IAccountService accountService)
    {
        var accounts = await accountService.GetAccountDropdownOptionsAsync();
        return Results.Ok(accounts);
    }

    public static async Task<IResult> GetAccountsWithRecentTransactions(IAccountService accountService)
    {
        var accounts = await accountService.GetAllAccountsWithRecentTransactionsAsync();
        return Results.Ok(accounts);
    }

    public static async Task<IResult> GetAccountById(int id, IAccountService accountService)
    {
        var account = await accountService.GetAccountByIdAsync(id);
        if (account == null)
        {
            return Results.NotFound(new { message = $"Account with ID {id} not found." });
        }
        return Results.Ok(account);
    }

    public static async Task<IResult> GetAccountBySlug(string slug, IAccountService accountService)
    {
        var account = await accountService.GetAccountBySlugAsync(slug);
        if (account == null)
        {
            return Results.NotFound(new { message = $"Account with slug '{slug}' not found." });
        }
        return Results.Ok(account);
    }

    public static async Task<IResult> GetAccountByType(string type, IAccountService accountService)
    {
        var account = await accountService.GetAccountByTypeAsync(type);
        if (account == null)
        {
            return Results.NotFound(new { message = $"Account with type/slug '{type}' not found." });
        }
        return Results.Ok(account);
    }

    public static async Task<IResult> CreateAccount(CreateAccountRequestDto request, IAccountService accountService)
    {
        var createdAccount = await accountService.CreateAccountAsync(request);
        return Results.Created($"/api/accounts/{createdAccount.Id}", createdAccount);
    }
}
