using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Implementations;

namespace PersonalFinanceTracker.Tests.Repositories;

[TestClass]
public class CategoryRepositoryTests
{
    private FinanceTrackerDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new FinanceTrackerDbContext(options);
    }

    [TestMethod]
    public async Task GetCategoryByIdAsync_ReturnsCategory_WhenExists()
    {
        using var context = GetInMemoryDbContext();
        var category = new Category { Name = "Food", Type = CategoryType.Expense };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var repo = new CategoryRepository(context);
        var result = await repo.GetCategoryByIdAsync(category.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual("Food", result.Name);
        Assert.AreEqual(CategoryType.Expense, result.Type);
    }

    [TestMethod]
    public async Task GetCategoryByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = GetInMemoryDbContext();
        var repo = new CategoryRepository(context);
        var result = await repo.GetCategoryByIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllCategoriesAsync_ReturnsAllCategories()
    {
        using var context = GetInMemoryDbContext();
        context.Categories.Add(new Category { Name = "Food", Type = CategoryType.Expense });
        context.Categories.Add(new Category { Name = "Salary", Type = CategoryType.Income });
        await context.SaveChangesAsync();

        var repo = new CategoryRepository(context);
        var categories = (await repo.GetAllCategoriesAsync()).ToList();

        Assert.AreEqual(2, categories.Count);
    }

    [TestMethod]
    public async Task CreateCategoryAsync_AddsCategory()
    {
        using var context = GetInMemoryDbContext();
        var repo = new CategoryRepository(context);
        var category = new Category { Name = "Transport", Type = CategoryType.Expense };

        var created = await repo.CreateCategoryAsync(category);

        Assert.IsTrue(created.Id > 0);
        var inDb = await context.Categories.FindAsync(created.Id);
        Assert.IsNotNull(inDb);
        Assert.AreEqual("Transport", inDb.Name);
    }

    [TestMethod]
    public async Task UpdateCategoryAsync_UpdatesCategory()
    {
        using var context = GetInMemoryDbContext();
        var category = new Category { Name = "Food", Type = CategoryType.Expense };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var repo = new CategoryRepository(context);
        category.Name = "Groceries";
        await repo.UpdateCategoryAsync(category);

        var updated = await context.Categories.FindAsync(category.Id);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Groceries", updated.Name);
    }

    [TestMethod]
    public async Task DeleteCategoryAsync_RemovesCategory()
    {
        using var context = GetInMemoryDbContext();
        var category = new Category { Name = "Food", Type = CategoryType.Expense };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var repo = new CategoryRepository(context);
        await repo.DeleteCategoryAsync(category.Id);

        var deleted = await context.Categories.FindAsync(category.Id);
        Assert.IsNull(deleted);
    }
}
