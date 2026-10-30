namespace CourseRegistration.Model;

/// <summary>Provides the current enrollment and waitlist state for an offering.</summary>
public sealed record OfferingSnapshot(
    int Capacity,
    IReadOnlyList<int> EnrolledLearnerIds,
    IReadOnlyList<int> WaitlistedLearnerIds);
