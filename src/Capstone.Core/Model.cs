using System;
using System.Collections.Generic;
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
public record TimeSlot(DayOfWeek Day, TimeOnly Start, TimeOnly End, string? Room = null)
{
    public bool Overlaps(TimeSlot other)
        => Day == other.Day && Start < other.End && other.Start < End;
}
public record Semester(int Year, int Term, int TargetCredits = 30);
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
    public TimeSlot Slot { get; init; } = new(DayOfWeek.Monday, new TimeOnly(10, 0), new TimeOnly(12, 0), null);
    public Semester Semester { get; init; } = new(1, 1);
    public List<string> Prerequisites { get; init; } = new();

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