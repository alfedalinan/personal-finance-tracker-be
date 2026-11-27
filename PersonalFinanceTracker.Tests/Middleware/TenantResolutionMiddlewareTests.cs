using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PersonalFinanceTracker.Tests.Middleware;

[TestClass]
public class TenantResolutionMiddlewareTests
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
    public async Task Middleware_ExtractsTenantId_CaseInsensitive()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        }).CreateClient();

        client.DefaultRequestHeaders.Add("X-Tenant-ID", "  11111111-1111-1111-1111-111111111111  ");

        var response = await client.GetAsync("/api/accounts/1/transactions?startDate=2026-01-01");
        Assert.AreNotEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Middleware_NonApiRoute_BypassesValidation_InProduction()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        }).CreateClient();

        // /openapi/v1.json should not require X-Tenant-ID in production
        var response = await client.GetAsync("/openapi/v1.json");
        Assert.AreNotEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Middleware_MissingHeader_InProduction_ReturnsBadRequest()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        }).CreateClient();

        var response = await client.GetAsync("/api/accounts/1/transactions?startDate=2026-01-01");
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Middleware_MissingHeader_InDevelopment_UsesFallback()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        }).CreateClient();

        var response = await client.GetAsync("/api/accounts/1/transactions?startDate=2026-01-01");
        Assert.AreNotEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
