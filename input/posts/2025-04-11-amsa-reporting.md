---
Title: How I Built a Multi-Tenant Reporting System for 500+ AMSA Branches
Description: A deep dive into the architecture behind AMSA Nigeria's reporting system — cross-database queries, JWT role hierarchies, and the decisions that made it work.
Published: 2025-04-11
Tags:
  - ASP.NET Core
  - Architecture
  - Multi-tenancy
  - SQL Server
Author: Abdulawwal Intisor
---

Most reporting systems are boring. One database, one tenant, one set of users. The AMSA Nigeria reporting system had to work across **National → State → Unit** hierarchy — meaning data from hundreds of branches, different access levels per role, and reports that had to aggregate upward cleanly.

Here's how I built it.

## The Problem

AMSA Nigeria has branches at three levels:
- **National** — sees everything
- **State** — sees their state and all units below
- **Unit** — sees only their own data

A simple single-tenant system wouldn't cut it. Every query had to be scope-aware.

## The Database Decision

I separated concerns into two databases:

```sql
AmsaAuthDB     -- users, roles, JWT tokens
AmsaReportDB   -- all report data, scoped by branch
```

Cross-database queries in SQL Server are clean with linked servers or just `USE` switching in EF Core. The key insight: **auth and data should never live in the same database for a multi-tenant system** at this scale.

## JWT Role Hierarchy

The token payload carries both the user's role and their branch code:

```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim("role", user.Role),           // National | State | Unit
    new Claim("branchCode", user.BranchCode) // NG | NG-OY | NG-OY-001
};
```

Every API endpoint extracts `branchCode` from the token and scopes the query accordingly. National users get `WHERE BranchCode LIKE 'NG%'`. State users get `WHERE BranchCode LIKE 'NG-OY%'`. Unit users get exact match.

## The Report State Machine

Reports go through states: `Draft → Submitted → Reviewed → Approved`. Each transition is guarded:

```csharp
public class ReportStateMachine
{
    public static bool CanTransition(ReportStatus from, ReportStatus to, string role)
    {
        return (from, to, role) switch
        {
            (Draft, Submitted, "Unit") => true,
            (Submitted, Reviewed, "State") => true,
            (Reviewed, Approved, "National") => true,
            _ => false
        };
    }
}
```

Clean, testable, no spaghetti conditionals.

## What I Learned

Building multi-tenant systems in Nigeria specifically means accounting for intermittent connectivity. I added aggressive caching at the state level so a bad connection doesn't kill a report submission mid-way.

The full source isn't public yet but the architecture is replicable for any hierarchical org system.
