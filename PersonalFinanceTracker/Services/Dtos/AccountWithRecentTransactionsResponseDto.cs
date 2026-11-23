namespace PersonalFinanceTracker.Services.Dtos;

public record AccountWithRecentTransactionsResponseDto(
    int Id,
    string Name,
    string Type,
    decimal CurrentBalance,
    string Slug,
    IEnumerable<TransactionSummaryDto> Transactions
);
