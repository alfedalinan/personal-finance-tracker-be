using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PersonalFinanceTracker.Tests;

[TestClass]
public class OpenApiVerificationTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _factory.Dispose();
    }

    [TestMethod]
    public async Task GetOpenApiDocument_ShouldReturnSuccessAndContainProductionEndpoints()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/openapi/v1.json");

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode, $"Expected success status code, but got {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.IsNotNull(content);
        Assert.IsTrue(content.Contains("/api/accounts"), "OpenAPI document should contain /api/accounts");
        Assert.IsTrue(content.Contains("/api/categories"), "OpenAPI document should contain /api/categories");
        Assert.IsTrue(content.Contains("/api/transactions"), "OpenAPI document should contain /api/transactions");
    }
}
