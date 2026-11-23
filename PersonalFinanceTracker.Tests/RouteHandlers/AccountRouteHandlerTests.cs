using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonalFinanceTracker.RouteHandlers;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.Tests.RouteHandlers;

[TestClass]
public class AccountRouteHandlerTests
{
    private Mock<IAccountService> _mockAccountService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockAccountService = new Mock<IAccountService>();
    }

    [TestMethod]
    public async Task GetAllAccounts_ShouldReturnOkWithAccounts()
    {
        // Arrange
        var accounts = new List<AccountResponseDto>
        {
            new AccountResponseDto(1, "Checking", "Bank", 500m, "checking")
        };
        _mockAccountService.Setup(s => s.GetAllAccountsAsync()).ReturnsAsync(accounts);

        // Act
        var result = await AccountRouteHandlers.GetAllAccounts(_mockAccountService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GetAccountById_ShouldReturnOk_WhenAccountExists()
    {
        // Arrange
        var account = new AccountResponseDto(1, "Checking", "Bank", 500m, "checking");
        _mockAccountService.Setup(s => s.GetAccountByIdAsync(1)).ReturnsAsync(account);

        // Act
        var result = await AccountRouteHandlers.GetAccountById(1, _mockAccountService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GetAccountById_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        _mockAccountService.Setup(s => s.GetAccountByIdAsync(99)).ReturnsAsync((AccountResponseDto?)null);

        // Act
        var result = await AccountRouteHandlers.GetAccountById(99, _mockAccountService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GetAccountBySlug_ShouldReturnOk_WhenAccountExists()
    {
        // Arrange
        var account = new AccountResponseDto(1, "Primary Savings", "Savings", 500m, "primary-savings");
        _mockAccountService.Setup(s => s.GetAccountBySlugAsync("primary-savings")).ReturnsAsync(account);

        // Act
        var result = await AccountRouteHandlers.GetAccountBySlug("primary-savings", _mockAccountService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GetAccountBySlug_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        _mockAccountService.Setup(s => s.GetAccountBySlugAsync("missing")).ReturnsAsync((AccountResponseDto?)null);

        // Act
        var result = await AccountRouteHandlers.GetAccountBySlug("missing", _mockAccountService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GetAccountByType_ShouldReturnOk_WhenAccountExists()
    {
        // Arrange
        var account = new AccountResponseDto(1, "Primary Savings", "Savings", 500m, "primary-savings");
        _mockAccountService.Setup(s => s.GetAccountByTypeAsync("savings")).ReturnsAsync(account);

        // Act
        var result = await AccountRouteHandlers.GetAccountByType("savings", _mockAccountService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GetAccountByType_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        _mockAccountService.Setup(s => s.GetAccountByTypeAsync("missing")).ReturnsAsync((AccountResponseDto?)null);

        // Act
        var result = await AccountRouteHandlers.GetAccountByType("missing", _mockAccountService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task CreateAccount_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateAccountRequestDto("Savings", "Bank", 1000m);
        var created = new AccountResponseDto(1, "Savings", "Bank", 1000m, "savings");
        _mockAccountService.Setup(s => s.CreateAccountAsync(request)).ReturnsAsync(created);

        // Act
        var result = await AccountRouteHandlers.CreateAccount(request, _mockAccountService.Object);

        // Assert
        Assert.IsNotNull(result);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await result.ExecuteAsync(httpContext);
        Assert.AreEqual(StatusCodes.Status201Created, httpContext.Response.StatusCode);
    }
}
