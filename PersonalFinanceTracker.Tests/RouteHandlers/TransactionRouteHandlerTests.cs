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
    public async Task GetAccountTransactionsAndBalance_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var responseDto = new AccountTransactionsResponseDto(1, "Checking", "Bank", 1000m, 1000m, new List<TransactionSummaryDto>());
        _mockTransactionService.Setup(s => s.GetAccountTransactionsAndBalanceAsync(1, "2026-11-01", "2026-11-15"))
            .ReturnsAsync(responseDto);

        // Act
        var result = await TransactionRouteHandlers.GetAccountTransactionsAndBalance(1, "2026-11-01", "2026-11-15", _mockTransactionService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        httpContext.RequestServices = services;

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GetAccountTransactionsAndBalance_ShouldReturnNotFound_WhenAccountMissing()
    {
        // Arrange
        _mockTransactionService.Setup(s => s.GetAccountTransactionsAndBalanceAsync(99, "2026-11-01", null))
            .ReturnsAsync((AccountTransactionsResponseDto?)null);

        // Act
        var result = await TransactionRouteHandlers.GetAccountTransactionsAndBalance(99, "2026-11-01", null, _mockTransactionService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        httpContext.RequestServices = services;

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GetAccountTransactionsAndBalance_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
    {
        // Arrange
        _mockTransactionService.Setup(s => s.GetAccountTransactionsAndBalanceAsync(1, null, null))
            .ThrowsAsync(new ArgumentException("startDate is required."));

        // Act
        var result = await TransactionRouteHandlers.GetAccountTransactionsAndBalance(1, null, null, _mockTransactionService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        httpContext.RequestServices = services;

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task CreateTransaction_ShouldReturnCreated_WhenSuccessful()
    {
        // Arrange
        var request = new CreateTransactionRequestDto(1, 1, "Salary", 5000m);
        var createdDto = new TransactionSummaryDto(1, DateTime.UtcNow, 5000m, 0m, 5000m, "Salary", "Income", "Income");
        _mockTransactionService.Setup(s => s.CreateTransactionAsync(request)).ReturnsAsync(createdDto);

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
    public async Task CreateTransaction_ShouldReturnCreated_WhenAmountIsNegative()
    {
        // Arrange
        var request = new CreateTransactionRequestDto(1, 1, "Groceries", -150.00m);
        var createdDto = new TransactionSummaryDto(1, DateTime.UtcNow, -150.00m, 1000m, 850m, "Groceries", "Food", "Expense");
        _mockTransactionService.Setup(s => s.CreateTransactionAsync(request)).ReturnsAsync(createdDto);

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

    [TestMethod]
    public void CreateTransactionRequestDto_Deserializes_OmittedDateToNull()
    {
        var json = "{\"accountId\":1,\"categoryId\":1,\"description\":\"Test\",\"amount\":100}";
        var dto = System.Text.Json.JsonSerializer.Deserialize<CreateTransactionRequestDto>(json);
        Assert.IsNotNull(dto);
        Assert.IsNull(dto.DateOfTransaction);
    }

    [TestMethod]
    public void CreateTransactionRequestDto_Deserializes_NullDateToNull()
    {
        var json = "{\"accountId\":1,\"categoryId\":1,\"description\":\"Test\",\"amount\":100,\"date\":null}";
        var dto = System.Text.Json.JsonSerializer.Deserialize<CreateTransactionRequestDto>(json);
        Assert.IsNotNull(dto);
        Assert.IsNull(dto.DateOfTransaction);
    }

    [TestMethod]
    public void CreateTransactionRequestDto_Deserializes_EmptyStringDateToNull()
    {
        var json = "{\"accountId\":1,\"categoryId\":1,\"description\":\"Test\",\"amount\":100,\"date\":\"\"}";
        var dto = System.Text.Json.JsonSerializer.Deserialize<CreateTransactionRequestDto>(json);
        Assert.IsNotNull(dto);
        Assert.IsNull(dto.DateOfTransaction);
    }

    [TestMethod]
    public void CreateTransactionRequestDto_Deserializes_ExplicitDateString()
    {
        var json = "{\"accountId\":1,\"categoryId\":1,\"description\":\"Test\",\"amount\":100,\"date\":\"2025-06-15T14:30:00Z\"}";
        var dto = System.Text.Json.JsonSerializer.Deserialize<CreateTransactionRequestDto>(json);
        Assert.IsNotNull(dto);
        Assert.IsNotNull(dto.DateOfTransaction);
        Assert.AreEqual(DateTimeKind.Utc, dto.DateOfTransaction.Value.Kind);
        Assert.AreEqual(new DateTime(2025, 6, 15, 14, 30, 0, DateTimeKind.Utc), dto.DateOfTransaction.Value);
    }
}
