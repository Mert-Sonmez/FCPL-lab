using System;
using System.Collections.Generic;
using System.Linq;
using SharpQuest.Contracts;

namespace Capstone.Core;
public static class ItemQueries
{
    public static IEnumerable<IItem> Longest(this IEnumerable<IItem> items, int n)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        return items.OrderByDescending(i => i.DisplayName.Length)
                    .ThenBy(i => i.Id, StringComparer.Ordinal)
                    .Take(n);
    }
}