using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;
public class Lab08Wiring : ILab08Wiring
{
    public IReadOnlyList<Type> KindTypes => new[]
    {
        typeof(MandatoryCourse),
        typeof(ElectiveCourse)
    };

    public IItemStore CreateStore() => new CourseStore();
    public IItem CreateSample(int seed) => CreateOfKind(0, seed);
    public IItem CreateOfKind(int kindIndex, int seed)
    {
        int startHour = 8 + (seed % 8);
        return kindIndex switch
        {
            0 => new MandatoryCourse
            {
                Id = $"M-{seed}",
                DisplayName = $"Mandatory {seed}",
                Credits = 3 + (seed % 4),
                AssessmentType = Assessment.Exam,
                Slot = new TimeSlot(DayOfWeek.Monday, new TimeOnly(startHour, 0), new TimeOnly(startHour + 2, 0), $"Room-{seed}"),
                Semester = new Semester(1 + (seed % 3), 1 + (seed % 2)),
                Prerequisites = seed == 0 ? new List<string>() : new List<string> { $"PR-{seed}" },
                Room = $"Room-{seed}"
            },
            1 => new ElectiveCourse
            {
                Id = $"E-{seed}",
                DisplayName = $"Elective {seed}",
                Credits = 2 + (seed % 3),
                AssessmentType = Assessment.Project,
                Slot = new TimeSlot(DayOfWeek.Wednesday, new TimeOnly(startHour, 0), new TimeOnly(startHour + 2, 0), null),
                Semester = new Semester(1 + (seed % 3), 1 + (seed % 2)),
                Prerequisites = new List<string> { $"M-{seed}", $"BASE-{seed % 3}" },
                Capacity = 20 + (seed % 40)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(kindIndex))
        };
    }

    public async Task SaveAsync(IItemStore store, string path)
    {
        await CourseStorage.SaveAsync(store, path);
    }
    public async Task<IItemStore> LoadAsync(string path)
    {
        return await CourseStorage.LoadAsync(path);
    }
}