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
    editor.on('change', () => {
        if (window.chrome && window.chrome.webview) {
            let markdown = editor.getMarkdown();
            
            // Toast UI が挿入する不要な <br> タグをクリーンアップ
            // 特に水平線 (---) の前後に挿入されるものを改行に置換
            markdown = markdown.replace(/<br>\s*([\n]*---[\n]*)\s*<br>/g, '\n\n$1\n\n');
            markdown = markdown.replace(/<br>\s*([\n]*---[\n]*)/g, '\n\n$1');
            markdown = markdown.replace(/([\n]*---[\n]*)\s*<br>/g, '$1\n\n');
            
            window.chrome.webview.postMessage(markdown);
        }
    });

    window.editorInstance = editor;
};

window.updateWysiwygEditor = function(markdown) {
    if (window.editorInstance && window.editorInstance.getMarkdown() !== markdown) {
        // 現在のカーソル位置などが失われる可能性があるため、通常は右ペイン編集中には呼ばれない想定
        window.editorInstance.setMarkdown(markdown, false); // cursorToEnd=false 等の代わり
    }
};
