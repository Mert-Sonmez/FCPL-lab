using SharpQuest.Contracts;

namespace Capstone.Core;

public interface ICourseItem : IItem
{
    int Credits { get; }
}

public enum Assessment
{
    Exam,
    Colloquium,
    Project
}

public record CourseDto(
    string? Id, 
    string? DisplayName, 
    int? Credits, 
    string? AssessmentType
);

public abstract class Course : ICourseItem
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required int Credits { get; init; } 
    public required Assessment AssessmentType { get; init; }

    protected string Card() => $"#{Id} {DisplayName} ({Credits} credits)";

    public abstract string Describe();
}

public class MandatoryCourse : Course
{
    public string Room { get; init; } = "301CP";
    public override string Describe() 
    {
        return $"{Card()} - Mandatory Course in Room {Room}, graded by {AssessmentType}";
    }
}

public class ElectiveCourse : Course
{
    public int Capacity { get; init; } = 30;
    public override string Describe() 
    {
        return $"{Card()} - Elective Course (Capacity: {Capacity}), graded by {AssessmentType}";
    }
}