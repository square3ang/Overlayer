using System;
using Overlayer.Core;
using UnityEngine;
using DG.Tweening;
using Overlayer.Core.Translatior;
using System.IO;
using Overlayer.Unity;
using RapidGUI;

namespace Overlayer.Utils
{
    internal class MovingManEditor : MonoBehaviour
    {
        public string codesBefore;
        public string codesAfter;
        public string matchValue;
        private Rect windowRect;
        private string[] contentLines;
        private bool isInitaialize = false;
        private bool isAnimating = false;
        private bool isSpawn = false;

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

                windowRect = GUILayout.Window(122,windowRect,DrawWindow,fmt, RGUIStyle.darkWindow);
            }
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
                Destroy(gameObject);
            }

            GUILayout.Space(10);
            GUILayout.EndVertical();
            
            //
        }


    }
}