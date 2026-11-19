# Personal Finance Tracker

A robust and lightweight Personal Finance Tracker API built with **.NET 10** and **Entity Framework Core**.

## Features
- **Account Management**: Track multiple accounts (checking, savings, credit cards) and their current balances.
- **Categorization**: Organize financial records into Income and Expense categories.
- **Transaction Logging**: Record transaction amounts, timestamps, descriptions, and link them to specific accounts and categories.

## Tech Stack
- **Backend**: .NET 10 / ASP.NET Core Web API
- **ORM**: Entity Framework Core 10 (SQLite)
- **Documentation**: OpenAPI / Scalar

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/) installed on your machine.

### Running the Application
1. Clone the repository and navigate to the project directory.
2. Restore dependencies and run the project:
   ```bash
   dotnet restore
   dotnet run
   ```
3. The API will start and expose OpenAPI endpoints (check `Program.cs` and `launchSettings.json` for ports).

## Project Structure
- `Entities/`: Domain models (`Account`, `Category`, `Transaction`, etc.)
- `Data/`: Entity Framework Core database context (`FinanceTrackerDbContext`) and database initializer (`DbInitializer.cs`)
- `Repositories/`: Data access layer abstraction and concrete implementations
  - `Interfaces/`: Repository contracts (`IAccountRepository.cs`, `ICategoryRepository.cs`, `ITransactionRepository.cs`)
  - `Implementations/`: EF Core concrete repository classes (`AccountRepository.cs`, `CategoryRepository.cs`, `TransactionRepository.cs`)
- `Services/`: Business logic layer
  - `Interfaces/`: Service contracts (`ITransactionService.cs`)
  - `Implementations/`: Concrete service classes (`TransactionService.cs`)
  - `Dtos/`: Data transfer objects (`AccountTransactionsResponseDto.cs`, `CreateTransactionRequestDto.cs`, `TransactionSummaryDto.cs`)
- `RouteHandlers/`: Minimal API route handler static classes (`TransactionRouteHandlers.cs`)
- `PersonalFinanceTracker.Tests/`: Comprehensive unit test suite covering repositories, services, and database initialization (`AccountRepositoryTests.cs`, `CategoryRepositoryTests.cs`, `TransactionRepositoryTests.cs`, `TransactionServiceTests.cs`, `DbInitializerTests.cs`)
- `Program.cs`: Application entry point, service registrations, and API endpoint mappings.

## Current Progress & Roadmap
- [x] **Data Layer**: Core entities, relationships, and SQLite DbContext configured.
- [x] **Data Access & Repository Layer**: Interfaces and EF Core concrete implementations.
- [x] **Services & Business Logic**: Service layer implemented (`ITransactionService`, `TransactionService`) with DTOs.
- [x] **Transactional Route Handlers**: Minimal API route handlers (`GetAccountTransactionsAndBalance`) configured in `Program.cs`.
- [x] **Unit & Integration Tests**: Comprehensive unit test suite covering repositories, services, and database initialization (`PersonalFinanceTracker.Tests`).
- [ ] **Validation & Error Handling**: Robust input validation and exception middleware (Future enhancement).

## API Endpoints

### Get Account Transactions and Balance
Retrieves account details, current balance, remaining balance, and a list of associated transactions.

- **URL**: `/api/accounts/{accountId}/transactions`
- **Method**: `GET`
- **Response**: `AccountTransactionsResponseDto`

#### Example (`PersonalFinanceTracker.http`)
```http
@PersonalFinanceTracker_HostAddress = http://localhost:5088

GET {{PersonalFinanceTracker_HostAddress}}/api/accounts/1/transactions
Accept: application/json
```

#### Example (`curl`)
```bash
curl -X GET "http://localhost:5088/api/accounts/1/transactions" -H "Accept: application/json"
```
