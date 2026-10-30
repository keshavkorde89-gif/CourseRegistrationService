namespace CourseRegistration.Model;

public sealed class CourseOffering
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}
