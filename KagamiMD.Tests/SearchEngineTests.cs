using KagamiMD.Services;
using Xunit;
using System;

namespace KagamiMD.Tests;

public class SearchEngineTests
{
    [Theory]
    [InlineData("Hello World", "Hello", 0, false, true, 0, 5)]
    [InlineData("Hello World", "hello", 0, false, false, 0, 5)]
    [InlineData("Hello World", "hello", 0, false, true, -1, 0)] // Not found (case sensitive)
    [InlineData("Hello World", "World", 0, false, true, 6, 5)]
    [InlineData("Hello World", "World", 7, false, true, 6, 5)] // Wrap around from index 7
    public void Find_LiteralSearch_ReturnsExpectedResult(
        string source, string pattern, int startIndex, bool useRegex, bool matchCase,
        int expectedIndex, int expectedLength)
    {
        var result = SearchEngine.Find(source, pattern, startIndex, useRegex, matchCase);

        if (expectedIndex == -1)
        {
            Assert.False(result.Success);
        }
        else
        {
            Assert.True(result.Success);
            Assert.Equal(expectedIndex, result.Index);
            Assert.Equal(expectedLength, result.Length);
        }
    }

    [Theory]
    [InlineData("abc-123-def", @"\d+", 0, true, true, 4, 3)]
    [InlineData("test123test", @"[a-z]+", 5, true, true, 7, 4)] // Next match starts at index 7
    [InlineData("Apple banana", @"^apple", 0, true, false, 0, 5)] // Regex case insensitive
    public void Find_RegexSearch_ReturnsExpectedResult(
        string source, string pattern, int startIndex, bool useRegex, bool matchCase,
        int expectedIndex, int expectedLength)
    {
        var result = SearchEngine.Find(source, pattern, startIndex, useRegex, matchCase);

        Assert.True(result.Success, $"Search failed for pattern {pattern}");
        Assert.Equal(expectedIndex, result.Index);
        Assert.Equal(expectedLength, result.Length);
    }

    [Theory]
    [InlineData("apple", "apple", false, true, true)]
    [InlineData("apple", "APPLE", false, false, true)]
    [InlineData("apple", "APPLE", false, true, false)]
    [InlineData("apple123", @"[a-z]+\d+", true, true, true)] // Full match regex
    [InlineData("apple123", @"[a-z]+", true, true, false)] // Partial match regex (IsMatch uses ^ and $)
    public void IsMatch_ReturnsExpectedResult(
        string input, string pattern, bool useRegex, bool matchCase, bool expected)
    {
        var result = SearchEngine.IsMatch(input, pattern, useRegex, matchCase);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("aa bb aa", "aa", "cc", false, true, "cc bb cc", 2)]
    [InlineData("Aa aa AA", "aa", "xx", false, false, "xx xx xx", 3)]
    [InlineData("123-456-789", @"\d+", "N", true, true, "N-N-N", 3)]
    public void ReplaceAll_ReturnsExpectedResult(
        string source, string pattern, string replacement, bool useRegex, bool matchCase,
        string expectedResult, int expectedCount)
    {
        var (result, count) = SearchEngine.ReplaceAll(source, pattern, replacement, useRegex, matchCase);

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public void Find_InvalidRegex_ThrowsArgumentException()
    {
        // SearchEngine.Find should throw when regex is invalid.
        // RegexParseException inherits from ArgumentException.
        Assert.ThrowsAny<ArgumentException>(() =>
            SearchEngine.Find("source", "[unclosed group", 0, true, true));
    }
}
