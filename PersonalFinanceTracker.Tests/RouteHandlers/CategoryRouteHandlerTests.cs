using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.RouteHandlers;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.Tests.RouteHandlers;

[TestClass]
public class CategoryRouteHandlerTests
{
    private Mock<ICategoryService> _mockCategoryService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockCategoryService = new Mock<ICategoryService>();
    }

    [TestMethod]
    public async Task GetAllCategories_ShouldReturnOkWithCategories()
    {
        // Arrange
        var categories = new List<CategoryResponseDto>
        {
            new CategoryResponseDto(1, "Salary", "Income")
        };
        _mockCategoryService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await CategoryRouteHandlers.GetAllCategories(_mockCategoryService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task CreateCategory_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateCategoryRequestDto("Utilities", CategoryType.Expense);
        var created = new CategoryResponseDto(1, "Utilities", "Expense");
        _mockCategoryService.Setup(s => s.CreateCategoryAsync(request)).ReturnsAsync(created);

        // Act
        var result = await CategoryRouteHandlers.CreateCategory(request, _mockCategoryService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status201Created, httpContext.Response.StatusCode);
    }
}
