using System;
using System.Text.RegularExpressions;

namespace KagamiMD.Services;

/// <summary>
/// 検索・置換のコアロジックを担当するクラス。
/// UI（Windows Forms）に依存しないため、ユニットテストが可能です。
/// </summary>
public class SearchEngine
{
    public struct SearchResult
    {
        public int Index;
        public int Length;
        public bool Success;
    }

    public static SearchResult Find(string source, string pattern, int startIndex, bool useRegex, bool matchCase)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern))
            return new SearchResult { Success = false };

        if (useRegex)
        {
            var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
            var regex = new Regex(pattern, options);
            var match = regex.Match(source, startIndex);

            if (!match.Success && startIndex > 0)
            {
                // Wrap around
                match = regex.Match(source, 0);
            }

            if (match.Success)
            {
                return new SearchResult { Index = match.Index, Length = match.Length, Success = true };
            }
        }
        else
        {
            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int foundIndex = source.IndexOf(pattern, startIndex, comparison);

            if (foundIndex == -1 && startIndex > 0)
            {
                // Wrap around
                foundIndex = source.IndexOf(pattern, 0, comparison);
            }

            if (foundIndex != -1)
            {
                return new SearchResult { Index = foundIndex, Length = pattern.Length, Success = true };
            }
        }

        return new SearchResult { Success = false };
    }

    public static bool IsMatch(string input, string pattern, bool useRegex, bool matchCase)
    {
        if (input == null || pattern == null) return false;

        var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
        if (useRegex)
        {
            try
            {
                // 選択範囲全体が正規表現に一致するか確認
                var regex = new Regex("^(" + pattern + ")$", options);
                return regex.IsMatch(input);
            }
            catch
            {
                return false;
            }
        }
        else
        {
            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return string.Equals(input, pattern, comparison);
        }
    }

    public static (string Result, int Count) ReplaceAll(string source, string pattern, string replacement, bool useRegex, bool matchCase)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern))
            return (source, 0);

        var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
        Regex regex;
        if (useRegex)
        {
            regex = new Regex(pattern, options);
        }
        else
        {
            regex = new Regex(Regex.Escape(pattern), options);
        }

        int count = regex.Matches(source).Count;
        string result = regex.Replace(source, replacement);
        return (result, count);
    }
}
