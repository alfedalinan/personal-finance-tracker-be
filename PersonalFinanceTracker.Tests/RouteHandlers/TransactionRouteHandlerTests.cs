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
public class TransactionRouteHandlerTests
{
    private Mock<ITransactionService> _mockTransactionService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockTransactionService = new Mock<ITransactionService>();
    }

    [TestMethod]
    public async Task CreateTransaction_ShouldReturnCreated_WhenSuccessful()
    {
        // Arrange
        var request = new CreateTransactionRequestDto(1, 1, "Salary", 5000m);
        var created = new Transaction { Id = 1, AccountId = 1, CategoryId = 1, Amount = 5000m, Description = "Salary" };
        _mockTransactionService.Setup(s => s.CreateTransactionAsync(request)).ReturnsAsync(created);

        // Act
        var result = await TransactionRouteHandlers.CreateTransaction(request, _mockTransactionService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        httpContext.RequestServices = services;
        
        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status201Created, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task CreateTransaction_ShouldReturnNotFound_WhenArgumentExceptionThrown()
    {
        // Arrange
        var request = new CreateTransactionRequestDto(99, 1, "Salary", 5000m);
        _mockTransactionService.Setup(s => s.CreateTransactionAsync(request))
            .ThrowsAsync(new ArgumentException("Account with ID 99 not found."));

        // Act
        var result = await TransactionRouteHandlers.CreateTransaction(request, _mockTransactionService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        httpContext.RequestServices = services;

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
    }
}
