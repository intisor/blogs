---
Title: I Built the Same API Endpoint Twice. Here's What I Learned.
Description: A direct comparison of the same ASP.NET Core endpoint built with Minimal API (LINQ) and FastEndpoints (raw SQL) — and what having both versions side-by-side revealed about how frameworks shape the decisions you make inside them.
Published: 2026-05-05
Tags:
  - ASP.NET Core
  - FastEndpoints
  - Minimal API
  - Architecture
Author: Abdulawwal Intisor
---

At some point during this project, I had two folders doing the same thing.

`Endpoints/`. `FastEndpoints/`. Same database. Same response DTOs. Same data going in and out. Two completely different ways of thinking about the same problem.

That's not a bug in my project structure. That's a learning scar. And it's worth writing about.

---

## Some Context First

The project is an API for AMSA Nigeria — the Ahmadiyya Muslim Students' Association. The system manages members, units, states, nationals, departments, and executive roles across a three-tier organizational hierarchy: National → State → Unit. Think of it as a mini organizational ERP: query who belongs to which unit, who holds which EXCO position at what level, import members from CSV, generate org-wide statistics.

Real system. Real data. Real people will use it.

The stack is ASP.NET Core with EF Core and SQL Server. Started with [Minimal API](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview) because it felt modern and lightweight — no controllers, no ceremony, just functions mapped to routes. Then discovered [FastEndpoints](https://fast-endpoints.com/docs/get-started) and started migrating.

Here's what didn't happen: an immediate clean rewrite. The old endpoints stayed. The new ones went in alongside them. For a while, both lived in the same codebase simultaneously.

That accident became one of the most useful things that happened during this build.

Because with both versions present, a direct comparison became unavoidable. Same endpoint. Same query logic. Same output. Two completely different implementations sitting next to each other, forced into contrast.

The endpoint used as the example throughout this post is `GET /api/units/{id}` — get a unit by ID, with its members and EXCO roles. It's a good candidate because it's not trivial. It has input validation, a primary query, two secondary queries, a composed response object, and a not-found case. Enough moving parts to show real differences.

---

## Version 1: Minimal API with LINQ

Here's the Minimal API version, living in `OrganizationEndpoints.cs`:

```csharp
private static async Task<IResult> GetUnitById(int id, AmsaDbContext db)
{
    try
    {
        var unit = await db.Units
            .AsNoTracking()
            .Where(u => u.UnitId == id)
            .Select(u => new UnitDetailDto
            {
                UnitId = u.UnitId,
                UnitName = u.UnitName,
                StateId = u.StateId,
                StateName = u.State.StateName,
                NationalId = u.State.NationalId,
                NationalName = u.State.National.NationalName,
                MemberCount = u.Members.Count()
            })
            .FirstOrDefaultAsync();

        if (unit == null)
            return Results.NotFound($"Unit with ID {id} not found");

        var members = await db.Members
            .AsNoTracking()
            .Where(m => m.UnitId == id)
            .Select(m => new UnitMemberDto
            {
                MemberId = m.MemberId,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.Email,
                Phone = m.Phone,
                Mkanid = m.Mkanid
            })
            .OrderBy(m => m.FirstName)
            .ToListAsync();

        var excoRoles = await db.MemberLevelDepartments
            .AsNoTracking()
            .Where(mld => mld.LevelDepartment.Level.UnitId == id)
            .Select(mld => new UnitExcoDto
            {
                FirstName = mld.Member.FirstName,
                LastName = mld.Member.LastName,
                Mkanid = mld.Member.Mkanid,
                DepartmentName = mld.LevelDepartment.Department.DepartmentName,
                LevelType = mld.LevelDepartment.Level.LevelType
            })
            .ToListAsync();

        return Results.Ok(new UnitDetailResponse
        {
            Unit = unit,
            Members = members,
            ExcoRoles = excoRoles
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving unit: {ex.Message}");
    }
}
```

Registered like this, at the top of the file:

```csharp
unitGroup.MapGet("/{id:int}", GetUnitById);
```

This is genuinely pleasant to read. The navigation properties — `u.State.StateName`, `u.State.National.NationalName`, `mld.LevelDepartment.Level.UnitId` — read almost like English sentences. There's no SQL to write. No JOIN clauses to maintain. The object graph does the talking and EF Core translates it into optimized queries behind the scenes.

This is exactly what [EF Core LINQ queries](https://learn.microsoft.com/en-us/ef/core/querying/) are built for. The [`Select` projection](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager) means only the required columns are fetched — not entire entity rows hydrated into memory. [`AsNoTracking()`](https://learn.microsoft.com/en-us/ef/core/querying/tracking) tells EF Core to skip the change tracker entirely since this is a read-only path, which is a meaningful performance win for endpoints that never write.

The whole handler fits in one mental unit. Read it once, top to bottom, and the intent is clear.

But notice what's absent: there's no validation on `id`. If someone passes `0` or `-99`, the query runs. It returns a 404 because nothing matches — which is technically correct but not *intentionally* correct. The right answer was arrived at by coincidence, not design.

Also notice the `try/catch` wrapping everything. Every single Minimal API handler in this project has that same block. Every one. Copy-pasted about twelve times across the codebase. That should tell you something about what Minimal API leaves up to you.

---

## Version 2: FastEndpoints with Raw SQL

Here's the same endpoint rebuilt in `OrganizationFastEndpoints.cs`:

```csharp
// Private file-scoped record for SQL projection
file record UnitDetailRawDto(
    int UnitId, string UnitName,
    int StateId, string StateName,
    int NationalId, string NationalName,
    int MemberCount
);

public sealed class GetUnitByIdEndpoint(AmsaDbContext db)
    : Endpoint<GetUnitByIdRequest, UnitDetailResponse>
{
    public override void Configure()
    {
        Get("/api/units/{id}");
        AllowAnonymous();
        Summary(s => s.Summary = "Get unit details with members and EXCO roles");
    }

    public override async Task HandleAsync(GetUnitByIdRequest req, CancellationToken ct)
    {
        var validationResult = OrganizationValidationMethods.ValidateUnitRequest(req);
        if (!validationResult.IsSuccess)
        {
            await Send.ResultAsync(Results.BadRequest(validationResult.ErrorMessage));
            return;
        }

        var unitRaw = await db.Database.SqlQueryRaw<UnitDetailRawDto>("""
            SELECT u.UnitId, u.UnitName, s.StateId, s.StateName,
                   n.NationalId, n.NationalName,
                   COUNT(DISTINCT m.MemberId) as MemberCount
            FROM Units u
            INNER JOIN States s ON u.StateId = s.StateId
            INNER JOIN National n ON s.NationalId = n.NationalId
            LEFT JOIN Members m ON u.UnitId = m.UnitId
            WHERE u.UnitId = {0}
            GROUP BY u.UnitId, u.UnitName, s.StateId, s.StateName,
                     n.NationalId, n.NationalName
            """, req.Id).FirstOrDefaultAsync(ct);

        if (unitRaw == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var members = await db.Database.SqlQueryRaw<UnitMemberDto>("""
            SELECT MemberId, FirstName, LastName, Email, Phone, Mkanid
            FROM Members
            WHERE UnitId = {0}
            ORDER BY FirstName, LastName
            """, req.Id).ToListAsync(ct);

        var excoRoles = await db.Database.SqlQueryRaw<UnitExcoDto>("""
            SELECT m.FirstName, m.LastName, m.Mkanid,
                   d.DepartmentName, lv.LevelType
            FROM Members m
            INNER JOIN MemberLevelDepartments mld ON m.MemberId = mld.MemberId
            INNER JOIN LevelDepartments ld ON mld.LevelDepartmentId = ld.LevelDepartmentId
            INNER JOIN Departments d ON ld.DepartmentId = d.DepartmentId
            INNER JOIN Levels lv ON ld.LevelId = lv.LevelId
            WHERE lv.UnitId = {0}
            ORDER BY d.DepartmentName
            """, req.Id).ToListAsync(ct);

        await Send.OkAsync(new UnitDetailResponse
        {
            Unit = new UnitDetailDto
            {
                UnitId = unitRaw.UnitId,
                UnitName = unitRaw.UnitName,
                StateId = unitRaw.StateId,
                StateName = unitRaw.StateName,
                NationalId = unitRaw.NationalId,
                NationalName = unitRaw.NationalName,
                MemberCount = unitRaw.MemberCount
            },
            Members = members,
            ExcoRoles = excoRoles
        }, ct);
    }
}
```

And the validation method it calls:

```csharp
public static class OrganizationValidationMethods
{
    public static Result<bool> ValidateUnitRequest(GetUnitByIdRequest req)
    {
        if (req.Id <= 0)
            return Result.Validation<bool>("Invalid unit ID. ID must be greater than 0.");

        return Result.Success(true);
    }
}
```

More code than the LINQ version. Deliberately so.

Writing raw SQL here is an explicit statement: *this is exactly what query hits the database.* No ORM translation layer. No inference. The `GROUP BY`, the `COUNT(DISTINCT)`, the JOIN order — all of it is specified and visible. For complex aggregations or performance-sensitive paths, that visibility matters. [EF Core's query translator is good](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries) but it's not omniscient, and sometimes you need to be the one writing the execution plan.

The `{0}` parameter syntax in `SqlQueryRaw` is worth understanding. Despite looking like string formatting, these are [proper parameterized queries](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries#passing-parameters) — EF Core passes them as SQL parameters under the hood, not interpolated strings. SQL injection is not a concern here. The shape of the query is written in code; the values are passed safely at runtime.

The `file record UnitDetailRawDto` at the top uses the `file` access modifier, which scopes the type to this single file. The rest of the project cannot see it. The public contract of this endpoint is `UnitDetailResponse`. How that response was assembled — what intermediate DTOs were used, what SQL was written — is an implementation detail that belongs to this file and nowhere else. This is [vertical slice architecture](https://jimmybogard.com/vertical-slice-architecture/) expressed at the type system level: each endpoint owns its full stack and exposes only its contract.

---

## Why the Query Approach Flipped

Minimal API uses LINQ. FastEndpoints uses raw SQL. That wasn't arbitrary.

Minimal API's promise is that simple things should stay simple. [LINQ projections](https://learn.microsoft.com/en-us/ef/core/querying/) are the natural language of EF Core — readable, safe, and well-optimized for standard data access patterns. Reaching for raw SQL inside a Minimal API handler would introduce friction that the framework never asked for. It would work, but it would feel like the wrong tool.

FastEndpoints operates at a different level of intentionality. By the time a handler is written inside a FastEndpoints endpoint, a request type has been defined, a response type has been declared, explicit validation has been wired up. The whole setup communicates: structure is worth the investment here. In that context, reaching for raw SQL to get precise control over a complex query is *consistent* with the philosophy — not contradictory to it. Deliberate query authorship fits naturally alongside deliberate type design.

This is something framework documentation rarely says out loud: the framework chosen shapes the *kind of decisions* made inside it. Minimal API nudges toward simplicity at every decision point. FastEndpoints nudges toward explicitness. Neither nudge is universally correct. But they're real, and they accumulate.

---

## What FastEndpoints Forces You to Think About

### 1. The Request is a Real Object

In Minimal API, `id` is a primitive `int` in a method signature. It exists, it has a value, and that's all.

In FastEndpoints, the request is `GetUnitByIdRequest`:

```csharp
public class GetUnitByIdRequest
{
    public int Id { get; set; }
}
```

Still simple — just wrapping an int. But it's a *seam*. When validation needs to be added, it validates the object. When authorization context is needed, it lives on the object. When the request needs to be logged or traced, the object is logged. The request has an identity.

And crucially, validation is now a testable, isolated method call:

```csharp
var validationResult = OrganizationValidationMethods.ValidateUnitRequest(req);
```

That method can be unit tested without spinning up a web server, without a database, without any infrastructure at all. In the Minimal API version, extracting validation to that level requires deliberate effort that the framework doesn't push toward. In FastEndpoints, it's the natural path. See [FastEndpoints request binding docs](https://fast-endpoints.com/docs/requests) for how the framework handles this.

### 2. The Response Contract is Declared Upfront

```csharp
public sealed class GetUnitByIdEndpoint(AmsaDbContext db)
    : Endpoint<GetUnitByIdRequest, UnitDetailResponse>
```

Before reading a single line of the handler, anyone looking at this class knows what goes in and what comes out. FastEndpoints uses these generic type parameters to generate Swagger documentation automatically. The endpoint *is* its contract.

Minimal API's equivalent return type is `Task<IResult>`. That tells the reader nothing. The contract only becomes visible by reading the entire handler.

### 3. No More Copy-Pasted Error Handling

FastEndpoints supports a [global exception handler](https://fast-endpoints.com/docs/exception-handler) configured once at startup. Every endpoint benefits automatically. There is no per-handler `try/catch`. There is no risk of forgetting to add it on one endpoint and getting a raw stack trace in production. The infrastructure concern is handled at the infrastructure level, not scattered across every handler.

This single difference has compounding effects on codebase maintainability that are easy to underestimate until you've actually had to chase down an unhandled exception that slipped through because one handler out of fifteen was missing its catch block.

### 4. The Send API Makes Intent Explicit

```csharp
await Send.OkAsync(response, ct);
await Send.NotFoundAsync(ct);
await Send.ResultAsync(Results.BadRequest(validationResult.ErrorMessage));
```

Compare to:

```csharp
return Results.Ok(response);
return Results.NotFound(...);
return Results.Problem(...);
```

Both work. But `Send.*` communicates that a response is being *sent down an active connection*, not just returned from a function. It's a small shift in framing, but it reflects the underlying reality of HTTP more accurately. See [FastEndpoints endpoint configuration](https://fast-endpoints.com/docs/configuration-settings) for the full Send API surface.

---

## What Minimal API Does Better

This is not a post that concludes FastEndpoints won and everything should be rewritten. That would be dishonest.

**Minimal API is genuinely faster to read at a glance.** Open `OrganizationEndpoints.cs` and a single handler can be fully understood in about twenty seconds. No base classes to know about. No framework conventions to learn. No type parameters to decode. A function that takes a db context and an id and returns a result — the most legible possible description of what an endpoint does.

For small projects, solo developers, internal tools, and prototypes, that legibility is the right priority. The mental overhead of FastEndpoints pays off over time, at scale, across teams. At the start of a project, or when requirements are shifting fast, the ceremony can slow down more than it helps.

**Minimal API is also closer to the metal of ASP.NET Core.** Everything it does is built on top of primitives that ship with the framework — [route handlers](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers), `IResult`, dependency injection via method parameters. FastEndpoints is a library with conventions and abstractions layered on top. That's a dependency. For long-lived projects that's an acceptable and worthwhile dependency; for short-lived ones it may not be.

The [official error handling documentation for Minimal API](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling) covers how to configure global exception handling there too — it's possible, just not the path of least resistance the way it is in FastEndpoints.

---

## The Real Lesson

When the Minimal API version of an endpoint was being written, the question being answered was: *what does this endpoint do?*

When the FastEndpoints version was being written, the question was: *what is this endpoint?*

That's a subtle distinction but it matters in practice. The first framing produces procedural code — a sequence of steps that accomplishes a task. The second produces structured code — a thing with an identity, an input contract, an output contract, and behavior. The first is easier to write initially. The second is easier to reason about as the codebase grows.

Neither framing is universally correct. But being able to recognize which question is being asked — and choose deliberately which one to answer — is the difference between someone who writes code and someone who designs systems.

That realization didn't come from reading documentation. It came from having a messy folder structure for two weeks.

---

## Where This Codebase Goes From Here

The Minimal API endpoints are not being deleted. They work, they're tested, and removing working code for the sake of consistency is a bad trade. But every new endpoint goes into FastEndpoints, and as existing ones need changes, they get migrated one at a time.

That's just software. You don't rewrite — you migrate. Slowly, intentionally, with tests you probably should have written earlier.

---

If the FastEndpoints library isn't on your radar yet, the [getting started guide](https://fast-endpoints.com/docs/get-started) is worth a few hours. It won't change how you think about everything. But it will make you ask better questions about the structure of the code you're already writing, and that compounds.

And if the best learning you've done recently came from an accident — from a messy project, a bad decision that turned into a comparison, a folder structure that got out of hand — that's not a failure of process. That's just how real understanding gets built.

---

*All code in this post is from a real production project. The full implementation is available on [GitHub](https://github.com).*
