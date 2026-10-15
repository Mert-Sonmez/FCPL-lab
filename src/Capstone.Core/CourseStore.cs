using SharpQuest.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capstone.Core;

public class CourseStore : IItemStore
{
    private readonly List<Course> _items = new();

    // İşte eksik olan parça burası! İçerideki listenin eleman sayısını döndürüyor.
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
}