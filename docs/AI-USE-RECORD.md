# AI Prompt and Usage Record

> Complete or correct this record before assessment submission. It distinguishes work proposed or generated with AI from verification performed independently.

## Prompt used

The implementation request asked for a minimal .NET 8 course registration assessment using EF Core InMemory, deterministic dummy data, structured `ILogger<T>` logging, enrollment/drop/FIFO promotion, prerequisite checks, xUnit tests, and design/setup/AI-use documentation. It specified the policy to skip ineligible waitlisted learners during promotion but retain their entries, and asked for honest reporting of test execution.

## AI-assisted contribution summary

- Defined entities for one offering, learners, active enrollments, and waitlist entries.
- Implemented service operations and explicit result statuses.
- Used sequence numbers for FIFO order and rechecked prerequisite eligibility during promotion.
- Added structured logging, a dummy-data seeder, a console demo, and xUnit tests with unique InMemory database names.
- Drafted design, setup, limitations, and verification documentation.

## Decisions requiring human review

| Decision | Current choice | Review / evidence needed |
|---|---|---|
| Storage | EF Core InMemory | Confirm this matches the assessment's permitted prototype scope |
| Ineligible queued learner | Skip for promotion, retain in queue | Run the corresponding test |
| Concurrency | Process-local lock on service instance | Confirm scope; this does not model database/distributed concurrency |
| Prerequisites | Boolean `HasPrerequisite` | Replace only if the supplied starter model provides a richer representation |
| Tests | xUnit with unique database per test | Execute tests and record actual output |

## Actual verification status

The current authoring environment does not have the `dotnet` executable installed. Therefore, `dotnet restore`, `dotnet build`, `dotnet test`, and the demo have **not** been executed here. No successful build or test result is claimed.

Before submission, run the commands in `README.md`, paste or summarize the actual output below, review the implementation against the original starter project if one exists, and record any edits or failures.

- SDK version:
- Restore command/result:
- Build command/result:
- Test command/result:
- Demo command/result:
- Human review findings and changes:


### Follow-up change: model-based seed data
- Requested change: define dummy records using `Entity.HasData` inside `RegistrationDbContext` and keep `Program.cs` free of individual seed data.
- Change made: configured the course offering and learners with `HasData`; removed `DummyDataSeeder`; changed `Program.cs` to call `Database.EnsureCreatedAsync()`; updated tests to call `EnsureCreated()` on each isolated InMemory database.
- Verification: source changes reviewed. Build and test execution must be recorded only after running the commands in an environment with the .NET 8 SDK.
