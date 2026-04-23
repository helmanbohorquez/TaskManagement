# Generative AI Usage

This project was built with **Cursor** as the coding assistant. The section below documents the prompt that kicked off the backend, a representative sample of the AI output, and the critical-thinking loop that followed.

## 1. The prompt

Fed to the AI agent in Cursor (with plan-mode approval) after confirming the tech choices:

> **Context:** I have a .NET technical exercise attached (`dotnet_technical_exercise.md`). Build a full-stack task management app with:
> - ASP.NET Core Web API backend using Clean Architecture (Domain, Application, Infrastructure, Api projects) and TDD.
> - Data persistence in SQLite via raw ADO.NET (`Microsoft.Data.Sqlite`). **Do not use Entity Framework, Dapper, or MediatR.**
> - JWT bearer authentication with BCrypt password hashing. Expose `POST /api/auth/register`, `POST /api/auth/login`, `GET/POST/PUT/DELETE /api/tasks` (authorized, owner-scoped), and a public `GET /api/health`.
> - Task fields: `title`, `description`, `status` (`Pending`/`InProgress`/`Done`), `dueDate`, associated with a user.
> - xUnit + Moq + FluentAssertions test projects for every layer; API layer uses `WebApplicationFactory<Program>`.
> - A React + Vite (JavaScript) SPA with Tailwind, an `AuthContext`, an axios interceptor that attaches the JWT and redirects on 401, and pages for Login, Register, and Tasks (list + create/edit modal + status filter).
> - Seed a demo user `demo@tasks.test` / `Demo123!` with a few sample tasks on startup. Include README, user story, and an architecture diagram.

## 2. A representative output sample

A portion of what the AI generated for `src/TaskManagement.Api/Program.cs` (JWT + Swagger wiring):

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptsAccessor) =>
    {
        var jwt = jwtOptsAccessor.Value;
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub"
        };
    });
```

And for the ADO.NET `TaskRepository.ListByUserAsync`:

```csharp
cmd.CommandText = status.HasValue
    ? @"SELECT Id, UserId, Title, Description, Status, DueDate, CreatedAt, UpdatedAt
        FROM Tasks WHERE UserId = $userId AND Status = $status
        ORDER BY DueDate ASC;"
    : @"SELECT Id, UserId, Title, Description, Status, DueDate, CreatedAt, UpdatedAt
        FROM Tasks WHERE UserId = $userId
        ORDER BY DueDate ASC;";
cmd.Parameters.AddWithValue("$userId", userId.ToString());
if (status.HasValue)
    cmd.Parameters.AddWithValue("$status", (int)status.Value);
