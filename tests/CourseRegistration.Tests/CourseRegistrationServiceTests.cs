using CourseRegistration.Data;
using CourseRegistration.Model;
using CourseRegistration.Model.Enum;
using CourseRegistration.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CourseRegistration.Tests;

public sealed class CourseRegistrationServiceTests
{
    private static (RegistrationDbContext Db, CourseRegistrationService Service) Create()
    {
        var options = new DbContextOptionsBuilder<RegistrationDbContext>()
            .UseInMemoryDatabase($"CourseRegistrationTests_{Guid.NewGuid()}")
            .Options;
        var db = new RegistrationDbContext(options);
        db.Database.EnsureCreated();
        return (db, new CourseRegistrationService(db, NullLogger<CourseRegistrationService>.Instance));
    }

    [Fact]
    public void Enroll_Succeeds_WhenCapacityAvailable()
    {
        var (db, service) = Create();
        var result = service.Enroll(1);
        Assert.True(result.Success);
        Assert.Equal(RegistrationStatus.Enrolled, result.Status);
        Assert.Contains(1, service.GetSnapshot().EnrolledLearnerIds);
        Assert.Empty(service.GetSnapshot().WaitlistedLearnerIds);
    }

    [Fact]
    public void FullCourse_AddsEligibleLearnerToWaitlist()
    {
        var (db, service) = Create();
        service.Enroll(1); service.Enroll(2);
        var result = service.Enroll(3);
        Assert.True(result.Success);
        Assert.Equal(RegistrationStatus.Waitlisted, result.Status);
        Assert.Equal(new[] { 3 }, service.GetSnapshot().WaitlistedLearnerIds);
        Assert.Equal(2, service.GetSnapshot().EnrolledLearnerIds.Count);
    }

    [Fact]
    public void DuplicateEnrollmentAndWaitlistRequests_DoNotDuplicateRecords()
    {
        var (db, service) = Create();
        service.Enroll(1); service.Enroll(2); service.Enroll(3);
        Assert.False(service.Enroll(1).Success);
        Assert.False(service.Enroll(3).Success);
        var snapshot = service.GetSnapshot();
        Assert.Equal(2, snapshot.EnrolledLearnerIds.Count);
        Assert.Equal(new[] { 3 }, snapshot.WaitlistedLearnerIds);
        Assert.Equal(1, db.Enrollments.Count(x => x.LearnerId == 1));
        Assert.Equal(1, db.WaitlistEntries.Count(x => x.LearnerId == 3));
    }

    [Fact]
    public void Drop_PromotesEarliestEligibleAndPreservesFifo()
    {
        var (db, service) = Create();
        service.Enroll(1); service.Enroll(2); service.Enroll(3); service.Enroll(4);
        var result = service.Drop(1);
        var snapshot = service.GetSnapshot();
        Assert.True(result.Success);
        Assert.Contains(3, snapshot.EnrolledLearnerIds);
        Assert.DoesNotContain(3, snapshot.WaitlistedLearnerIds);
        Assert.Equal(new[] { 4 }, snapshot.WaitlistedLearnerIds);
        Assert.Equal(snapshot.Capacity, snapshot.EnrolledLearnerIds.Count);
    }

    [Fact]
    public void IneligibleLearner_CannotEnrollOrJoinWaitlist()
    {
        var (db, service) = Create();
        service.Enroll(1); service.Enroll(2);
        var result = service.Enroll(5);
        Assert.False(result.Success);
        Assert.Equal(RegistrationStatus.Rejected, result.Status);
        Assert.DoesNotContain(5, service.GetSnapshot().EnrolledLearnerIds);
        Assert.DoesNotContain(5, service.GetSnapshot().WaitlistedLearnerIds);
    }

    [Fact]
    public void IneligibleAtQueueHead_IsSkippedRetainedAndNextEligiblePromoted()
    {
        var (db, service) = Create();
        service.Enroll(1); service.Enroll(2); service.Enroll(3); service.Enroll(4);
        var sneha = db.Learners.Single(x => x.Id == 3);
        sneha.HasPrerequisite = false;
        db.SaveChanges();

        service.Drop(1);
        var snapshot = service.GetSnapshot();
        Assert.Contains(4, snapshot.EnrolledLearnerIds);
        Assert.Contains(3, snapshot.WaitlistedLearnerIds); // retained, despite being ineligible
        Assert.DoesNotContain(4, snapshot.WaitlistedLearnerIds);
        Assert.Equal(snapshot.Capacity, snapshot.EnrolledLearnerIds.Count);
    }

    [Fact]
    public void UnknownLearnerAndInvalidOrRepeatedDrops_DoNotChangeState()
    {
        var (db, service) = Create();
        Assert.False(service.Enroll(999).Success);
        Assert.False(service.Drop(999).Success);
        service.Enroll(1); service.Enroll(2); service.Enroll(3);
        var before = service.GetSnapshot();
        Assert.False(service.Drop(4).Success); // known but not enrolled
        AssertSnapshotsEqual(before, service.GetSnapshot());
        Assert.True(service.Drop(1).Success);
        var afterValidDrop = service.GetSnapshot();
        Assert.False(service.Drop(1).Success); // repeated drop must not release another seat
        AssertSnapshotsEqual(afterValidDrop, service.GetSnapshot());
    }

    [Fact]
    public void LearnerCannotBeBothEnrolledAndWaitlisted()
    {
        var (db, service) = Create();
        service.Enroll(1); service.Enroll(2); service.Enroll(3);
        service.Drop(1);
        var snapshot = service.GetSnapshot();
        Assert.Empty(snapshot.EnrolledLearnerIds.Intersect(snapshot.WaitlistedLearnerIds));
        Assert.Equal(snapshot.EnrolledLearnerIds.Count, snapshot.EnrolledLearnerIds.Distinct().Count());
        Assert.Equal(snapshot.WaitlistedLearnerIds.Count, snapshot.WaitlistedLearnerIds.Distinct().Count());
    }

    [Fact]
    public void CapacityNeverExceededAfterOperations()
    {
        var (db, service) = Create();
        foreach (var id in new[] { 1, 2, 3, 4, 5, 999 }) service.Enroll(id);
        service.Drop(1);
        service.Drop(2);
        var snapshot = service.GetSnapshot();
        Assert.True(snapshot.EnrolledLearnerIds.Count <= snapshot.Capacity);
    }
    private static void AssertSnapshotsEqual(OfferingSnapshot expected, OfferingSnapshot actual)
    {
        Assert.Equal(expected.Capacity, actual.Capacity);
        Assert.Equal(expected.EnrolledLearnerIds, actual.EnrolledLearnerIds);
        Assert.Equal(expected.WaitlistedLearnerIds, actual.WaitlistedLearnerIds);
    }

}
