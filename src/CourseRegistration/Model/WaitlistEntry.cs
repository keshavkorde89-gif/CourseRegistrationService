namespace CourseRegistration.Model;

public sealed class WaitlistEntry
{
    public int Id { get; set; }
    public int OfferingId { get; set; }
    public int LearnerId { get; set; }
    public long Sequence { get; set; }
}
