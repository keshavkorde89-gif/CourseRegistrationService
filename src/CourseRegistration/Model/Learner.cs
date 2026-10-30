namespace CourseRegistration.Model;

public sealed class Learner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool HasPrerequisite { get; set; }
}
