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
public class Course : ICourseItem
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required int Credits { get; init; }
    public required Assessment AssessmentType { get; init; }
}