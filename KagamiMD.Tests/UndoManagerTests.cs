using KagamiMD.Services;
using Xunit;

namespace KagamiMD.Tests;

public class UndoManagerTests
{
    [Fact]
    public void RecordState_InitialState_CanUndoIsFalse()
    {
        var manager = new UndoManager();
        manager.Clear("Initial", 0, 0);
        
        Assert.False(manager.CanUndo);
        Assert.False(manager.CanRedo);
    }

    [Fact]
    public void RecordState_AfterOneRecord_CanUndoIsTrue()
    {
        var manager = new UndoManager();
        manager.Clear("Initial", 0, 0);
        manager.RecordState("Second", 5, 2);
        
        Assert.True(manager.CanUndo);
        Assert.False(manager.CanRedo);
    }

    [Fact]
    public void Undo_RestoresPreviousState()
    {
        var manager = new UndoManager();
        manager.Clear("Initial", 0, 1);
        manager.RecordState("Second", 5, 2);
        
        var result = manager.Undo();
        
        Assert.True(result.HasValue);
        Assert.Equal("Initial", result.Value.Text);
        Assert.Equal(0, result.Value.SelectionStart);
        Assert.Equal(1, result.Value.FirstVisibleLine);
        Assert.False(manager.CanUndo);
        Assert.True(manager.CanRedo);
    }

    [Fact]
    public void Redo_RestoresStateAfterUndo()
    {
        var manager = new UndoManager();
        manager.Clear("Initial", 0, 0);
        manager.RecordState("Second", 5, 10);
        manager.Undo();
        
        var result = manager.Redo();
        
        Assert.True(result.HasValue);
        Assert.Equal("Second", result.Value.Text);
        Assert.Equal(5, result.Value.SelectionStart);
        Assert.Equal(10, result.Value.FirstVisibleLine);
        Assert.True(manager.CanUndo);
        Assert.False(manager.CanRedo);
    }

    [Fact]
    public void RecordState_InMiddleOfHistory_RemovesFuture()
    {
        var manager = new UndoManager();
        manager.Clear("1", 0, 0);
        manager.RecordState("2", 1, 0);
        manager.RecordState("3", 2, 0);
        
        manager.Undo(); // Now at "2"
        manager.RecordState("New 3", 3, 0);
        
        Assert.False(manager.CanRedo);
        
        var undoResult = manager.Undo();
        Assert.True(undoResult.HasValue);
        Assert.Equal("2", undoResult.Value.Text);
    }

    [Fact]
    public void MaxHistory_IsRespected()
    {
        var manager = new UndoManager();
        manager.Clear("Seed", 0, 0);

        // MaxHistory is 20 in UndoManager.cs
        // 1 initial + 20 additions = 21 items total
        for (int i = 1; i <= 25; i++)
        {
            manager.RecordState("State " + i, i, i);
        }

        // After 25 additions, the first few should be gone.
        // History should contain "State 25" down to whatever fits.
        
        // Let's verify we can undo 20 times?
        for (int i = 0; i < 20; i++)
        {
            Assert.True(manager.CanUndo);
            manager.Undo();
        }
        
        // After 20 undos, we should be at the oldest available state.
        Assert.False(manager.CanUndo);
    }
}
