namespace CourseRegistration.Model;

public sealed class Enrollment
{
    public int Id { get; set; }
    public int OfferingId { get; set; }
    public int LearnerId { get; set; }
}
