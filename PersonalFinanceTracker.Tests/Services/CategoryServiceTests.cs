using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Interfaces;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Implementations;

namespace PersonalFinanceTracker.Tests.Services;

[TestClass]
public class CategoryServiceTests
{
    private Mock<ICategoryRepository> _mockCategoryRepo = null!;
    private CategoryService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _service = new CategoryService(_mockCategoryRepo.Object);
    }

    [TestMethod]
    public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Salary", Type = CategoryType.Income },
            new Category { Id = 2, Name = "Rent", Type = CategoryType.Expense }
        };
        _mockCategoryRepo.Setup(r => r.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _service.GetAllCategoriesAsync();

        // Assert
        Assert.IsNotNull(result);
        var list = result.ToList();
        Assert.AreEqual(2, list.Count);
        Assert.AreEqual("Salary", list[0].Name);
        Assert.AreEqual("Income", list[0].Type);
    }

    [TestMethod]
    public async Task CreateCategoryAsync_ShouldCreateAndReturnCategory()
    {
        // Arrange
        var request = new CreateCategoryRequestDto("Groceries", CategoryType.Expense);
        _mockCategoryRepo.Setup(r => r.CreateCategoryAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => { c.Id = 5; return c; });

        // Act
        var result = await _service.CreateCategoryAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.Id);
        Assert.AreEqual("Groceries", result.Name);
        Assert.AreEqual("Expense", result.Type);
    }
}
