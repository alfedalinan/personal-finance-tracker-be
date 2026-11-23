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
    public async Task GetAccountBySlugAsync_ShouldReturnAccount_WhenAccountExists()
    {
        // Arrange
        var account = new Account { Id = 1, Name = "Primary Savings", Type = "Savings", Slug = "primary-savings", CurrentBalance = 500m };
        _mockAccountRepo.Setup(r => r.GetAccountBySlugAsync("primary-savings")).ReturnsAsync(account);

        // Act
        var result = await _service.GetAccountBySlugAsync("primary-savings");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("primary-savings", result.Slug);
        Assert.AreEqual("Primary Savings", result.Name);
    }

    [TestMethod]
    public async Task GetAccountBySlugAsync_ShouldReturnNull_WhenAccountDoesNotExist()
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.GetAccountBySlugAsync("missing")).ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GetAccountBySlugAsync("missing");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAccountByTypeAsync_ShouldReturnAccount_WhenAccountExists()
    {
        // Arrange
        var account = new Account { Id = 1, Name = "Primary Savings", Type = "Savings", Slug = "primary-savings", CurrentBalance = 500m };
        _mockAccountRepo.Setup(r => r.GetAccountByTypeAsync("savings")).ReturnsAsync(account);

        // Act
        var result = await _service.GetAccountByTypeAsync("savings");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Savings", result.Type);
    }

    [TestMethod]
    public async Task CreateAccountAsync_ShouldCreateAndReturnAccount()
    {
        // Arrange
        var request = new CreateAccountRequestDto("Investment", "Brokerage", 1500m, "custom-slug");
        _mockAccountRepo.Setup(r => r.CreateAccountAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account a) => { a.Id = 10; return a; });

        // Act
        var result = await _service.CreateAccountAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(10, result.Id);
        Assert.AreEqual("Investment", result.Name);
        Assert.AreEqual("Brokerage", result.Type);
        Assert.AreEqual("custom-slug", result.Slug);
        Assert.AreEqual(1500m, result.CurrentBalance);
    }

    [TestMethod]
    public async Task GetAllAccountsWithRecentTransactionsAsync_ShouldReturnMappedDtos()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                Id = 1,
                Name = "Checking",
                Type = "Bank",
                CurrentBalance = 500m,
                Slug = "checking",
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Id = 100,
                        DateOfTransaction = DateTime.UtcNow,
                        Amount = 50m,
                        BeforeBalance = 450m,
                        AfterBalance = 500m,
                        Description = "Salary",
                        Category = new Category { Name = "Income", Type = CategoryType.Income }
                    }
                }
            }
        };
        _mockAccountRepo.Setup(r => r.GetAllAccountsWithRecentTransactionsAsync()).ReturnsAsync(accounts);

        // Act
        var result = await _service.GetAllAccountsWithRecentTransactionsAsync();

        // Assert
        Assert.IsNotNull(result);
        var list = result.ToList();
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual("Checking", list[0].Name);
        var txs = list[0].Transactions.ToList();
        Assert.AreEqual(1, txs.Count);
        Assert.AreEqual("Salary", txs[0].Description);
        Assert.AreEqual("Income", txs[0].CategoryName);
    }
}
