# Personal Finance Tracker

A robust, high-performance, and multi-tenant Personal Finance Tracker API built with **.NET 10** and **Entity Framework Core**, featuring ASP.NET Core Minimal APIs, SQLite persistence, repository-service architecture, and a comprehensive test suite.

---

## Table of Contents
- [Features](#features)
- [Multi-Tenant Architecture](#multi-tenant-architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Current Progress & Roadmap](#current-progress--roadmap)
- [Getting Started](#getting-started)
- [Running Tests](#running-tests)
- [API Endpoints Reference](#api-endpoints-reference)
  - [Accounts API](#accounts-api)
  - [Categories API](#categories-api)
  - [Transactions API](#transactions-api)

---

## Features
- **Multi-Tenant Isolation**: Tenant context resolution per request via the `X-Tenant-ID` header, ensuring strict logical separation of accounts, categories, and transactions across different tenants.
- **Account Management**: Track multiple accounts (checking, savings, credit cards, investments), initial balances, currency codes, and custom slugs.
- **Categorization**: Organize financial records into Income and Expense categories (`CategoryType`).
- **Transaction Logging**: Record transaction amounts, timestamps, descriptions, type, and link them to specific accounts and categories with balance recalculations.
- **Minimal API Endpoints**: Lightweight, fast, and strongly-typed endpoint route handlers with OpenAPI / Scalar documentation support.

---

## Multi-Tenant Architecture

The backend implements a robust multi-tenant isolation pattern powered by `TenantResolutionMiddleware` and scoped `ITenantContext`:

1. **Tenant Resolution (`TenantResolutionMiddleware`)**:
   - Inspects incoming HTTP requests for the `X-Tenant-ID` header (case-insensitive).
   - If present, extracts and normalizes the tenant identifier (trimmed and lowercased).
   - If missing on non-API requests or when running in Development/Test environments, falls back to a configured default tenant (`TenantSettings:DefaultTenantId` or `"default-tenant"`).
   - In production environments for API requests, returns `400 Bad Request` if `X-Tenant-ID` is missing.
2. **Context Propagation (`ITenantContext`)**:
   - Registered as a scoped service (`ITenantContext` / `TenantContext`).
   - Automatically injected into repositories and data access layers to apply global tenant query filters in Entity Framework Core (`FinanceTrackerDbContext`), ensuring tenants can only access their own data.

---

## Tech Stack
- **Backend Framework**: .NET 10 / ASP.NET Core Web API (Minimal APIs)
- **ORM**: Entity Framework Core 10 with SQLite
- **Testing**: xUnit, FluentAssertions / Moq
- **API Documentation**: OpenAPI / Scalar

---

## Project Structure
- `PersonalFinanceTracker/`
  - `Entities/`: Domain models (`Account.cs`, `Category.cs`, `Transaction.cs`, `CategoryType.cs`)
  - `Data/`: EF Core database context (`FinanceTrackerDbContext.cs`) and database initializer (`DbInitializer.cs`)
  - `Middleware/`: Tenant isolation middleware (`TenantResolutionMiddleware.cs`)
  - `Repositories/`: Data access layer abstraction and concrete implementations
    - `Interfaces/`: Repository contracts (`IAccountRepository.cs`, `ICategoryRepository.cs`, `ITransactionRepository.cs`)
    - `Implementations/`: EF Core concrete classes (`AccountRepository.cs`, `CategoryRepository.cs`, `TransactionRepository.cs`)
  - `Services/`: Business logic layer
    - `Interfaces/`: Service contracts (`IAccountService.cs`, `ICategoryService.cs`, `ITransactionService.cs`)
    - `Implementations/`: Concrete services (`AccountService.cs`, `CategoryService.cs`, `TransactionService.cs`)
    - `Dtos/`: Data Transfer Objects (`AccountResponseDto.cs`, `CreateAccountRequestDto.cs`, `CreateTransactionRequestDto.cs`, `TransactionSummaryDto.cs`, etc.)
  - `RouteHandlers/`: Minimal API static route handlers (`AccountRouteHandlers.cs`, `CategoryRouteHandlers.cs`, `TransactionRouteHandlers.cs`)
  - `Program.cs`: Application startup, dependency injection container, middleware pipeline, and endpoint mappings.
- `PersonalFinanceTracker.Tests/`: Comprehensive unit and integration test suite covering repositories, services, middleware, route handlers, and DB initialization.

---

## Current Progress & Roadmap
- [x] **Core Domain & Data Layer**: Entities, relationships, migrations, and SQLite `FinanceTrackerDbContext` configured.
- [x] **Multi-Tenant Middleware**: Request-level tenant identification via `X-Tenant-ID` and `TenantResolutionMiddleware`.
- [x] **Repository & Service Layers**: Full CRUD repository implementations and business services for Accounts, Categories, and Transactions.
- [x] **Minimal API Route Handlers**: Endpoint mappings for all domains with OpenAPI response type metadata.
- [x] **Comprehensive Test Suite**: 82+ unit and integration tests covering multi-tenant isolation, repositories, services, and route handlers.

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/) installed on your machine.

### Running the Application
1. Clone the repository and navigate to the project directory:
   ```bash
   cd personal-finance-tracker-be
   ```
2. Restore dependencies and run the application:
   ```bash
   dotnet restore
   dotnet run --project PersonalFinanceTracker/PersonalFinanceTracker.csproj
   ```
3. The API will start (typically on `http://localhost:5088` or `https://localhost:7215`).

---

## Running Tests
To execute the complete unit and integration test suite (`PersonalFinanceTracker.Tests`), run:
```bash
dotnet test
```
This executes all 82+ tests verifying repository queries, service logic, tenant isolation, and route handler behaviors.

---

## API Endpoints Reference

All API requests can include an optional `X-Tenant-ID` header (e.g., `X-Tenant-ID: tenant-alpha`). If omitted in development, the default tenant is used.

### Accounts API

#### 1. Get All Accounts
- **URL**: `GET /api/accounts`
- **Description**: Retrieves all accounts belonging to the current tenant.
- **Example (`curl`)**:
  ```bash
  curl -X GET "http://localhost:5088/api/accounts" \
       -H "Accept: application/json" \
       -H "X-Tenant-ID: tenant-alpha"
  ```

#### 2. Get Account Dropdown List
- **URL**: `GET /api/accounts/list`
- **Description**: Retrieves a lightweight list of account IDs and names for UI dropdown selectors.

#### 3. Get Accounts with Recent Transactions
- **URL**: `GET /api/accounts/summary`
- **Description**: Retrieves all accounts with their top 5 most recent transactions.

#### 4. Get Account by ID
- **URL**: `GET /api/accounts/{id}`
- **Description**: Retrieves a specific account by its unique numeric ID.

#### 5. Get Account by Slug
- **URL**: `GET /api/accounts/slug/{slug}`
- **Description**: Retrieves a specific account by its URL-friendly slug.

#### 6. Get Account by Type
- **URL**: `GET /api/accounts/by-type/{type}`
- **Description**: Retrieves an account by its account type or slug.

#### 7. Create Account
- **URL**: `POST /api/accounts`
- **Description**: Creates a new bank account.
- **Request Body (`application/json`)**:
  ```json
  {
    "name": "Primary Checking",
    "type": "Checking",
    "initialBalance": 1500.00,
    "currency": "USD"
  }
  ```
- **Example (`curl`)**:
  ```bash
  curl -X POST "http://localhost:5088/api/accounts" \
       -H "Content-Type: application/json" \
       -H "X-Tenant-ID: tenant-alpha" \
       -d '{"name":"Primary Checking","type":"Checking","initialBalance":1500.00,"currency":"USD"}'
  ```

#### 8. Get Account Transactions and Balance
- **URL**: `GET /api/accounts/{accountId}/transactions?startDate=2026-01-01&endDate=2026-12-31`
- **Description**: Retrieves account details, current balance, remaining balance, and filtered transactions.

---

### Categories API

#### 1. Get All Categories
- **URL**: `GET /api/categories`
- **Description**: Retrieves all transaction categories (Income and Expense) for the tenant.
- **Example (`curl`)**:
  ```bash
  curl -X GET "http://localhost:5088/api/categories" \
       -H "Accept: application/json" \
       -H "X-Tenant-ID: tenant-alpha"
  ```

#### 2. Create Category
- **URL**: `POST /api/categories`
- **Description**: Creates a new transaction category.
- **Request Body (`application/json`)**:
  ```json
  {
    "name": "Groceries",
    "type": "Expense"
  }
  ```

---

### Transactions API

#### 1. Create Transaction
- **URL**: `POST /api/transactions`
- **Description**: Records a new transaction (income or expense) against an account.
- **Request Body (`application/json`)**:
  ```json
  {
    "accountId": 1,
    "categoryId": 2,
    "amount": 75.50,
    "type": "Expense",
    "description": "Supermarket shopping",
    "date": "2026-11-28T00:00:00Z"
  }
  ```

#### 2. Update Transaction
- **URL**: `PUT /api/transactions/{id}`
- **Description**: Updates an existing transaction.

#### 3. Delete Transaction
- **URL**: `DELETE /api/transactions/{id}`
- **Description**: Deletes a transaction by its unique ID and updates account balances.

