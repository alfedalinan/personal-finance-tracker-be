using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Interfaces;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Implementations;

namespace PersonalFinanceTracker.Tests.Services;

[TestClass]
public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _mockTransactionRepo;
    private readonly Mock<IAccountRepository> _mockAccountRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _mockTransactionRepo = new Mock<ITransactionRepository>();
        _mockAccountRepo = new Mock<IAccountRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _service = new TransactionService(
            _mockTransactionRepo.Object,
            _mockAccountRepo.Object,
            _mockCategoryRepo.Object
        );
    }

    [TestMethod]
    public async Task CreateTransactionAsync_ShouldCreateTransaction_WhenDataIsValid()
    {
        // Arrange
        var request = new CreateTransactionRequestDto(1, 1, "Water Bill Payment", -185.00m);
        _mockAccountRepo.Setup(r => r.GetAccountByIdAsync(1)).ReturnsAsync(new Account { Id = 1 });
        _mockCategoryRepo.Setup(r => r.GetCategoryByIdAsync(1)).ReturnsAsync(new Category { Id = 1 });
        _mockTransactionRepo.Setup(r => r.CreateTransactionAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(new Transaction { Id = 1, Amount = -185.00m });

        // Act
        var result = await _service.CreateTransactionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(-185.00m, result.Amount);
    }

    [TestMethod]
    public async Task CreateTransactionAsync_ShouldThrowException_WhenAccountNotFound()
    {
        // Arrange
        var request = new CreateTransactionRequestDto(99, 1, "Test", -10.00m);
        _mockAccountRepo.Setup(r => r.GetAccountByIdAsync(99)).ReturnsAsync((Account?)null);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.CreateTransactionAsync(request));
    }

    [TestMethod]
    public async Task CreateTransactionAsync_ShouldThrowException_WhenCategoryNotFound()
    {
        // Arrange
        var request = new CreateTransactionRequestDto(1, 99, "Test", -10.00m);
        _mockAccountRepo.Setup(r => r.GetAccountByIdAsync(1)).ReturnsAsync(new Account { Id = 1 });
        _mockCategoryRepo.Setup(r => r.GetCategoryByIdAsync(99)).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.CreateTransactionAsync(request));
    }
}
