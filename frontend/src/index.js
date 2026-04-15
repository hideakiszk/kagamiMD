import Editor from '@toast-ui/editor';
import '@toast-ui/editor/dist/toastui-editor.css';

// カスタムのテーマなどもここで設定可能

window.initWysiwygEditor = function(elementId, initialMarkdown) {
    const editor = new Editor({
        el: document.getElementById(elementId),
        height: '100vh',
        initialEditType: 'wysiwyg',
        initialValue: initialMarkdown,
        hideModeSwitch: true, // WYSIWYG固定にするため
        UsageStatistics: false
    });

    // 変更イベントでC#側（WebView2）にMarkdownをpostMessage
    // デバウンス: 連続入力中はpostMessageの乱発を防ぎ、入力が落ち着いてから同期
    let _changeTimer = null;
    editor.on('change', () => {
        if (_changeTimer) clearTimeout(_changeTimer);
        _changeTimer = setTimeout(() => {
            if (window.chrome && window.chrome.webview) {
                let markdown = editor.getMarkdown();
                
                // Toast UI が挿入する不要な <br> タグをクリーンアップ
                // 特に水平線 (---) の前後に挿入されるものを改行に置換
                markdown = markdown.replace(/<br>\s*([\n]*---[\n]*)\s*<br>/g, '\n\n$1\n\n');
                markdown = markdown.replace(/<br>\s*([\n]*---[\n]*)/g, '\n\n$1');
                markdown = markdown.replace(/([\n]*---[\n]*)\s*<br>/g, '$1\n\n');
                
                window.chrome.webview.postMessage(markdown);
            }
        }, 250);
    });

    window.editorInstance = editor;
};

window.updateWysiwygEditor = function(markdown) {
    if (window.editorInstance) {
        // 改行コードなどを正規化して比較し、実質的な変更がない場合は無視する（カーソル飛び防止）
        const current = window.editorInstance.getMarkdown().replace(/\r\n/g, '\n');
        const incoming = markdown.replace(/\r\n/g, '\n');
        
        if (current !== incoming) {
            window.editorInstance.setMarkdown(markdown, false);
        }
    }
};
