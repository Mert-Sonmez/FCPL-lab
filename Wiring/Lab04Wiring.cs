using System;
using System.Collections.Generic;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public class Lab04Wiring : ILab04Wiring
{
    public Type BaseType => typeof(Course);
    public IReadOnlyList<Type> KindTypes => new[] { typeof(MandatoryCourse), typeof(ElectiveCourse) };
    public IItemStore CreateStore() => new CourseStore(); 
    //
    public IItem CreateOfKind(int kind, int seed)
    {
        string id = $"{kind}-{seed}";
        string name = $"Course {seed}";

        if (kind == 0)
        {
            return new MandatoryCourse { Id = id, DisplayName = name, Credits = 3, AssessmentType = Assessment.Exam };
        }
        else
        {
            return new ElectiveCourse { Id = id, DisplayName = name, Credits = 3, AssessmentType = Assessment.Project };
        }
    }

    public string Describe(IItem item) => ((Course)item).Describe();
    public IReadOnlyList<string> DescribeAll(IItemStore store)
    { //
        var descriptions = new List<string>();
        foreach (var item in store.All())
        {
            descriptions.Add(((Course)item).Describe());
        }
        return descriptions;
    }
    public string Classify(IItem item) => item switch
    { //
        null => throw new ArgumentNullException(nameof(item)),
        MandatoryCourse => "mandatory",
        ElectiveCourse => "elective",
        _ => "other"
    };

   //
    public bool TryFindByName(IItemStore store, string name, out IItem? found)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            found = null;
            return false;
        }

        foreach (var item in store.All())
        {
            if (item.DisplayName == name)
            {
                found = item;
                return true;
            }
        }
        
        found = null;
        return false;
    }
    //
    public int AddAll(IItemStore store, params IItem[] items)
    {
        int addedCount = 0;
        foreach (var item in items)
        {
            store.Add(item);
            addedCount++;
        }
        return addedCount;
    }
}