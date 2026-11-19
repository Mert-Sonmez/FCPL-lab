using SharpQuest.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Core;
public class CourseStore : IItemStore, IEnumerable<IItem>
{
    private readonly List<Course> _items = new();
    public event Action<IItem>? ItemAdded;
    public event EventHandler<ItemChangeEventArgs>? ItemAddedWithDetails;
    public int Count => _items.Count;
    public void Add(IItem item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));
        
        if (_items.Any(i => i.Id == item.Id))
            throw new ArgumentException("Id already exists");

        _items.Add((Course)item);
    
        ItemAdded?.Invoke(item);

        ItemAddedWithDetails?.Invoke(this, new ItemChangeEventArgs(item, DateTime.Now)); // second event
    }
    public record ItemChangeEventArgs(IItem Item, DateTime Time);

    public IItem? FindById(string id)
    {
        return _items.FirstOrDefault(i => i.Id == id);
    }
    public IReadOnlyList<IItem> All()
    {
        return _items.ToList();
    }
    public bool Remove(string id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is null) return false;

        return _items.Remove(item);
    }

    public IEnumerator<IItem> GetEnumerator()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IItem this[int index]
    {
        get { return _items[index]; }
    }
    public IItem? this[string id]
    {
        get { return FindById(id); } 
    }
    public IEnumerable<IItem> Matching(string text)
    {
        foreach (var item in _items)
        {
            if (string.IsNullOrEmpty(text) || item.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase))
            {
                yield return item;
            }
        }
    }
    public IEnumerable<IItem> GetHardCourses() 
        => _items.Where(c => c.Credits > 3);
    public IEnumerable<string> GetSortedCourseNames() 
        => _items.OrderBy(c => c.DisplayName).Select(c => c.DisplayName);
    public int GetTotalCredits() 
        => _items.Sum(c => c.Credits);
    public IEnumerable<IItem> GetTop3HardestCourses() 
        => _items.OrderByDescending(c => c.Credits).Take(3);
    public IEnumerable<IItem> GetCoursesWithLongestNames(int count) 
        => _items.Longest(count);
    public bool CanTake(string courseId, IEnumerable<string> passedCourseIds)
    {
        var course = _items.FirstOrDefault(c => c.Id == courseId);
        if (course is null) return false;

        var passed = passedCourseIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return course.Prerequisites.All(p => passed.Contains(p));
    }
    public IEnumerable<Course> GetAvailableCourses(IEnumerable<string> passedCourseIds)
    {
        var passed = passedCourseIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return _items.Where(c => !passed.Contains(c.Id) && c.Prerequisites.All(p => passed.Contains(p)));
    }
    public int GetCreditsForSemester(int year, int term)
        => _items.Where(c => c.Semester.Year == year && c.Semester.Term == term).Sum(c => c.Credits);

    public Task SaveAsync(string path) => CourseStorage.SaveAsync(this, path);
    public static Task<CourseStore> LoadAsync(string path) => CourseStorage.LoadAsync(path);    
}
