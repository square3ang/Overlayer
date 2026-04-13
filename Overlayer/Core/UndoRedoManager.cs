using System.Collections.Generic;

namespace Overlayer.Core;

public class UndoRedoManager {
    private readonly Stack<string> _UndoStack = new();
    private readonly Stack<string> _RedoStack = new();

    public void SaveState(string state) {
        _UndoStack.Push(state);
        _RedoStack.Clear();
    }

    public string Undo() {
        if(_UndoStack.Count > 1) {
            string state = _UndoStack.Pop();
            _RedoStack.Push(state);
            return _UndoStack.Count > 0 ? _UndoStack.Peek() : null;
        }
        return null;
    }

    public string Redo() {
        if(_RedoStack.Count > 0) {
            string state = _RedoStack.Pop();
            _UndoStack.Push(state);
            return state;
        }
        return null;
    }
}
