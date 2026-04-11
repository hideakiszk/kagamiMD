using KagamiMD.Interop;

namespace KagamiMD.Services;

public class ScrollSyncCoordinator
{
    public const string ScrollHideScript = """
        (function() {
            var styleId = 'kagami-scroll-hide';
            if (!document.getElementById(styleId)) {
                var style = document.createElement('style');
                style.id = styleId;
                style.innerHTML = '*::-webkit-scrollbar { display: none !important; } body { -ms-overflow-style: none !important; scrollbar-width: none !important; }';
                document.head.appendChild(style);
            }
        })()
        """;

    public const string ScrollShowScript = "var s = document.getElementById('kagami-scroll-hide'); if (s) s.remove();";

    public const string ScrollEventScript = """
        (function() {
            if (!window.hasScrollBound) {
                window.addEventListener('scroll', function() {
                    if (window.isInternalScrolling) {
                        window.isInternalScrolling = false;
                        return;
                    }
                    var height = document.body.scrollHeight - window.innerHeight;
                    var ratio = height <= 0 ? 0 : window.scrollY / height;
                    window.chrome.webview.postMessage(ratio.toString());
                });
                window.hasScrollBound = true;
            }
            // 連動時はスクロールバーを隠すCSSを再適用
            var styleId = 'kagami-scroll-hide';
            if (!document.getElementById(styleId)) {
                var style = document.createElement('style');
                style.id = styleId;
                style.innerHTML = '*::-webkit-scrollbar { display: none !important; } body { -ms-overflow-style: none !important; scrollbar-width: none !important; }';
                document.head.appendChild(style);
            }
        })()
        """;

    public static string BuildWebViewScrollScript(double ratio)
    {
        return $$"""
            (function() {
                var height = document.body.scrollHeight - window.innerHeight;
                window.isInternalScrolling = true;
                window.scrollTo(0, Math.max(0, height * {{ratio}}));
            })()
            """;
    }

    public void SyncEditorToRatio(IntPtr editorHandle, double ratio, int vScrollBarMax, int vScrollBarLargeChange, ref int vScrollBarValue)
    {
        int range = Math.Max(0, vScrollBarMax - vScrollBarLargeChange + 1);
        int lineIndex = (int)(range * ratio);

        if (vScrollBarValue != lineIndex)
        {
            vScrollBarValue = lineIndex;
            int firstVisibleLine = EditorInterop.GetFirstVisibleLine(editorHandle);
            int diff = lineIndex - firstVisibleLine;
            EditorInterop.ScrollLines(editorHandle, diff);
        }
    }

    public void SyncToLineIndex(IntPtr editorHandle, int lineIndex, out double webViewRatio, int vScrollBarMax, int vScrollBarLargeChange)
    {
        int firstVisibleLine = EditorInterop.GetFirstVisibleLine(editorHandle);
        int diff = lineIndex - firstVisibleLine;
        EditorInterop.ScrollLines(editorHandle, diff);

        double range = Math.Max(1, vScrollBarMax - vScrollBarLargeChange + 1);
        webViewRatio = (double)lineIndex / range;
    }
}
