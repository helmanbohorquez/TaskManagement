# Architecture

## Goals

- Clean Architecture with strict dependency rules.
- No EF, no Dapper, no MediatR (explicit constraints from the exercise).
- Test-driven: layer-by-layer tests written before/alongside implementation.
- Stateless JWT authentication between SPA and API.

## Layer diagram

```
+--------------------+      +----------------+
|  React SPA (Vite)  | <--> |  Web API       |
|  JWT in localStorage|     |  Controllers   |
+--------------------+      +--------+-------+
                                     |
                                     v
                              +--------------+
                              | Application  |  services, DTOs, validators, ports
                              +------+-------+
                                     |
                        +------------+------------+
                        |                         |
                        v                         v
                 +------------+            +--------------+
                 |   Domain   |            | Infrastructure|
                 |  entities  |<-----------+  ADO.NET,     |
                 |  rules     |            |  Bcrypt, JWT  |
                 +------------+            +-------+-------+
                                                   |
                                                   v
                                            +--------------+
                                            | SQLite file  |
                                            +--------------+
```

## Dependency rule

- `TaskManagement.Domain` depends on nothing.
- `TaskManagement.Application` depends only on `Domain`.
- `TaskManagement.Infrastructure` depends on `Application` and `Domain` (implements the ports).
- `TaskManagement.Api` depends on `Application` and `Infrastructure` (wiring + HTTP).

Controllers never talk to repositories directly; they go through application services. Repositories never reach into HTTP concepts.

## Key components

| Layer           | Responsibilities                                                                 |
| --------------- | -------------------------------------------------------------------------------- |
| Domain          | `TaskItem`, `User`, `TaskItemStatus`, domain exceptions, repo port interfaces.   |
| Application     | `TaskService`, `AuthService`, DTOs, FluentValidation validators, security ports. |
| Infrastructure  | SQLite ADO.NET repositories, `DbInitializer`, `BcryptPasswordHasher`, `JwtTokenService`. |
| API             | Controllers, `ExceptionHandlingMiddleware`, JWT bearer + Swagger, CORS.          |

## Authentication flow

1. Client calls `POST /api/auth/register` or `/login` and receives `{ token, expiresAt, email }`.
2. Client stores the JWT in `localStorage` and an axios interceptor attaches `Authorization: Bearer <token>` to every subsequent request.
3. The API validates the signature, issuer, audience, and lifetime; the `sub` claim carries the user id.
4. `TasksController` extracts `sub`, converts it to `Guid`, and passes it through the service layer so every DB query is scoped to that user.

## Why SQLite + ADO.NET?

- Zero install, great for demos and CI.
- Forces us to write explicit, parameterized SQL, which satisfies the "no EF / no Dapper" constraint and showcases raw data-access skills.
- A dedicated `ISqliteConnectionFactory` keeps connection-string handling in one place and is trivial to mock or point at a temp file in tests.

## Validation & error handling

- FluentValidation in the Application layer validates DTOs and throws `ValidationException`.
- Domain guard clauses throw `DomainException` for invariants the DTO layer cannot express (e.g. empty `UserId`).
- `ExceptionHandlingMiddleware` maps each exception type to an HTTP status code + JSON body (`{ code, message }`).

## Testing strategy

| Project                                   | What it exercises                                                   |
| ----------------------------------------- | ------------------------------------------------------------------- |
| `TaskManagement.Domain.Tests`             | Entity invariants, value transitions.                               |
| `TaskManagement.Application.Tests`        | Service logic with mocked repos + real validators (xUnit + Moq).    |
| `TaskManagement.Infrastructure.Tests`     | Real SQLite file per fixture: SQL correctness, hasher, JWT service. |
| `TaskManagement.Api.Tests`                | `WebApplicationFactory<Program>` end-to-end HTTP tests.             |
