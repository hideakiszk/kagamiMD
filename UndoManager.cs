using System;
using System.Collections.Generic;

namespace KagamiMD;

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
    private const int MaxHistory = 20;

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
        if (_currentIndex >= 0 && _history[_currentIndex].Text == text)
        {
            _history[_currentIndex] = new State(text, selectionStart, firstVisibleLine); // Update selection and scroll only
            return;
        }

        _history.Add(new State(text, selectionStart, firstVisibleLine));
        
        if (_history.Count > MaxHistory + 1) // +1 because the first state is the initial one
        {
            _history.RemoveAt(0);
        }
        else
        {
            _currentIndex++;
        }
        
        // Ensure index is within bounds if we removed the first item
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
