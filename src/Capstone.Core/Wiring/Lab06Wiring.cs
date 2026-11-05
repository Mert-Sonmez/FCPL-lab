using System;
using System.Collections.Generic;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;
public class Lab06Wiring : ILab06Wiring
{
    public Type PublisherType => typeof(CourseStore);
    public IItemStore CreateStore() => new CourseStore();
    public IItem CreateSample(int seed) => new MandatoryCourse { Id = seed.ToString(), DisplayName = "A", Credits = 3, AssessmentType = Assessment.Exam };
    public IReadOnlyList<IItem> Where(IItemStore store, Func<IItem, bool> keep)
    {
        if (keep is null) throw new ArgumentNullException(nameof(keep)); // Check Null

        var kept = new List<IItem>();
        foreach (var item in ((CourseStore)store))
        {
            if (keep(item))
            {
                kept.Add(item);
            }
        }
        return kept;
    }
    public IReadOnlyList<string> Map(IItemStore store, Func<IItem, string> transform)
    {
        if (transform is null) throw new ArgumentNullException(nameof(transform)); 

        var mapped = new List<string>();
        foreach (var item in ((CourseStore)store))
        {
            mapped.Add(transform(item));
        }
        return mapped;
    }
    //I check if the function is null first, to prevent crashes
    public void ForEach(IItemStore store, Action<IItem> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action)); 

        foreach (var item in ((CourseStore)store))
        {
            action(item);
        }
    }
    public Action Subscribe(IItemStore store, Action<IItem> handler)
    {
        var myStore = (CourseStore)store;
        myStore.ItemAdded += handler; // start listening to the event
        
        return () => myStore.ItemAdded -= handler; // returns a function to stop listening
    }
}