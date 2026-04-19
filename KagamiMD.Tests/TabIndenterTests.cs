using KagamiMD.Services;

namespace KagamiMD.Tests;

public class TabIndenterTests
{
    // ─── インデント追加 ─────────────────────────────────────────

    [Fact]
    public void Apply_Indent_SingleLine_AddsTab()
    {
        var lines = new[] { "hello" };
        var (text, len) = TabIndenter.Apply(lines, 0, 0, 4, deindent: false);
        Assert.Equal("\thello", text);
        Assert.Equal(text.Length, len);
    }

    [Fact]
    public void Apply_Indent_MultipleLines_AddsTabToEach()
    {
        var lines = new[] { "line1", "line2", "line3" };
        var (text, len) = TabIndenter.Apply(lines, 0, 2, 4, deindent: false);
        Assert.Equal("\tline1\r\n\tline2\r\n\tline3", text);
        Assert.Equal(text.Length, len);
    }

    [Fact]
    public void Apply_Indent_PartialRange_OnlyIndentsSelectedLines()
    {
        var lines = new[] { "line1", "line2", "line3" };
        var (text, _) = TabIndenter.Apply(lines, 1, 2, 4, deindent: false);
        Assert.Equal("\tline2\r\n\tline3", text);
    }

    [Fact]
    public void Apply_Indent_EmptyLine_AddsTabOnly()
    {
        var lines = new[] { "" };
        var (text, _) = TabIndenter.Apply(lines, 0, 0, 4, deindent: false);
        Assert.Equal("\t", text);
    }

    // ─── インデント解除（タブ）──────────────────────────────────

    [Fact]
    public void Apply_Deindent_TabPrefixed_RemovesTab()
    {
        var lines = new[] { "\thello" };
        var (text, _) = TabIndenter.Apply(lines, 0, 0, 4, deindent: true);
        Assert.Equal("hello", text);
    }

    [Fact]
    public void Apply_Deindent_NoIndent_LeavesLineUnchanged()
    {
        var lines = new[] { "hello" };
        var (text, _) = TabIndenter.Apply(lines, 0, 0, 4, deindent: true);
        Assert.Equal("hello", text);
    }

    // ─── インデント解除（スペース）─────────────────────────────

    [Fact]
    public void Apply_Deindent_SpacePrefixed_RemovesUpToTabWidth()
    {
        var lines = new[] { "    hello" }; // 4スペース
        var (text, _) = TabIndenter.Apply(lines, 0, 0, 4, deindent: true);
        Assert.Equal("hello", text);
    }

    [Fact]
    public void Apply_Deindent_FewerSpacesThanTabWidth_RemovesAll()
    {
        var lines = new[] { "  hello" }; // 2スペース、tabWidth=4
        var (text, _) = TabIndenter.Apply(lines, 0, 0, 4, deindent: true);
        Assert.Equal("hello", text);
    }

    [Fact]
    public void Apply_Deindent_ExactTabWidthSpaces_RemovesExact()
    {
        var lines = new[] { "   hello" }; // 3スペース、tabWidth=2
        var (text, _) = TabIndenter.Apply(lines, 0, 0, 2, deindent: true);
        Assert.Equal(" hello", text); // 2スペース分だけ除去
    }

    // ─── 複数行の混合 ──────────────────────────────────────────

    [Fact]
    public void Apply_Deindent_MultiLine_MixedIndent()
    {
        var lines = new[] { "\tline1", "  line2", "line3" };
        var (text, _) = TabIndenter.Apply(lines, 0, 2, 4, deindent: true);
        Assert.Equal("line1\r\nline2\r\nline3", text);
    }

    // ─── 改行の扱い ────────────────────────────────────────────

    [Fact]
    public void Apply_Indent_LastLineInArray_NoTrailingCrLf()
    {
        // lines配列の最終行（TextBox末尾行に相当）には改行を付与しない
        var lines = new[] { "only" };
        var (text, _) = TabIndenter.Apply(lines, 0, 0, 4, deindent: false);
        Assert.False(text.EndsWith("\r\n"), "末尾行には改行を付与しない");
    }

    [Fact]
    public void Apply_Indent_IntermediateLine_HasCrLf()
    {
        // 行配列の末尾ではない行（後続行が存在）には CRLF が付与される
        // （Form1 で lastLineEndPos = GetFirstCharIndexFromLine(endLine+1) と整合する）
        var lines = new[] { "a", "b", "c" };
        var (text, _) = TabIndenter.Apply(lines, 0, 1, 4, deindent: false);
        Assert.Equal("\ta\r\n\tb\r\n", text);
    }
}
