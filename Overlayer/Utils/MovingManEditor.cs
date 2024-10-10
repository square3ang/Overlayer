using System;
using Overlayer.Core;
using UnityEngine;
using DG.Tweening;
using Overlayer.Core.Translatior;
using System.IO;
using Overlayer.Patches;
using Overlayer.Tags;
using Overlayer.Unity;
using RapidGUI;
using Time = UnityEngine.Time;

namespace Overlayer.Utils
{
    internal class MovingManEditor : MonoBehaviour
    {
        public string codesBefore;
        public string codesAfter;
        public string matchValue;
        private Rect windowRect;
        private Rect previewWindowRect;
        private string[] contentLines;
        private bool isInitaialize = false;
        private bool isAnimating = false;
        private bool isSpawn = false;

        private int tester = 0;
        private float timer = 0;

        public string targetTag = "Combo";
        public double startSize = 30;

        public double endSize = 80;
        public double defaultSize = 30;
        public double speed = 800;

        public bool invert = false;
        public Ease ease = Ease.OutExpo;


        public void Initialize(string tag, string codesBefore, string codesAfter)
        {
            if (tag.Contains("("))
            {
                var arr = tag.Split('(')[1].Split(')')[0].Split(',');
                targetTag = arr[0];
                startSize = double.Parse(arr[1]);
                endSize = double.Parse(arr[2]);
                defaultSize = double.Parse(arr[3]);
                speed = double.Parse(arr[4]);
                invert = bool.Parse(arr[5]);
                ease = EnumHelper<Ease>.Parse(arr[6]);
            }

            windowRect.width = 300;

            isInitaialize = true;
            this.codesBefore = codesBefore;
            this.codesAfter = codesAfter;
            BlockUMMClosing.Block = true;
            TagManager.testerValue = "0";
        }

        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= speed / 1000f)
            {
                timer -= (float)speed / 1000f;
                tester++;
                if (tester > 100) tester = 0;
                TagManager.testerValue = tester.ToString();
            }
        }

        public void OnGUI()
        {
            if (isInitaialize)
            {
                var fmt = string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "MovingMan");
                if (!isSpawn && Event.current.type == EventType.Repaint)
                {
                    windowRect = GUILayout.Window(122, windowRect, DrawWindow, fmt, RGUIStyle.darkWindow);
                    windowRect.x = (int)(Screen.width * 0.5f - windowRect.width * 0.5f);
                    windowRect.y = (int)(Screen.height * 0.5f - windowRect.height * 0.5f);
                    
                    
                    
                    isSpawn = true;
                }
                
                windowRect = GUILayout.Window(122, windowRect, DrawWindow, fmt, RGUIStyle.darkWindow);
                var txt = "<size=" + Math.Max(Math.Max(startSize, endSize), defaultSize) +
                          ">Test</size>";
                var sz = GUI.skin.label.CalcSize(new GUIContent(txt));
                previewWindowRect.width = sz.x + 50;
                previewWindowRect.height = sz.y + 50;
                previewWindowRect.x = windowRect.x + windowRect.width + 10;
                previewWindowRect.y = windowRect.y;
                previewWindowRect = GUI.Window(1122, previewWindowRect, PreviewWindow, "",
                    RGUIStyle.darkWindow);
            }
        }

        private void PreviewWindow(int windowID)
        {
            GUI.BringWindowToFront(windowID);
            GUILayout.Label("<size=" + Effect.MovingMan("INTERNAL_TESTER_TAG_1234512345", startSize, endSize,
                defaultSize, speed, invert, ease) + ">Test</size>");
        }

        private void DrawWindow(int windowID)
        {
            GUI.BringWindowToFront(windowID);

            GUILayout.BeginVertical();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Lang.Get("TARGET_TAG", "Target Tag"));
            Drawer.DrawTags(ref targetTag);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            Drawer.DrawDouble(Main.Lang.Get("START_SIZE", "Start Size"), ref startSize);
            Drawer.DrawDouble(Main.Lang.Get("END_SIZE", "End Size"), ref endSize);
            Drawer.DrawDouble(Main.Lang.Get("DEFAULT_SIZE", "Default Size"), ref defaultSize);
            Drawer.DrawDouble(Main.Lang.Get("SPEED", "Speed"), ref speed);
            Drawer.DrawBool(Main.Lang.Get("INVERT", "Invert"), ref invert);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Lang.Get("EASE", "Ease"));
            Drawer.DrawEnumPlus("", ref ease, a => a);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (Drawer.Button(Main.Lang.Get("DONE", "Done")))
            {
                BlockUMMClosing.Block = false;
                Destroy(gameObject);
            }

            GUILayout.Space(10);
            GUILayout.EndVertical();

            //
        }
    }
}