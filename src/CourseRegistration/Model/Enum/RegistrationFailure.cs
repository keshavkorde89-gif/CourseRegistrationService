namespace CourseRegistration.Model.Enum;

/// <summary>Identifies why a registration operation failed.</summary>
public enum RegistrationFailure
{
    UnknownOffering,
    UnknownLearner,
    DuplicateEnrollment,
    DuplicateWaitlistEntry,
    PrerequisiteNotMet,
    NotEnrolled
}
