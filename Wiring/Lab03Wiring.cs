using Capstone.Core;
using SharpQuest.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Capstone.Core.Wiring;

public class Lab03Wiring : ILab03Wiring
{
    public IItemStore CreateStore() => new CourseStore();
    
    public IItem CreateSample(int seed) => new MandatoryCourse
    { 
        Id = $"cs-{seed}", 
        DisplayName = $"Course {seed}",
        Credits = 5,
        AssessmentType = Assessment.Exam
    };
    public IReadOnlyList<IItem> Search(IItemStore store, string query)
    {
        var matcher = new SearchMatcher();
        return store.All().Where(item => matcher.Matches(item.DisplayName, query)).ToList();
    }
}