using System;
using System.Collections.Generic;
using System.Linq;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;
public class Lab05Wiring : ILab05Wiring
{   
    public Type CollectionType => typeof(CourseStore);
    public Type GenericDefinition => typeof(RecentlyUsed<>);
    public IItemStore CreateStore() => new CourseStore();
    public IItem CreateSample(int seed) => new MandatoryCourse { Id = seed.ToString(), DisplayName = "A", Credits = 3, AssessmentType = Assessment.Exam };
    public IItem At(IItemStore store, int index) => ((CourseStore)store)[index];
    public IItem? ById(IItemStore store, string id) => ((CourseStore)store)[id];
    public IReadOnlyList<IItem> EachInOrder(IItemStore store) => ((CourseStore)store).ToList();
    public IEnumerable<IItem> Matching(IItemStore store, string text) => ((CourseStore)store).Matching(text);
    public IReadOnlyList<T> Recent<T>(int capacity, params T[] items)
    {
        var recent = new RecentlyUsed<T>(capacity);
        foreach (var item in items) recent.Push(item);
        return recent.ToList();
    }
}
