using PersonalFinanceTracker.Services.Dtos;

namespace PersonalFinanceTracker.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
    Task<IEnumerable<CategoryTypeDropdownDto>> GetCategoryTypeDropdownOptionsAsync();
    Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryRequestDto request);
}
