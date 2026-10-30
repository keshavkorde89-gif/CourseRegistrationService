# Verification Checklist

The test source is mapped to the acceptance criteria below. Execution has **not** been verified in this authoring environment because the .NET SDK is unavailable.

| Acceptance criterion | Test |
|---|---|
| Available capacity | `Enroll_Succeeds_WhenCapacityAvailable` |
| Full course enters waitlist | `FullCourse_AddsEligibleLearnerToWaitlist` |
| Duplicate enrolled/waitlisted requests | `DuplicateEnrollmentAndWaitlistRequests_DoNotDuplicateRecords` |
| Drop promotes earliest eligible and preserves FIFO | `Drop_PromotesEarliestEligibleAndPreservesFifo` |
| Ineligible learner cannot enroll or queue | `IneligibleLearner_CannotEnrollOrJoinWaitlist` |
| Ineligible queue head skipped but retained | `IneligibleAtQueueHead_IsSkippedRetainedAndNextEligiblePromoted` |
| Unknown learner, invalid and repeated drops | `UnknownLearnerAndInvalidOrRepeatedDrops_DoNotChangeState` |
| Learner not simultaneously enrolled and queued | `LearnerCannotBeBothEnrolledAndWaitlisted` |
| Capacity remains within limit | `CapacityNeverExceededAfterOperations` |

## Commands to execute

```bash
dotnet --info
dotnet restore CourseRegistration.sln
dotnet build CourseRegistration.sln
dotnet test CourseRegistration.sln --logger "console;verbosity=normal"
dotnet run --project src/CourseRegistration/CourseRegistration.csproj
```

## Actual results — fill after execution

- .NET SDK version:
- Restore:
- Build:
- Tests passed/failed/skipped:
- Demo output:
- Issues found and fixes:
