namespace PersonalFinanceTracker.Services.Dtos;

public record AccountTransactionsResponseDto(
    int AccountId,
    string AccountName,
    string AccountType,
    decimal CurrentBalance,
    decimal RemainingBalance,
    IEnumerable<TransactionSummaryDto> Transactions
);
