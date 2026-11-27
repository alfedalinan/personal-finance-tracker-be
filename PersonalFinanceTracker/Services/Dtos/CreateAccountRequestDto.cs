namespace PersonalFinanceTracker.Services.Dtos;

public record CreateAccountRequestDto(
    string Name,
    string Type,
    decimal CurrentBalance = 0m,
    string? Slug = null,
    string? TenantId = null
);
