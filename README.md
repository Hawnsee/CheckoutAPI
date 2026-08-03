# Checkout API

A robust, scalable .NET REST API built to manage checkout operations. This project serves as a technical showcase of enterprise-grade backend architecture, focusing on transaction safety, state management, and separation of concerns.

## Architecture & Design Patterns
This API is structured around modern architectural principles to ensure high maintainability and reliability in production environments:
*   **CQRS (Command Query Responsibility Segregation):** Strict separation between read and write operations, isolated within the `Application/Commands` and `Application/Queries` directories.
*   **Domain-Driven Design (DDD):** Core business logic and states are encapsulated in domain models such as `OrderStatusType` and `IdempotentRequest`.
*   **Idempotency & Safe Retries:** Implemented via `IdentifiedCommand` and `IdempotentRequestDAO` to guarantee that concurrent or duplicated checkout requests do not result in double-processing or data corruption.
*   **Concurrency Management:** Advanced handling of state and race conditions (detailed in `docs/001-Concurrencia-Estado.md`).

## Tech Stack
*   **Framework:** .NET / C#
*   **ORM:** Entity Framework Core (using code-first approach via the `Migrations` folder and `ApplicationDBContext`)
*   **Containerization:** Docker & Docker Compose (`Dockerfile`, `docker-compose.yml`) for consistent deployment

## Local Setup
To run this project locally, ensure you have Docker installed on your machine.

1. Clone the repository.
2. Build and start the isolated environment using Docker Compose:
```bash
   docker-compose up -d --build
   ```

## API Testing & Endpoints
For quick testing and endpoint discovery, HTTP files are included in the `htmlTest/` directory. These can be executed directly using VS Code's REST Client:
*   `post_apiStatus.http`: Triggers a checkout or status command.
*   `get_idempotencyRequest.http`: Queries the status of a specific idempotent operation.
