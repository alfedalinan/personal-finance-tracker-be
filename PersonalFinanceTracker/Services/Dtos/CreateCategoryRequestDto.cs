using PersonalFinanceTracker.Entities;

namespace PersonalFinanceTracker.Services.Dtos;

public record CreateCategoryRequestDto(
    string Name,
    CategoryType Type
);
