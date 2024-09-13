using Overlayer.Core;
using Overlayer.Core.Interfaces;
using Overlayer.Core.Translation;
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
            //Drawer.ButtonLabel($"{Main.Lang[TM.GGReqCnt]} {Main.GGReqCnt}", () => Application.OpenURL(Main.DiscordLink));
            //Drawer.ButtonLabel($"{Main.Lang[TM.GetGGReqCnt]} {Main.GetGGReqCnt}", () => Application.OpenURL(Main.DiscordLink));
            //Drawer.ButtonLabel($"{Main.Lang[TM.TUFReqCnt]} {Main.TUFReqCnt}", () => Application.OpenURL(Main.DiscordLink));
            //Drawer.ButtonLabel($"{Main.Lang[TM.GetTUFReqCnt]} {Main.GetTUFReqCnt}", () => Application.OpenURL(Main.DiscordLink));
            //Drawer.ButtonLabel($"{Main.Lang[TM.HandshakeCnt]} {Main.HandshakeCnt}", () => Application.OpenURL(Main.DiscordLink));
            //Drawer.ButtonLabel($"{Main.Lang[TM.PlayCnt]} {Main.PlayCnt}", () => Application.OpenURL(Main.DiscordLink));
            GUILayout.BeginHorizontal();
            {
                if (isUndoAvailable)
                {
                    if (Drawer.Button("◀ " + drawables[depth - 1].Name))
                        Pop();
                }
                if (isRedoAvailable)
                {
                    var draw = drawables[depth];
                    if (Drawer.Button(draw.Name + " ▶"))
                        Push(draw);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            current.Draw();
            string[] ver = Main.Lang[TranslationKeys.Misc.RealLatestVersion].Split('.');
            if (GUILayout.Button($"<color=#08BDFF>C</color><color=#0BBFF7>#</color><color=#0FC2F0>#</color><color=#13C4E9>'</color><color=#17C7E2>s</color> <color=#1FCCD4>M</color><color=#23CFCD>O</color><color=#27D2C6>D</color> <color=#2FD7B7>S</color><color=#33DAB0>e</color><color=#37DCA9>r</color><color=#3ADFA2>v</color><color=#3EE19B>e</color><color=#42E494>r</color><color=#46E78D>!</color> <color=#4EEC7E>(</color><color=#52EF77>O</color><color=#56F170>v</color><color=#5AF469>e</color><color=#5EF762>r</color><color=#62F95B>l</color><color=#66FC54>a</color><color=#6AFE4C>y</color><color=#6BFE53>e</color><color=#6CFE5A>r</color> <color=#6FFE68>L</color><color=#70FE6F>a</color><color=#72FE76>t</color><color=#73FE7D>e</color><color=#74FE84>s</color><color=#76FE8B>t</color> <color=#78FE99>V</color><color=#7AFEA0>e</color><color=#7BFEA7>r</color><color=#7DFEAE>s</color><color=#7EFEB5>i</color><color=#7FFEBC>o</color><color=#81FEC3>n</color><color=#82FECA>:</color><color=#83FED1>{ver[0]}</color><color=#85FED8>.</color><color=#86FEDF>{ver[1]}</color><color=#87FEE6>.</color><color=#89FEED>{ver[2]}</color><color=#8AFEF4>)</color>", GUI.skin.label))
                Application.OpenURL(Main.DiscordLink);
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
