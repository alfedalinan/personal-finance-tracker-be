using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Interfaces;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Implementations;

namespace PersonalFinanceTracker.Tests.Services;

[TestClass]
public class AccountServiceTests
{
    private Mock<IAccountRepository> _mockAccountRepo = null!;
    private AccountService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockAccountRepo = new Mock<IAccountRepository>();
        _service = new AccountService(_mockAccountRepo.Object);
    }

    [TestMethod]
    public async Task GetAllAccountsAsync_ShouldReturnAllAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Name = "Checking", Type = "Bank", CurrentBalance = 500m },
            new Account { Id = 2, Name = "Savings", Type = "Bank", CurrentBalance = 2000m }
        };
        _mockAccountRepo.Setup(r => r.GetAllAccountsAsync()).ReturnsAsync(accounts);

        // Act
        var result = await _service.GetAllAccountsAsync();

        // Assert
        Assert.IsNotNull(result);
        var list = result.ToList();
        Assert.AreEqual(2, list.Count);
        Assert.AreEqual("Checking", list[0].Name);
        Assert.AreEqual(500m, list[0].CurrentBalance);
    }

    [TestMethod]
    public async Task GetAccountByIdAsync_ShouldReturnAccount_WhenAccountExists()
    {
        // Arrange
        var account = new Account { Id = 1, Name = "Checking", Type = "Bank", CurrentBalance = 500m };
        _mockAccountRepo.Setup(r => r.GetAccountByIdAsync(1)).ReturnsAsync(account);

        // Act
        var result = await _service.GetAccountByIdAsync(1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Checking", result.Name);
        Assert.AreEqual(500m, result.CurrentBalance);
    }

    [TestMethod]
    public async Task GetAccountByIdAsync_ShouldReturnNull_WhenAccountDoesNotExist()
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.GetAccountByIdAsync(99)).ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GetAccountByIdAsync(99);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateAccountAsync_ShouldCreateAndReturnAccount()
    {
        // Arrange
        var request = new CreateAccountRequestDto("Investment", "Brokerage", 1500m);
        _mockAccountRepo.Setup(r => r.CreateAccountAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account a) => { a.Id = 10; return a; });

        // Act
        var result = await _service.CreateAccountAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(10, result.Id);
        Assert.AreEqual("Investment", result.Name);
        Assert.AreEqual("Brokerage", result.Type);
        Assert.AreEqual(1500m, result.CurrentBalance);
    }
}
