using Markdig;

namespace KagamiMD.Services;

public class MarkdownPreviewService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownPreviewService()
    {
        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    }

    public string GenerateHtmlWrapper(string markdownText, string baseHref)
    {
        var htmlBody = Markdown.ToHtml(markdownText ?? string.Empty, _pipeline);
        return $$"""
            <!doctype html>
            <html>
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <base href="{{baseHref}}" />
                <script src="https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js"></script>
                <style>
                    body {
                        font-family: 'Segoe UI', sans-serif;
                        margin: 24px;
                        line-height: 1.65;
                        color: #1f2937;
                        background: #ffffff;
                    }
                    h1, h2, h3, h4, h5, h6 { line-height: 1.25; margin-top: 1.2em; }
                    pre { background: #f3f4f6; padding: 16px; border-radius: 8px; overflow: auto; }
                    code { background: #f3f4f6; padding: 0 4px; border-radius: 4px; }
                    blockquote { margin: 1em 0; padding: 0 16px; border-left: 4px solid #cbd5e1; color: #475569; }
                    table { border-collapse: collapse; width: 100%; }
                    th, td { border: 1px solid #d1d5db; padding: 8px 10px; }
                    img { max-width: 100%; }
                </style>
            </head>
            <body>
            <div id="content">{{htmlBody}}</div>
            <script>
                // Restore scroll position
                const scrollPos = sessionStorage.getItem('previewScrollPos');
                if (scrollPos) {
                    window.scrollTo(0, parseInt(scrollPos, 10));
                }

                // Save scroll position
                window.addEventListener('scroll', () => {
                    sessionStorage.setItem('previewScrollPos', window.scrollY);
                });

                mermaid.initialize({
                    startOnLoad: false,
                    securityLevel: 'loose',
                    theme: 'default'
                });

                function renderMermaid() {
                    document.querySelectorAll('#content pre > code.language-mermaid').forEach((codeBlock) => {
                        const parent = codeBlock.parentElement;
                        if (parent) {
                            const diagram = document.createElement('div');
                            diagram.className = 'mermaid';
                            diagram.textContent = codeBlock.textContent;
                            parent.replaceWith(diagram);
                        }
                    });
                    if (typeof mermaid !== 'undefined') {
                        mermaid.run();
                    }
                }

                renderMermaid();
            </script>
            </body>
            </html>
            """;
    }

    public string GenerateUpdateScript(string markdownText)
    {
        var htmlBody = Markdown.ToHtml(markdownText ?? string.Empty, _pipeline);
        var escapedHtml = System.Text.Json.JsonSerializer.Serialize(htmlBody);
        return $$"""
            var contentDiv = document.getElementById('content');
            if (contentDiv) {
                contentDiv.innerHTML = {{escapedHtml}};
                if (typeof renderMermaid === 'function') {
                    renderMermaid();
                }
            }
            """;
    }

    public string GenerateWysiwygHtmlWrapper(string markdownText)
    {
        var escapedMarkdown = System.Text.Json.JsonSerializer.Serialize(markdownText ?? string.Empty);
        return $$"""
            <!doctype html>
            <html>
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <base href="https://kagami-assets/" />
                <link rel="stylesheet" href="tui-editor.bundle.css">
                <style>
                    body {
                        margin: 0;
                        padding: 0;
                        background: #ffffff;
                    }
                </style>
            </head>
            <body>
            <div id="editor"></div>
            <div id="error-log" style="color:red; margin: 20px;"></div>
            <script>
                window.onerror = function(msg, url, line, col, error) {
                    document.getElementById('error-log').innerText += msg + '\n';
                };
            </script>
            <script src="tui-editor.bundle.js" onerror="document.getElementById('error-log').innerText += 'Failed to load script\n';"></script>
            <script>
                if (window.initWysiwygEditor) {
                    try {
                        window.initWysiwygEditor('editor', {{escapedMarkdown}});
                    } catch (e) {
                        document.getElementById('error-log').innerText += 'Init error: ' + e.message + '\n';
                    }
                } else {
                    document.getElementById('error-log').innerText += "Editor bundle not loaded\n";
                }
            </script>
            </body>
            </html>
            """;
    }

    public string GetPreviewBaseDirectoryPath(string? currentFilePath)
    {
        var directoryPath = !string.IsNullOrWhiteSpace(currentFilePath)
            ? Path.GetDirectoryName(currentFilePath)
            : AppContext.BaseDirectory;

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            directoryPath = AppContext.BaseDirectory;
        }

        return Path.GetFullPath(directoryPath);
    }
}
