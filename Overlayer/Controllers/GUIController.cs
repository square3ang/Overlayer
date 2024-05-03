using Overlayer.Core;
using Overlayer.Core.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using TM = Overlayer.Core.Translation.TranslationKeys.Misc;

namespace Overlayer.Controllers
{
    public class GUIController
    {
        private List<IDrawable> drawables = new List<IDrawable>();
        private int depth;
        private bool isUndoAvailable => depth > 0;
        private bool isRedoAvailable => depth < drawables.Count;
        private IDrawable current;
        private IDrawable first;
        private int skipFrames = 0;
        private Stack<Action> onSkipCallbacks = new Stack<Action>();
        public void Init(IDrawable drawable)
        {
            first = current = drawable;
        }
        public void Push(IDrawable drawable)
        {
            if (drawables.Count == depth)
            {
                drawables.Add(current);
                depth++;
            }
            else
            {
                if (drawable.Name != drawables[depth].Name)
                {
                    drawables.RemoveRange(depth, drawables.Count - depth);
                    drawables.Add(current);
                    depth++;
                }
                else drawables[depth++] = current;
            }
            current = drawable;
        }
        public void Pop()
        {
            if (!isUndoAvailable) return;
            var cache = current;
            current = drawables[--depth];
            drawables[depth] = cache;
        }
        public void Draw()
        {
            if (skipFrames > 0)
            {
                skipFrames--;
                if (onSkipCallbacks.Count > 0)
                    onSkipCallbacks.Pop()?.Invoke();
                return;
            }
            Drawer.ButtonLabel($"{Main.Lang[TM.GGReqCnt]} {Main.GGReqCnt}", Main.OpenDiscordLink);
            Drawer.ButtonLabel($"{Main.Lang[TM.GetGGReqCnt]} {Main.GetGGReqCnt}", Main.OpenDiscordLink);
            Drawer.ButtonLabel($"{Main.Lang[TM.TUFReqCnt]} {Main.TUFReqCnt}", Main.OpenDiscordLink);
            Drawer.ButtonLabel($"{Main.Lang[TM.GetTUFReqCnt]} {Main.GetTUFReqCnt}", Main.OpenDiscordLink);
            Drawer.ButtonLabel($"{Main.Lang[TM.HandshakeCnt]} {Main.HandshakeCnt}", Main.OpenDiscordLink);
            Drawer.ButtonLabel($"{Main.Lang[TM.PlayCnt]} {Main.PlayCnt}", Main.OpenDiscordLink);
            GUILayout.BeginHorizontal();
            {
                if (isUndoAvailable)
                {
                    if (GUILayout.Button("◀ " + drawables[depth - 1].Name))
                        Pop();
                }
                if (isRedoAvailable)
                {
                    var draw = drawables[depth];
                    if (GUILayout.Button(draw.Name + " ▶"))
                        Push(draw);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            current.Draw();
        }
        public void Skip(Action onSkip = null, int frames = 1)
        {
            skipFrames += frames;
            onSkipCallbacks.Push(onSkip);
        }
        public void Flush()
        {
            current = first;
            drawables = new List<IDrawable>();
            depth = 0;
            onSkipCallbacks = new Stack<Action>();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
        }
    }
}
