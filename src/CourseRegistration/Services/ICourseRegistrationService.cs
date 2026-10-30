using CourseRegistration.Model;

namespace CourseRegistration.Services;

public interface ICourseRegistrationService
{
    RegistrationResult Enroll(int learnerId);

    RegistrationResult Enroll(int offeringId, int learnerId);

    RegistrationResult Drop(int learnerId);

    RegistrationResult Drop(int offeringId, int learnerId);

    OfferingSnapshot GetSnapshot();

    OfferingSnapshot? GetSnapshot(int offeringId);
}
