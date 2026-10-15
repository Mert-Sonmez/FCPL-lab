using System;
using Xunit;
using SharpQuest.Contracts; 
namespace Capstone.Tests;
public class SearchMatcherTests
{
    // R1
    [Theory]
    [InlineData("Pikachu")]
    [InlineData("fire lizard")]
    public void Query_matches_valid_text(string text)
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches(text, "   "));
        Assert.True(matcher.Matches(text, ""));
    }

    // R2
    [Fact]
    public void Matches_ignores_case_differences()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Pikachu", "PIKA"));
    }

    // R3
    [Fact]
    public void Matches_ignores_diacritics()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Pokémon", "pokemon"));
    }

    // R4 & R5
    [Fact]
    public void Matches_requires()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("fire lizard", "lizard fire"));
        
        Assert.False(matcher.Matches("Pikachu", "Pikachu Charizard")); 
    }

    // R6
    [Fact]
    public void Matches_finds_longer_words()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Pikachu", "chu"));
    }

    // R7
    [Fact]
    public void Matches_returns_false_null_text()
    {
        var matcher = SearchMatcher.Create();
        Assert.False(matcher.Matches(null!, "Pika"));
    }

    // R8
    [Fact]
    public void Matches_throws_ArgumentNullException()
    {
        var matcher = SearchMatcher.Create();
        Assert.Throws<ArgumentNullException>(() => { matcher.Matches("Pikachu", null!); });
    }
}