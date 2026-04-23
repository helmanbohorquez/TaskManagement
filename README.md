# Task Management

A full-stack task-management app built for the .NET technical exercise. The backend is ASP.NET Core Web API with Clean Architecture and TDD, hand-rolled ADO.NET against SQLite, BCrypt password hashing, and JWT bearer authentication. The frontend is a React + Vite SPA styled with Tailwind.

> **Constraints respected:** no Entity Framework, no Dapper, no MediatR.

## Features

- Email + password sign-up and login; JWT bearer tokens with issuer/audience/lifetime/signature validation.
- CRUD on tasks with `title`, `description`, `status` (`Pending`, `InProgress`, `Done`), and `dueDate`.
- Tasks are always scoped to the authenticated user (no cross-account leakage).
- Responsive React UI with list filters, create/edit modal, and status badges.
- Swagger UI with an "Authorize" button for local exploration.
- Demo account seeded on first run.

## Repository layout

```
TaskManagement/
  backend/
    TaskManagement.slnx
    src/
      TaskManagement.Domain/          entities, enums, exceptions, repo interfaces
      TaskManagement.Application/     services, DTOs, FluentValidation validators
      TaskManagement.Infrastructure/  ADO.NET repos, Bcrypt, JWT, DbInitializer
      TaskManagement.Api/             controllers, middleware, Program.cs
    tests/
      TaskManagement.Domain.Tests/
      TaskManagement.Application.Tests/
      TaskManagement.Infrastructure.Tests/
      TaskManagement.Api.Tests/
  frontend/
    task-manager-ui/                  React + Vite + JS + Tailwind
  docs/
    user-story.md
    architecture.md
    genai-section.md
```

See [docs/architecture.md](docs/architecture.md) for layer boundaries, data flow, and testing strategy.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (the project targets `net10.0`).
- [Node.js 20+](https://nodejs.org/) and npm (only required for the frontend).

## Backend

```bash
cd backend

# run all unit + integration tests (64 tests)
dotnet test TaskManagement.slnx

# start the API on http://localhost:5080
dotnet run --project src/TaskManagement.Api
```

- Swagger UI: http://localhost:5080/swagger
- Health check: http://localhost:5080/api/health
- SQLite file: `backend/src/TaskManagement.Api/taskmgmt.db` (created on first run and auto-seeded)

### Demo credentials

| Email              | Password  |
| ------------------ | --------- |
| `demo@tasks.test`  | `Demo123!` |

Three sample tasks in mixed statuses are also seeded the first time the database is created.

### Configuration

`backend/src/TaskManagement.Api/appsettings.json` controls:

- `ConnectionStrings:Default` - SQLite data source.
- `Jwt:Issuer`, `Jwt:Audience`, `Jwt:SigningKey`, `Jwt:ExpiryMinutes` - JWT settings. **Replace `SigningKey` with a real secret for non-local deployments.**
- `Cors:AllowedOrigins` - origins permitted to call the API.
- `Seed:Demo` - set to `false` to skip seeding the demo user and tasks.

## Frontend

```bash
cd frontend/task-manager-ui
npm install
npm run dev
```

The SPA starts on http://localhost:5173 and expects the API at `VITE_API_BASE_URL` (defaults to `http://localhost:5080`; override via `.env`).

## API reference (short version)

| Verb   | Route                 | Auth  | Notes                                                      |
| ------ | --------------------- | ----- | ---------------------------------------------------------- |
| POST   | `/api/auth/register`  | No    | `{ email, password }` -> `{ token, expiresAt, email }`     |
| POST   | `/api/auth/login`     | No    | Same payload and response as register.                     |
| GET    | `/api/health`         | No    | Liveness probe.                                            |
| GET    | `/api/tasks`          | Yes   | Optional `?status=Pending|InProgress|Done`.                |
| GET    | `/api/tasks/{id}`     | Yes   | Returns 404 if not owned by caller.                        |
| POST   | `/api/tasks`          | Yes   | `{ title, description?, dueDate }` -> 201 with Location.   |
| PUT    | `/api/tasks/{id}`     | Yes   | `{ title, description?, dueDate, status }`                 |
| DELETE | `/api/tasks/{id}`     | Yes   | 204 on success, 404 if not owned.                          |

Error responses share the shape `{ "code": "ValidationError", "message": "..." }`.

## Test summary

```
dotnet test TaskManagement.slnx
```

| Suite                               | Tests | Focus                                                     |
| ----------------------------------- | ----- | --------------------------------------------------------- |
| `TaskManagement.Domain.Tests`       | 18    | Entity invariants and guard clauses.                      |
| `TaskManagement.Application.Tests`  | 19    | Services with mocked repos; real validators.              |
| `TaskManagement.Infrastructure.Tests` | 17  | SQLite repo behavior, BCrypt hasher, JWT issuance.        |
| `TaskManagement.Api.Tests`          | 10    | HTTP contract via `WebApplicationFactory<Program>`.       |
| **Total**                           | **64** |                                                         |

## Documents

- [User story](docs/user-story.md)
- [Architecture](docs/architecture.md)
- [Generative AI section](docs/genai-section.md) - required write-up on prompt, validation, improvements, and edge cases.
