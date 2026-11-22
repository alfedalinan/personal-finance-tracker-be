using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Interfaces;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllCategoriesAsync();
        return categories.Select(c => new CategoryResponseDto(
            c.Id,
            c.Name,
            c.Type.ToString()
        ));
    }

    public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryRequestDto request)
    {
        var category = new Category
        {
            Name = request.Name,
            Type = request.Type
        };

        var createdCategory = await _categoryRepository.CreateCategoryAsync(category);

        return new CategoryResponseDto(
            createdCategory.Id,
            createdCategory.Name,
            createdCategory.Type.ToString()
        );
    }
}
