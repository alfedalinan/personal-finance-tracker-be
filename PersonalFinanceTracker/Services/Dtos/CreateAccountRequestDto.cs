namespace PersonalFinanceTracker.Services.Dtos;

public record CreateAccountRequestDto(
    string Name,
    string Type,
    decimal CurrentBalance = 0m
);
