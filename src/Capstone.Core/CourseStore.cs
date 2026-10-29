using SharpQuest.Contracts;
using System;
using System.Collections; // IEnumerable 
using System.Collections.Generic;
using System.Linq;

namespace Capstone.Core;
public class CourseStore : IItemStore, IEnumerable<IItem>
{
    private readonly List<Course> _items = new();
    public int Count => _items.Count;
    public void Add(IItem item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));
        
        if (_items.Any(i => i.Id == item.Id))
            throw new ArgumentException("Id already exists");

        _items.Add((Course)item);
    }
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
        { //this section is mandatory
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
}
