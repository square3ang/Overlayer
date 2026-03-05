using Overlayer.Core;
using Overlayer.Core.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Overlayer.Controllers;

public class GUIController {
    private List<IDrawable> drawables = new();
    private int depth;
    private bool isUndoAvailable => depth > 0;
    private bool isRedoAvailable => depth < drawables.Count;
    private IDrawable current;
    private IDrawable first;
    private int skipFrames = 0;
    private Stack<Action> onSkipCallbacks = new();
    public void Init(IDrawable drawable) {
        first = current = drawable;
        hasOnceCalled = false;
    }
    public void Push(IDrawable drawable) {
        if(drawables.Count == depth) {
            drawables.Add(current);
            depth++;
        } else {
            if(drawable.Name != drawables[depth].Name) {
                drawables.RemoveRange(depth, drawables.Count - depth);
                drawables.Add(current);
                depth++;
            } else {
                drawables[depth++] = current;
            }
        }
        current = drawable;
        hasOnceCalled = false;
    }
    public void Pop() {
        if(!isUndoAvailable) {
            return;
        }

        var cache = current;
        current = drawables[--depth];
        drawables[depth] = cache;
        hasOnceCalled = false;
    }

    private bool hasOnceCalled = false;

    public void Draw() {
        if(skipFrames > 0) {
            skipFrames--;
            if(onSkipCallbacks.Count > 0) {
                onSkipCallbacks.Pop()?.Invoke();
            }

            return;
        }
        GUILayout.BeginHorizontal();
        {
            if(isUndoAvailable) {
                if(Drawer.Button("◀ " + Main.Lang.Get("BACK", "Back") + drawables[depth - 1].Name)) {
                    Pop();
                }
            }
            if(isRedoAvailable) {
                var draw = drawables[depth];
                if(Drawer.Button(draw.Name + "▶ " + Main.Lang.Get("FORWARD", "Forward"))) {
                    Push(draw);
                }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if(!hasOnceCalled) {
            current?.OnceCall();
            hasOnceCalled = true;
        }

        current.Draw();
        if(isUndoAvailable) {
            GUILayout.BeginHorizontal();
            if(Drawer.Button("◀ " + Main.Lang.Get("BACK", "Back") + drawables[depth - 1].Name)) {
                Pop();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    public void Skip(Action onSkip = null, int frames = 1) {
        skipFrames += frames;
        onSkipCallbacks.Push(onSkip);
    }
    public void Flush() {
        current = first;
        drawables = new List<IDrawable>();
        depth = 0;
        onSkipCallbacks = new Stack<Action>();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
    }
}
