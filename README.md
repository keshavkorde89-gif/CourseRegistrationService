# Course Registration — .NET 10 + EF Core InMemory

A .NET 10 ASP.NET Core API for enrollment, drops, and FIFO waitlist promotion. It uses EF Core's InMemory provider and deterministic `HasData()` seed data; it is a prototype, not production persistence.

## Requirements

- .NET 10 SDK
- Network access on first run to restore NuGet packages

## Run from the solution directory

```bash
dotnet restore CourseRegistration.sln
dotnet build CourseRegistration.sln
dotnet test CourseRegistration.sln
dotnet run --project src/CourseRegistration/CourseRegistration.csproj
```

The API creates an in-memory database named `CourseRegistration`; `HasData()` seeds one course with capacity 2 and five learners. The database is recreated when the process restarts. Requests are documented in [`src/CourseRegistration/CourseRegistration.http`](src/CourseRegistration/CourseRegistration.http).

## API requests

The default offering has ID 1. Update `@baseUrl` in the `.http` file if the server prints a different address.

### Enroll or join the waitlist

`POST /api/courseregistration/enroll`

```http
POST http://localhost:5000/api/courseregistration/enroll
Content-Type: application/json

{
  "learnerId": 1
}
```

Returns `201 Created` when the learner is enrolled or added to the waitlist, `400 Bad Request` when the prerequisite is not met or the learner ID is invalid, `404 Not Found` for an unknown learner, and `409 Conflict` for a duplicate request.

### Drop an enrollment

`DELETE /api/courseregistration/{offeringId}/learners/{learnerId}`

```http
DELETE http://localhost:5000/api/courseregistration/1/learners/1
```

Returns `200 OK` when the enrollment is dropped and FIFO promotion is processed, `404 Not Found` for an unknown offering or learner, and `409 Conflict` when the learner is not enrolled.

### Get offering enrollment and waitlist details

`GET /api/courseregistration/{offeringId}`

```http
GET http://localhost:5000/api/courseregistration/1
```

Returns `200 OK` with the capacity, enrolled learner IDs, and waitlisted learner IDs in FIFO order, or `404 Not Found` for an unknown offering.

## Project structure

- `src/CourseRegistration/RegistrationDbContext.cs` — EF Core entities and model configuration.
- `RegistrationDbContext.OnModelCreating` — deterministic dummy data configured with `HasData`.
- `src/CourseRegistration/CourseRegistrationService.cs` — business rules, promotion, and structured logging.
- `src/CourseRegistration/Controllers/CourseRegistrationController.cs` — HTTP endpoints.
- `src/CourseRegistration/Program.cs` — ASP.NET Core hosting and DI setup.
- `tests/CourseRegistration.Tests/` — xUnit tests; each test uses a unique EF InMemory database.
- `docs/DESIGN.md` — workflow, invariants, and trade-offs.
- `docs/AI-USE-RECORD.md` — AI assistance and review checklist.
- `docs/VERIFICATION.md` — test mapping and results to fill in.

## Business rules

- Only known learners who satisfy the prerequisite may enroll or join the waitlist.
- Existing enrollment and waitlist requests are not duplicated.
- Waitlist order uses an increasing sequence number.
- On drop, the first currently eligible learner is promoted. Ineligible entries are skipped but retained in the queue.
- Invalid or repeated drops do not release a seat.
- `ILogger<CourseRegistrationService>` emits structured messages for important accepted and rejected operations.

## Sample workflow

With capacity 2, enroll learners 1 and 2, then learner 3 joins the waitlist. Dropping learner 1 promotes learner 3. Learner 5 is rejected because the prerequisite is not satisfied.

## Limitations

- EF Core InMemory does not behave like a relational database and does not validate relational transactions, unique constraints, or SQL Server locking semantics.
- The process-local `lock` protects calls through the same service instance only. It is not distributed locking and does not protect separate service instances/processes.
- The indexes configured in EF model metadata are not proof of uniqueness enforcement in the InMemory provider.
- Data is volatile and resets on restart. A production system needs a relational persistence strategy and appropriate transaction/concurrency design.

## Verification honesty

Actual build and test results are recorded in [`docs/VERIFICATION.md`](docs/VERIFICATION.md).