```

## 3. How I validated the AI output

- **Ran every layer's tests after generation.** 64 tests currently pass across the four test projects (`dotnet test TaskManagement.slnx`).
- **Read every line of SQL.** Confirmed every query is parameterized (no string concatenation) and every non-admin query includes `UserId = $userId` so users can only touch their own rows.
- **Manually exercised the API** via Swagger UI with the demo account, verifying: register, login, create/update/delete task, cross-user isolation, 401 on expired/missing token, 400 on invalid DTOs, 409 on duplicate emails.
- **Inspected the JWT** decoded payload to confirm `sub`, `email`, and `exp` claims.

## 4. Improvements I made over the first draft

| Area                  | Issue in initial draft                                                                 | Fix applied                                                                                                         |
| --------------------- | -------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| JWT options capture   | `Program.cs` read `JwtOptions` into a local variable at build time, so test overrides didn't reach `AddJwtBearer`. | Switched to the options pattern: `AddOptions<JwtBearerOptions>().Configure<IOptions<JwtOptions>>(...)` so values are resolved at request time. |
| Swashbuckle 10 API    | AI initially used the pre-10.x `OpenApiReference` / `ReferenceType.SecurityScheme` pattern, which no longer compiles. | Replaced with `AddSecurityRequirement(document => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>() })`. |
| Owner-scoping         | First draft allowed `GetById(taskId)` without user filter, leaking tasks across users. | Repository signatures now take `(Guid id, Guid userId)` so every query is scoped at the data-access layer.           |
| JSON enum round-trip  | API serialized `status` as string but tests deserialized with default options and threw `JsonException`. | Added `JsonStringEnumConverter` to the API and a shared `JsonOptions.Default` in the test project.                   |
| Password policy       | Initial validator only required `MinimumLength(6)`.                                    | Tightened to at least 8 chars with upper/lower/digit.                                                                |
| Email normalization   | `GetByEmailAsync` was case-sensitive, but `User.Create` lowercased. Breaking login for users who typed mixed case. | Added lowercase + trim inside `UserRepository.GetByEmailAsync` and in `AuthService` before the lookup.               |
| Demo-seed idempotency | Re-running startup duplicated the demo user.                                           | `SeedDemoDataAsync` checks for the demo email first; the operation is now idempotent.                                |
| Due-date timezone drift | `TaskForm.toDateInput` subtracted the local offset from a UTC-midnight ISO string, so edit forms showed the previous day for users west of UTC. Every save-without-change shifted the stored date one day earlier. `TaskCard.formatDate` also rendered the date in local time, so the card and the form disagreed. | `toDateInput` now reads the UTC date portion directly (`d.toISOString().slice(0, 10)`), and `formatDate` passes `timeZone: 'UTC'` to `toLocaleDateString`. Submit logic was already UTC-consistent, so card and form now agree and round-trips are stable. |
| Past-dated tasks      | `CreateTaskRequestValidator` and `UpdateTaskRequestValidator` only checked that `DueDate != default(DateTime)`, so the API happily accepted due dates in the past on both POST and PUT. | Added `.Must(d => d.Date >= DateTime.UtcNow.Date)` to both validators (comparing `.Date` so "today" is still allowed), plus a `min={todayInput()}` on the frontend date picker as a UX guard. Two new endpoint tests cover the create and update cases. |
| Stuck drag opacity on the board | After converting the tasks page to a Kanban-style board with HTML5 drag & drop, dropping a card in a new column left it rendered at `opacity-50`. The optimistic re-render unmounts the source card, so the browser's `dragend` event never fires on it and `draggingTaskId` never clears. | `handleMoveTask` now clears `draggingTaskId` synchronously at the top, before the optimistic state update that triggers the re-mount. The existing `onDragEnd` handler still covers drag-cancel paths (drop outside any column, Esc). |
| Expired status            | Past-due tasks that weren't Done had no visual distinction from other Pending/InProgress cards, so overdue work was easy to miss. The domain had no concept of "expired" at all. | Added `TaskItemStatus.Expired` as a **computed, presentation-only** value: never persisted, never settable via the API. `TaskResponse` gained an `IsExpired` flag (`Status != Done && DueDate.Date < UtcNow.Date`). Defense in depth on writes: `UpdateTaskRequestValidator` rejects `Status = Expired` (400) and `TaskItem.ChangeStatus` throws a `DomainException` if called with `Expired`. `TaskService.ListAsync` translates `?status=Expired` queries to an in-memory filter so the endpoint still works despite no rows storing that value. Frontend: `TaskCard` stacks a red "Expired" pill and paints the due-date line red when `task.isExpired` is true, keeping the card in its natural column on the 3-column board. `DbInitializer.SeedDemoDataAsync` now seeds one past-due Pending task so the demo account shows the badge out of the box. |

## 5. Edge cases deliberately covered

- **Authentication**
  - Missing `Authorization` header -> 401.
  - Expired or malformed JWT -> 401 via `ValidateLifetime`/`ValidateIssuerSigningKey`.
  - Tokens issued by a different signing key (e.g. other environment) -> 401.
  - `sub` claim missing -> `GetUserId` throws, middleware -> 401 path.

- **Validation**
  - Empty or whitespace-only title -> 400 (`ValidationException` on DTO + domain guard).
  - Title > 200 chars, description > 2000 chars -> 400 at both validator and domain levels.
  - Missing/unknown `status` value on update -> 400 via `IsInEnum`.
  - Password missing upper/lower/digit or < 8 chars -> 400 with per-rule messages.
  - Email without `@` or TLD -> 400.

- **Authorization / data isolation**
  - Bob's GET of Alice's task id -> 404 (not 403, to avoid leaking existence).
  - Bob's DELETE of Alice's task -> 404, no rows affected.
  - List endpoint always filtered by the caller's `userId`.

- **Error handling**
  - Duplicate email on register -> 409 Conflict.
  - Wrong password on login -> 401 (same shape as unknown email to resist user-enumeration).
  - Unhandled exceptions -> 500 with a generic message; full detail only in server logs.

## 6. Where I chose to trust the AI

- Boilerplate (csproj references, using directives, test fixture skeletons).
- Obvious CRUD SQL once I verified the parameterization pattern.
- Tailwind class names and responsive grid layout in the frontend.

## 7. Where I explicitly did not trust the AI

- Anything touching security (hash cost, JWT validation parameters, claim extraction).
- SQL parameterization: I re-read every `CommandText` block.
- Public surface area of the API: I cross-checked routes and status codes against the exercise requirements.
