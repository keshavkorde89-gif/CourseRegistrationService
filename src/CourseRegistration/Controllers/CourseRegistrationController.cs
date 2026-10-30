using System.ComponentModel.DataAnnotations;
using CourseRegistration.Model;
using CourseRegistration.Model.Enum;
using CourseRegistration.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseRegistration.Controllers;

/// <summary>
/// Provides HTTP endpoints for course registration.
/// </summary>
[ApiController]
[Route("api/courseregistration")]
[Produces("application/json")]
public sealed class CourseRegistrationController(
    ICourseRegistrationService registrationService,
    ILogger<CourseRegistrationController> logger) : ControllerBase
{
    private const int DefaultOfferingId = 1;

    /// <summary>
    /// Enrolls a learner or adds them to the waitlist.
    /// </summary>
    [HttpPost("enroll")]
    public ActionResult<RegistrationResult> Enroll(
        [FromBody] EnrollLearnerRequest request)
    {
        logger.LogInformation(
            "Enrollment requested for learner {LearnerId}",
            request.LearnerId);

        var result = registrationService.Enroll(
            DefaultOfferingId,
            request.LearnerId);

        return result.Failure switch
        {
            RegistrationFailure.UnknownOffering or
            RegistrationFailure.UnknownLearner => NotFound(result),

            RegistrationFailure.DuplicateEnrollment or
            RegistrationFailure.DuplicateWaitlistEntry => Conflict(result),

            RegistrationFailure.PrerequisiteNotMet => BadRequest(result),

            null => CreatedAtAction(
                nameof(GetOffering),
                new { offeringId = DefaultOfferingId },
                result),

            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Drops a learner's enrollment and processes waitlist promotion.
    /// </summary>
    [HttpDelete("{offeringId:int}/learners/{learnerId:int}")]
    public ActionResult<RegistrationResult> Drop(
        [Range(1, int.MaxValue)] int offeringId,
        [Range(1, int.MaxValue)] int learnerId)
    {
        logger.LogInformation(
            "Drop requested for learner {LearnerId} in offering {OfferingId}",
            learnerId,
            offeringId);

        var result = registrationService.Drop(offeringId, learnerId);

        return result.Failure switch
        {
            RegistrationFailure.UnknownOffering or
            RegistrationFailure.UnknownLearner => NotFound(result),

            RegistrationFailure.NotEnrolled => Conflict(result),

            _ when result.Success => Ok(result),

            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Retrieves enrolled learners and the waitlist for an offering.
    /// </summary>
    [HttpGet("{offeringId:int}")]
    public ActionResult<OfferingSnapshot> GetOffering(
        [Range(1, int.MaxValue)] int offeringId)
    {
        var snapshot = registrationService.GetSnapshot(offeringId);

        if (snapshot is null)
        {
            logger.LogWarning(
                "Offering {OfferingId} was not found",
                offeringId);

            return NotFound();
        }

        return Ok(snapshot);
    }
}
