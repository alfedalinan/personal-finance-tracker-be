using Microsoft.AspNetCore.Http;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.RouteHandlers;

public static class CategoryRouteHandlers
{
    public static async Task<IResult> GetAllCategories(ICategoryService categoryService)
    {
        var categories = await categoryService.GetAllCategoriesAsync();
        return Results.Ok(categories);
    }

    public static async Task<IResult> CreateCategory(CreateCategoryRequestDto request, ICategoryService categoryService)
    {
        var createdCategory = await categoryService.CreateCategoryAsync(request);
        return Results.Created($"/api/categories/{createdCategory.Id}", createdCategory);
    }
}
