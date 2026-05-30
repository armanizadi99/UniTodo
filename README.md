# UniTodo API

[![.NET 9](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**UniTodo** is a modern, robust, and highly structured backend API designed for collaborative task management. Whether for individual productivity or high-performance team coordination, UniTodo provides the infrastructure to manage complex workflows through reusable templates and active task "runs."

Built with a focus on **Clean Architecture** and **Domain-Driven Design (DDD)**, this project serves as a demonstration of professional backend engineering standards in the .NET ecosystem.

## 🚀 Key Features

-   **Modular Monolith Architecture:** Clear separation of concerns between `Auth` and `Todos` modules, allowing for independent development and high maintainability.
-   **Reusable Todo Templates:** Define structured task lists once and instantiate them whenever needed, ensuring consistency in recurring workflows.
-   **Collaborative Runs:** Manage active instances of todo lists with support for team members, individual task assignments, and status tracking.
-   **Secure Authentication:** Integrated authentication using **ASP.NET Core Identity** and custom-issued **JWT Bearer** tokens.
-   **Auditable Entities:** Built-in tracking for entity creation and modification timestamps across the domain model.
-   **Strongly Typed IDs:** Enhanced type safety and prevention of "primitive obsession" using generic strongly-typed identifiers.

## 🏗️ Architecture & Engineering Standards

UniTodo is engineered for maintainability and scalability, adhering to industry-standard patterns:

### Domain-Driven Design (DDD)
The core logic resides in a rich domain model, utilizing:
-   **Entities & Aggregates:** Enforcing business rules and invariants (e.g., `TodoListTemplate`, `TodoListRun`).
-   **Value Objects:** Encapsulating logic for domain primitives like `UserId`, `TodoItemDescription`, and `TodoItemNotes`.
-   **Domain Exceptions:** Precise error handling that reflects business logic failures.

### Clean Architecture & Clean Code
The project is strictly layered to decouple business logic from external concerns:
-   **Explicit Mapping:** Currently utilizes clean, manual mapping between Entities and DTOs within the Application layer to maintain full control over the data contract, with plans to move to entity extension methods for better encapsulation.
-   **Persistence Layer:** Decoupled using the Unit of Work and Repository patterns.
-   **Fluent Configuration:** Database schemas are managed via explicit EF configurations, keeping the domain model clean of persistence attributes.

### Persistence & Data
-   **Modular Databases:** Uses separate database contexts for different modules (Identity and Todos) to ensure data isolation.
-   **Automatic Auditing:** `IAuditable` interface and `DbContext` overrides handle `CreatedAt` and `UpdatedAt` timestamps automatically.

## 🛠️ Tech Stack

-   **Framework:** .NET 9.0 (ASP.NET Core)
-   **Persistence:** EF Core 9.0 with SQLite
-   **Identity:** ASP.NET Core Identity
-   **Security:** JWT Bearer Authentication
-   **Testing:** xUnit
-   **Documentation:** Swagger/OpenAPI

## 🚦 Getting Started

### Prerequisites
-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
-   Visual Studio 2022 (or JetBrains Rider / VS Code)

### Installation
1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-username/UniTodo.git
    cd UniTodo
    ```

2.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```

3.  **Setup Databases:**
    The project uses SQLite for both authentication and task management. Apply the migrations for both contexts:
    ```bash
    # Apply migrations for the Todo module
    dotnet ef database update --project UniTodo --context TodoDbContext
    
    # Apply migrations for the Auth module
    dotnet ef database update --project UniTodo --context AuthDbContext
    ```
    *Note: This will create `Todos.db` and `Auth.db` in the project root.*

4.  **Run the application:**
    ```bash
    dotnet run --project UniTodo
    ```
    The API will be available at `https://localhost:5000` (or your configured port). You can access the Swagger UI at `/swagger/index.html`.

## 🧪 Testing
The project includes unit tests for the Domain, Application, and Infrastructure layers.
```bash
dotnet test
```

---
Developed with ❤️ by [hamidreza izadi/armanizadi99]
