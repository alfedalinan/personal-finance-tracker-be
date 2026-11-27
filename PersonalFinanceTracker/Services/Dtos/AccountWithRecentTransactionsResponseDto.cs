namespace PersonalFinanceTracker.Services.Dtos;

public record AccountWithRecentTransactionsResponseDto(
    int Id,
    string Name,
    string Type,
    decimal CurrentBalance,
    string Slug,
    string TenantId,
    IEnumerable<TransactionSummaryDto> Transactions
);
