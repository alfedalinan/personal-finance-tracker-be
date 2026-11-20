using PersonalFinanceTracker.Entities;

namespace PersonalFinanceTracker.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category> CreateCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
}
