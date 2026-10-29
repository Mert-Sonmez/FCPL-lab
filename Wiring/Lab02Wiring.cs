using Capstone.Core;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public class Lab02Wiring : ILab02Wiring
{
    public IItemStore CreateStore() => new CourseStore();
    public IItemMapper CreateMapper() => new CourseMapper();
    public IItem CreateSample(int seed) => new MandatoryCourse
    { 
        Id = $"cs-{seed}", 
        DisplayName = $"Course {seed}",
        Credits = 5,
        AssessmentType = Assessment.Exam
    };
}