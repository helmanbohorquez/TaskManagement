# Approach & thought process

This document narrates how I approached the exercise — how I read the spec, the decisions I made along the way, the trade-offs I accepted, and what I would change with more time. It is deliberately chronological: I want the reader to follow my reasoning, not just see the finished product.

For the *what* (final architecture, diagrams, test layout) see [`architecture.md`](architecture.md). For the AI-usage write-up the exercise asked for, see [`genai-section.md`](genai-section.md).

## 1. How I read the spec

Before writing any code I made a single pass over the exercise document and extracted three separate lists:

- **Hard constraints** (non-negotiable): ASP.NET Core Web API, Clean Architecture, TDD, **no EF / no Dapper / no MediatR**, JWT authentication, a React front-end, tasks scoped per user, unit + integration tests, a GenAI section in the docs.
- **Soft requirements** (defaults the spec hinted at but did not pin down): lightweight storage, demo-friendly setup, responsive UI.
- **Nice-to-haves**: I chose not to spend time here because the core surface area was already large. Candidates: refresh tokens, pagination, role-based admin, full CI pipeline, observability.

Calling out the constraints explicitly up front kept me honest later when I was tempted to reach for EF "just for speed" — I'd already written it down as a hard no.

## 2. Key decisions and trade-offs

### 2.1 SQLite + raw ADO.NET

SQLite is zero-install, works identically on any developer machine, and survives a fresh `dotnet run`. Raw ADO.NET (`Microsoft.Data.Sqlite`) was forced by the "no EF / no Dapper" constraint, which I actually appreciated: it meant I had to write explicit parameterized SQL, handle connection lifetimes by hand, and centralize schema creation in a `DbInitializer`. The cost is more boilerplate in the repositories; the benefit is that there is nothing "magic" hiding the data layer.

Trade-off accepted: no query composition helpers, no change tracking, no migrations framework. For a production system with a real schema I would switch to EF Core the moment the repository code started feeling repetitive.

### 2.2 Clean Architecture with four projects

I split the solution into `Domain`, `Application`, `Infrastructure`, and `Api` because the exercise explicitly asked for Clean Architecture and because it makes TDD per layer natural: each project has its own xUnit test project with a matching name. The dependency rules are strict: `Domain` depends on nothing; `Application` depends only on `Domain`; `Infrastructure` and `Api` sit at the edges.

Trade-off accepted: more projects means more boilerplate and more `csproj` wiring. For an app this size a single project would build faster, but I would lose the architectural guard rails and the ability to tell a reviewer, "open `Domain` — it has no dependencies, so the business rules are impossible to accidentally couple to SQLite."

### 2.3 No mediator, no CQRS

MediatR is banned by the spec, and I did not try to hand-roll a pipeline to replace it. Each controller calls an application service directly (`ITaskService`, `IAuthService`). Services are thin orchestrators that validate input, call the repository port, and map results to DTOs.

### 2.4 JWT with BCrypt, stored in `localStorage`

I picked symmetric JWTs (HS256) because the spec called for a single API + single SPA with no external identity provider. BCrypt for passwords is the boring, correct default. The token lives in `localStorage` on the client — not ideal against XSS, but acceptable for an exercise and easy to swap for `httpOnly` cookies later. An axios interceptor attaches the token to every request and redirects to `/login` on 401, which keeps auth concerns out of the pages.

## 3. TDD workflow

I wrote tests **in the order the dependency arrows point**, so every layer was pinned down before I touched the one above it:

1. `Domain.Tests` — entity invariants (`User`, `TaskItem`, status transitions, guard clauses).
2. `Application.Tests` — services with mocked repositories and real FluentValidation validators.
3. `Infrastructure.Tests` — real SQLite file per fixture to prove the SQL actually works, plus BCrypt round-trips and JWT issuance/validation.
4. `Api.Tests` — `WebApplicationFactory<Program>` black-box tests that drive the real HTTP pipeline with in-memory config overrides.

This ordering meant that by the time I wired controllers I was composing parts I already trusted. It also gave me a concrete "done" signal at each layer: if the tests were green, the layer was complete.

Final count: **64 passing tests** across the four projects.

## 4. Problems I hit (and what they taught me)

These are the non-trivial issues I ran into — none of them are in the final code, but the process of debugging them is part of the thought process the exercise asked about.

- **JWT options captured at build time vs. test overrides.** My first version of `Program.cs` captured `JwtOptions` into a local variable before calling `AddJwtBearer`. That worked for `dotnet run` but broke in `Api.Tests` because `WebApplicationFactory`'s config overrides arrived too late. The fix was to bind options dynamically via `AddOptions<JwtBearerOptions>().Configure<IOptions<JwtOptions>>(...)`, which resolves the latest configuration at runtime. Lesson: when an option is configuration-driven, prefer `IOptions<T>` over a captured snapshot.
- **`NameClaimType` mismatch.** JWT's standard subject claim is `sub`, but ASP.NET Core's default `NameClaimType` is `ClaimTypes.NameIdentifier`. My controllers read `User.FindFirst("sub")`, so I set `NameClaimType = "sub"` on the token validation parameters. Without that the tests returned 401s that looked like a signing problem but were actually a claim-name problem.
- **JSON enum serialization between API and tests.** The API registered `JsonStringEnumConverter` via `AddControllers().AddJsonOptions(...)`, but the test client's default `ReadFromJsonAsync<T>()` did not use the same options. I created a shared `JsonOptions.Default` in the test project and passed it explicitly. Lesson: serialization settings are part of the contract; share them.
- **Swashbuckle 10 breaking changes.** The `OpenApiSecurityScheme.Reference` API is gone in the Microsoft.OpenApi 2.x flattening. Swashbuckle now expects `new OpenApiSecuritySchemeReference("Bearer", document)`. A quick search, a syntax update, and the "Authorize" button in Swagger worked.
- **`.sln` vs `.slnx`.** `dotnet new sln` in .NET 10 produces `.slnx`, not `.sln`. I adjusted the commands and the CI mental model accordingly.

## 5. What I would do with more time

Honest self-audit — things I know are not there:

- **Refresh tokens.** The current JWT lives for 120 minutes and then the user re-authenticates. For a real product I would add short-lived access tokens plus long-lived refresh tokens in `httpOnly` cookies.
- **Pagination and sorting.** `GET /api/tasks` returns everything the user owns. Fine for a demo, wrong for a busy user with hundreds of tasks.
- **Server-side overdue detection.** I added an `Expired` status late in the process. A background job (or a computed field on read) should transition `Pending` tasks whose `dueDate` has passed. Right now `Expired` can only be set manually.
- **Global search / full-text.** SQLite's FTS5 would fit well, and the repository pattern isolates the change to one layer.
- **Observability.** Serilog with structured logs + a `/metrics` endpoint would make the API production-visible.
- **CI pipeline.** A GitHub Actions workflow that runs `dotnet test` and `npm run build` on every push would give the reviewer a green check next to the latest commit. I left it out of this submission to keep the scope tight, not because I don't value it.
- **Stronger frontend testing.** The SPA has no unit tests. React Testing Library on the Tasks page would catch regressions in the filter / modal flow.

## 6. How I used AI

Covered in detail in [`genai-section.md`](genai-section.md). Short version: I used Cursor as a pair-programmer, but every piece of generated code went through the test suite, a read-through, and — in several cases — a refactor. The JWT-options and `NameClaimType` problems above came from treating AI output as a draft rather than a final answer.
