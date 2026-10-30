using CourseRegistration.Controllers;
using CourseRegistration.Data;
using CourseRegistration.Model;
using CourseRegistration.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CourseRegistration.Tests;

public sealed class CourseRegistrationControllerTests
{
    private static (RegistrationDbContext Db, ICourseRegistrationService Service, CourseRegistrationController Controller) Create()
    {
        var options = new DbContextOptionsBuilder<RegistrationDbContext>()
            .UseInMemoryDatabase($"CourseRegistrationControllerTests_{Guid.NewGuid()}")
            .Options;
        var db = new RegistrationDbContext(options);
        db.Database.EnsureCreated();
        var service = new CourseRegistrationService(db, NullLogger<CourseRegistrationService>.Instance);
        var controller = new CourseRegistrationController(service, NullLogger<CourseRegistrationController>.Instance);
        return (db, service, controller);
    }

    [Fact]
    public void Enroll_ReturnsCreated_WhenLearnerIsEnrolledOrWaitlisted()
    {
        var (_, _, controller) = Create();

        var enrolled = controller.Enroll(new EnrollLearnerRequest { LearnerId = 1 });
        var enrolledResponse = Assert.IsType<CreatedAtActionResult>(enrolled.Result);
        Assert.Equal(201, enrolledResponse.StatusCode);

        controller.Enroll(new EnrollLearnerRequest { LearnerId = 2 });
        controller.Enroll(new EnrollLearnerRequest { LearnerId = 3 });
        var waitlisted = controller.Enroll(new EnrollLearnerRequest { LearnerId = 4 });
        var waitlistedResponse = Assert.IsType<CreatedAtActionResult>(waitlisted.Result);
        Assert.Equal(201, waitlistedResponse.StatusCode);
    }

    [Fact]
    public void Enroll_ReturnsBadRequestNotFoundAndConflictForFailures()
    {
        var (_, _, controller) = Create();

        Assert.IsType<BadRequestObjectResult>(controller.Enroll(new EnrollLearnerRequest { LearnerId = 5 }).Result);
        Assert.IsType<NotFoundObjectResult>(controller.Enroll(new EnrollLearnerRequest { LearnerId = 999 }).Result);

        controller.Enroll(new EnrollLearnerRequest { LearnerId = 1 });
        Assert.IsType<ConflictObjectResult>(controller.Enroll(new EnrollLearnerRequest { LearnerId = 1 }).Result);
    }

    [Fact]
    public void Drop_ReturnsOkAndPromotesFifoWaitlistedLearner()
    {
        var (_, service, controller) = Create();
        controller.Enroll(new EnrollLearnerRequest { LearnerId = 1 });
        controller.Enroll(new EnrollLearnerRequest { LearnerId = 2 });
        controller.Enroll(new EnrollLearnerRequest { LearnerId = 3 });

        var result = controller.Drop(1, 1);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Contains(3, service.GetSnapshot(1)!.EnrolledLearnerIds);
        Assert.DoesNotContain(3, service.GetSnapshot(1)!.WaitlistedLearnerIds);
    }

    [Fact]
    public void Drop_ReturnsConflictForNotEnrolledAndNotFoundForUnknownOfferingOrLearner()
    {
        var (_, _, controller) = Create();

        Assert.IsType<ConflictObjectResult>(controller.Drop(1, 1).Result);
        Assert.IsType<NotFoundObjectResult>(controller.Drop(999, 1).Result);
        Assert.IsType<NotFoundObjectResult>(controller.Drop(1, 999).Result);
    }

    [Fact]
    public void GetOffering_ReturnsSnapshotOrNotFound()
    {
        var (_, _, controller) = Create();

        var found = controller.GetOffering(1);
        Assert.IsType<OkObjectResult>(found.Result);
        Assert.IsType<OfferingSnapshot>(Assert.IsType<OkObjectResult>(found.Result).Value);
        Assert.IsType<NotFoundResult>(controller.GetOffering(999).Result);
    }
}
