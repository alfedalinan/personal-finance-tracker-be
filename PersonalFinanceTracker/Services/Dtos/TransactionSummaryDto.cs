namespace PersonalFinanceTracker.Services.Dtos;

public record TransactionSummaryDto(
    int Id,
    DateTime DateOfTransaction,
    decimal Amount,
    string Description,
    string CategoryName,
    string CategoryType
);
