using System;
using System.Collections;
using System.Collections.Generic;

namespace Capstone.Core;
public class RecentlyUsed<T> : IEnumerable<T>
{   private readonly int _capacity;
    private readonly LinkedList<T> _items = new();
    public RecentlyUsed(int capacity)
    {
        if (capacity < 1) 
            throw new ArgumentOutOfRangeException(nameof(capacity));
            
        _capacity = capacity;
    }
    public void Push(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        
        var current = _items.First;
        while (current != null)
        {
            if (comparer.Equals(current.Value, item))
            {
                _items.Remove(current);
                break;
            }
            current = current.Next;
        }

        _items.AddFirst(item);

        if (_items.Count > _capacity)
        {
            _items.RemoveLast();
        }
    }
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

//I created a generic class with 'T' to store the newest items