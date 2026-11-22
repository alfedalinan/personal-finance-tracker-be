namespace PersonalFinanceTracker.Services.Dtos;

public record TransactionSummaryDto(
    int Id,
    DateTime DateOfTransaction,
    decimal Amount,
    decimal BeforeBalance,
    decimal AfterBalance,
    string Description,
    string CategoryName,
    string CategoryType
);
