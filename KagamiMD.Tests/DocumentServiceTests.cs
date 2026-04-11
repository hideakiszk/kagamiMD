using System.Text;
using KagamiMD.Services;

namespace KagamiMD.Tests;

public class DocumentServiceTests : IDisposable
{
    private readonly DocumentService _service;
    private readonly string _tempFilePath;

    public DocumentServiceTests()
    {
        _service = new DocumentService();
        _tempFilePath = Path.GetTempFileName();
    }

    [Fact]
    public void NormalizeLineEndings_ShouldConvertToTargetEnding()
    {
        // Arrange
        string input = "Line1\r\nLine2\rLine3\nLine4";
        string expectedCrLf = "Line1\r\nLine2\r\nLine3\r\nLine4";
        string expectedLf = "Line1\nLine2\nLine3\nLine4";

        // Act
        string resultCrLf = DocumentService.NormalizeLineEndings(input, "\r\n");
        string resultLf = DocumentService.NormalizeLineEndings(input, "\n");

        // Assert
        Assert.Equal(expectedCrLf, resultCrLf);
        Assert.Equal(expectedLf, resultLf);
    }

    [Fact]
    public void WriteAndReadFile_WithUtf8Bom_ShouldPreserveEncodingAndContent()
    {
        // Arrange
        string content = "テスト\r\nMarkdown用";
        var encoding = new UTF8Encoding(true); // UTF-8 with BOM

        // Act
        _service.WriteFile(_tempFilePath, content, encoding, "\r\n");
        string readContent = _service.ReadFile(_tempFilePath, out Encoding detectedEncoding);

        // Assert
        Assert.Equal(content, readContent);
        Assert.IsType<UTF8Encoding>(detectedEncoding);
        Assert.Equal(encoding.GetPreamble(), detectedEncoding.GetPreamble());
    }

    [Fact]
    public void WriteAndReadFile_WithShiftJis_ShouldPreserveEncodingAndContent()
    {
        // Arrange
        string content = "シフトJISのテスト\r\nあああ";
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("shift_jis");

        // Act
        _service.WriteFile(_tempFilePath, content, encoding, "\r\n");
        string readContent = _service.ReadFile(_tempFilePath, out Encoding detectedEncoding);

        // Assert
        Assert.Equal(content, readContent);
        Assert.Equal(932, detectedEncoding.CodePage); // 932 is shift-jis
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }
}
