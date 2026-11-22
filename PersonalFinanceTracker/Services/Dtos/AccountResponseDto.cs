namespace PersonalFinanceTracker.Services.Dtos;

public record AccountResponseDto(
    int Id,
    string Name,
    string Type,
    decimal CurrentBalance
);
