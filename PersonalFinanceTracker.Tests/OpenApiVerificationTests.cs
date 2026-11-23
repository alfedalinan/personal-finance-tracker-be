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
        Assert.IsTrue(content.Contains("/api/categories/list"), "OpenAPI document should contain /api/categories/list");
        Assert.IsFalse(content.Contains("/api/categories/types/list"), "OpenAPI document should NOT contain legacy /api/categories/types/list");
        Assert.IsTrue(content.Contains("/api/transactions"), "OpenAPI document should contain /api/transactions");
    }

    [TestMethod]
    public async Task CategoryEndpoints_Behavior_ShouldMatchCleanReplacement()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - New endpoint
        var listResponse = await client.GetAsync("/api/categories/list");
        // Act - Legacy endpoint
        var legacyResponse = await client.GetAsync("/api/categories/types/list");

        // Assert
        Assert.IsTrue(listResponse.IsSuccessStatusCode, "Expected 200 OK for /api/categories/list");
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, legacyResponse.StatusCode, "Expected 404 Not Found for legacy /api/categories/types/list");
    }
}
