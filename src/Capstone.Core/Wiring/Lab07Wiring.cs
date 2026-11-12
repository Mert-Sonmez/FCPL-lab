using System;
using System.Collections.Generic;
using System.Linq;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;
public class Lab07Wiring : ILab07Wiring
{
    public Type PublisherType => typeof(CourseStore);
    public Type ExtensionHost => typeof(ItemQueries); 
    public IItemStore CreateStore() => new CourseStore();
    public IItem CreateSample(int seed) => new MandatoryCourse { Id = seed.ToString(), DisplayName = "A", Credits = 3, AssessmentType = Assessment.Exam };
    public IEnumerable<string> Names(IItemStore store)
    {
        if (store is null) throw new ArgumentNullException(nameof(store));
        
        return ((CourseStore)store).Select(i => i.DisplayName);
    }
    public IReadOnlyList<IItem> Sorted(IEnumerable<IItem> items)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));
        return items
            .OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.Id, StringComparer.Ordinal)
            .ToList();
    }
    public IReadOnlyList<IItem> Page(IEnumerable<IItem> items, int skip, int take)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));
        if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
        if (take < 0) throw new ArgumentOutOfRangeException(nameof(take));

        return items
            .OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.Id, StringComparer.Ordinal)
            .Skip(skip)
            .Take(take)
            .ToList();
    }
    public IReadOnlyList<IItem> Longest(IEnumerable<IItem> items, int n)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));
        
        return items.Longest(n).ToList();
    }
    public StoreStats Summary(IEnumerable<IItem> items)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));

        if (!items.Any())
        {
            return new StoreStats(0, 0, null, 0);
        }

        int count = items.Count();
        
        int uniqueInitials = items
            .Where(i => !string.IsNullOrEmpty(i.DisplayName))
            .Select(i => char.ToUpperInvariant(i.DisplayName[0]))
            .Distinct()
            .Count();

        string longestName = items
            .OrderByDescending(i => i.DisplayName.Length)
            .First().DisplayName;

        double averageLength = items.Average(i => i.DisplayName.Length);

        return new StoreStats(count, uniqueInitials, longestName, averageLength);
    }
}
