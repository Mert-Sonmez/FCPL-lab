using System;
using System.Linq;
using System.Globalization;

namespace Capstone.Core;

public class SearchMatcher
{
    public static SearchMatcher Create() => new SearchMatcher();
    public bool Matches(string text, string query)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (text == null) return false;
        if (string.IsNullOrWhiteSpace(query)) return true;

        var words = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var compareInfo = CultureInfo.InvariantCulture.CompareInfo;

        return words.All(word => 
            compareInfo.IndexOf(text, word, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0
        );
    }
}