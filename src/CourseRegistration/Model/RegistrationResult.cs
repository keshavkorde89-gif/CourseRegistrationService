using CourseRegistration.Model.Enum;

namespace CourseRegistration.Model;

/// <summary>Describes the result of an enrollment or drop operation.</summary>
public sealed record RegistrationResult(
    bool Success,
    RegistrationStatus Status,
    string Message,
    RegistrationFailure? Failure = null);
