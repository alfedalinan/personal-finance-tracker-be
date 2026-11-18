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
- `Data/`: Entity Framework Core database context (`FinanceTrackerDbContext`)
- `Program.cs`: Application entry point and database service registration.

## Current Progress & Roadmap
- [x] **Data Layer**: Core entities, relationships, and SQLite DbContext configured.
- [ ] **API Endpoints**: CRUD operations for Accounts, Categories, and Transactions.
- [ ] **Validation & Error Handling**: Robust input validation and exception middleware.
- [ ] **Testing**: Unit and integration tests for services and endpoints.
