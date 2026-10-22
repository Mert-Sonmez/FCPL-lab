using System;
using Xunit;
using Capstone.Core;

namespace Capstone.Tests;

public class TestFile
{
    [Fact]
    public void Test_1()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Mathematics", "Math"));
    }

    [Fact]
    public void Test_2()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Mathematics", "math"));
    }

    [Fact]
    public void Test_3()
    {
        var matcher = SearchMatcher.Create();
        Assert.False(matcher.Matches(null!, "Math"));
    }

    [Fact]
    public void Test_4()
    {
        var matcher = SearchMatcher.Create();
        Assert.Throws<ArgumentNullException>(() => matcher.Matches("Mathematics", null!));
    }

    [Theory]
    [InlineData("Mathematics")]
    [InlineData("Physics")]
    public void Test_5(string query)
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches(query, query));
    }
}