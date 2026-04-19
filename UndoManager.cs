using System;
using System.Collections.Generic;

namespace KagamiMD.Services;

public class UndoManager
{
    private struct State
    {
        public string Text;
        public int SelectionStart;
        public int FirstVisibleLine;

        public State(string text, int selectionStart, int firstVisibleLine)
        {
            Text = text;
            SelectionStart = selectionStart;
            FirstVisibleLine = firstVisibleLine;
        }
    }

    private readonly List<State> _history = new List<State>();
    private int _currentIndex = -1;
    private const int DefaultMaxHistory = 20;

    // テキストサイズに応じた履歴上限（大規模ファイルでのメモリ消費を抑制）
    private static int GetMaxHistory(int textLength)
    {
        if (textLength > 500_000) return 5;   // 500KB超: 最大5件
        if (textLength > 100_000) return 10;  // 100KB超: 最大10件
        return DefaultMaxHistory;             // 通常: 20件
    }

    public bool CanUndo => _currentIndex > 0;
    public bool CanRedo => _currentIndex < _history.Count - 1;

    public void Clear(string initialText, int selectionStart, int firstVisibleLine = 0)
    {
        _history.Clear();
        _history.Add(new State(initialText, selectionStart, firstVisibleLine));
        _currentIndex = 0;
    }

    public void RecordState(string text, int selectionStart, int firstVisibleLine)
    {
        // If we are recording a state while not at the end of history (after some undos),
        // we remove the future states.
        if (_currentIndex < _history.Count - 1)
        {
            _history.RemoveRange(_currentIndex + 1, _history.Count - (_currentIndex + 1));
        }

        // Avoid recording the exact same state sequentially
        // ReferenceEqualsで高速に同一インスタンスを排除してからstring比較
        if (_currentIndex >= 0)
        {
            var current = _history[_currentIndex];
            if (ReferenceEquals(current.Text, text) || current.Text == text)
            {
                _history[_currentIndex] = new State(text, selectionStart, firstVisibleLine); // Update selection and scroll only
                return;
            }
        }

        _history.Add(new State(text, selectionStart, firstVisibleLine));

        // テキストサイズに応じた動的上限で古い履歴を削除
        // （maxHistory + 1 は「最大履歴数 + 初期状態1件」の上限）
        int maxHistory = GetMaxHistory(text.Length);
        while (_history.Count > maxHistory + 1)
        {
            _history.RemoveAt(0);
        }

        if (_history.Count > _currentIndex + 1)
        {
            _currentIndex = _history.Count - 1;
        }

        // Ensure index is within bounds
        if (_currentIndex >= _history.Count)
        {
            _currentIndex = _history.Count - 1;
        }
    }

    public (string Text, int SelectionStart, int FirstVisibleLine)? Undo()
    {
        if (!CanUndo) return null;
        
        _currentIndex--;
        var state = _history[_currentIndex];
        return (state.Text, state.SelectionStart, state.FirstVisibleLine);
    }

    public (string Text, int SelectionStart, int FirstVisibleLine)? Redo()
    {
        if (!CanRedo) return null;
        
        _currentIndex++;
        var state = _history[_currentIndex];
        return (state.Text, state.SelectionStart, state.FirstVisibleLine);
    }
}
