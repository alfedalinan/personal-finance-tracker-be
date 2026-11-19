namespace PersonalFinanceTracker.Services.Dtos;

public record CreateTransactionRequestDto(
    int AccountId,
    int CategoryId,
    string Description,
    decimal Amount,
    DateTime? DateOfTransaction = null
);
