using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Repositories.Implementations;
using PersonalFinanceTracker.Repositories.Interfaces;
using PersonalFinanceTracker.RouteHandlers;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Implementations;
using PersonalFinanceTracker.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<FinanceTrackerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=financetracker.db"));

// Register Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Register Services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

var app = builder.Build();

// Initialize the database and seed initial accounts
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating/seeding the database.");
    }
}

// Configure the HTTP request pipeline.
app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    app.MapGet("/weatherforecast", () =>
    {
        var forecast =  Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");
}

app.MapGet("/api/accounts/{accountId}/transactions", TransactionRouteHandlers.GetAccountTransactionsAndBalance)
.WithName("GetAccountTransactionsAndBalance")
.Produces<AccountTransactionsResponseDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.WithSummary("Get account transactions and balance")
.WithDescription("Retrieves transactions and balance summary for an account within an optional date range.");

app.MapGet("/api/accounts", AccountRouteHandlers.GetAllAccounts)
.WithName("GetAllAccounts")
.Produces<IEnumerable<AccountResponseDto>>(StatusCodes.Status200OK)
.WithSummary("Get all accounts")
.WithDescription("Retrieves a list of all bank accounts.");

app.MapGet("/api/accounts/{id}", AccountRouteHandlers.GetAccountById)
.WithName("GetAccountById")
.Produces<AccountResponseDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithSummary("Get account by ID")
.WithDescription("Retrieves a specific bank account by its unique identifier.");

app.MapGet("/api/accounts/slug/{slug}", AccountRouteHandlers.GetAccountBySlug)
.WithName("GetAccountBySlug")
.Produces<AccountResponseDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithSummary("Get account by slug")
.WithDescription("Retrieves a specific bank account by its universal slug.");

app.MapGet("/api/accounts/by-type/{type}", AccountRouteHandlers.GetAccountByType)
.WithName("GetAccountByType")
.Produces<AccountResponseDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithSummary("Get account by type or slug")
.WithDescription("Retrieves a specific bank account by its account type or slug.");

app.MapPost("/api/accounts", AccountRouteHandlers.CreateAccount)
.WithName("CreateAccount")
.Produces<AccountResponseDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.WithSummary("Create a new account")
.WithDescription("Creates a new bank account with the specified details.");

app.MapGet("/api/categories", CategoryRouteHandlers.GetAllCategories)
.WithName("GetAllCategories")
.Produces<IEnumerable<CategoryResponseDto>>(StatusCodes.Status200OK)
.WithSummary("Get all categories")
.WithDescription("Retrieves a list of all transaction categories.");

app.MapPost("/api/categories", CategoryRouteHandlers.CreateCategory)
.WithName("CreateCategory")
.Produces<CategoryResponseDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.WithSummary("Create a new category")
.WithDescription("Creates a new transaction category with the specified details.");

app.MapPost("/api/transactions", TransactionRouteHandlers.CreateTransaction)
.WithName("CreateTransaction")
.Produces<TransactionSummaryDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.WithSummary("Create a new transaction")
.WithDescription("Creates a new financial transaction for an account.");

app.MapPut("/api/transactions/{id}", TransactionRouteHandlers.UpdateTransaction)
.WithName("UpdateTransaction")
.Produces<TransactionSummaryDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.WithSummary("Update an existing transaction")
.WithDescription("Updates an existing financial transaction by its unique identifier.");

app.MapDelete("/api/transactions/{id}", TransactionRouteHandlers.DeleteTransaction)
.WithName("DeleteTransaction")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.WithSummary("Delete a transaction")
.WithDescription("Deletes a financial transaction by its unique identifier.");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
