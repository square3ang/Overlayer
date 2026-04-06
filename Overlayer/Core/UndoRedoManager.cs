using System.Collections.Generic;

namespace Overlayer.Core;

public class UndoRedoManager {
    private readonly Stack<string> undoStack = new();
    private readonly Stack<string> redoStack = new();

    public void SaveState(string state) {
        undoStack.Push(state);
        redoStack.Clear();
    }

    public string Undo() {
        if (undoStack.Count > 1) {
            var state = undoStack.Pop();
            redoStack.Push(state);
            return undoStack.Count > 0 ? undoStack.Peek() : null;
        }

        return null;
    }

    public string Redo() {
        if (redoStack.Count > 0) {
            var state = redoStack.Pop();
            undoStack.Push(state);
            return state;
        }

        return null;
    }
}