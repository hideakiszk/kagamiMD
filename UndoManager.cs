using System;
using System.Collections.Generic;

namespace KagamiMD;

public class UndoManager
{
    private struct State
    {
        public string Text;
        public int SelectionStart;

        public State(string text, int selectionStart)
        {
            Text = text;
            SelectionStart = selectionStart;
        }
    }

    private readonly List<State> _history = new List<State>();
    private int _currentIndex = -1;
    private const int MaxHistory = 20;

    public bool CanUndo => _currentIndex > 0;
    public bool CanRedo => _currentIndex < _history.Count - 1;

    public void Clear(string initialText, int selectionStart)
    {
        _history.Clear();
        _history.Add(new State(initialText, selectionStart));
        _currentIndex = 0;
    }

    public void RecordState(string text, int selectionStart)
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
            _history[_currentIndex] = new State(text, selectionStart); // Update selection only
            return;
        }

        _history.Add(new State(text, selectionStart));
        
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

    public (string Text, int SelectionStart)? Undo()
    {
        if (!CanUndo) return null;
        
        _currentIndex--;
        var state = _history[_currentIndex];
        return (state.Text, state.SelectionStart);
    }

    public (string Text, int SelectionStart)? Redo()
    {
        if (!CanRedo) return null;
        
        _currentIndex++;
        var state = _history[_currentIndex];
        return (state.Text, state.SelectionStart);
    }
}
