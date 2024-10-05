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
            windowRect.height = 300;

            isInitaialize = true;
            this.codesBefore = codesBefore;
            this.codesAfter = codesAfter;
        }

        public void OnGUI()
        {
            if (isInitaialize)
            {
                if (!isSpawn && Event.current.type == EventType.Repaint)
                {
                    windowRect = GUILayout.Window(122, windowRect, DrawWindow, $"MovingMan Editor", RGUIStyle.darkWindow);
                    windowRect.x = (int)(Screen.width * 0.5f - windowRect.width * 0.5f);
                    windowRect.y = (int)(Screen.height * 0.5f - windowRect.height * 0.5f);
                    isSpawn = true;
                }

                windowRect = GUILayout.Window(122,windowRect,DrawWindow,$"MovingMan Editor", RGUIStyle.darkWindow);
            }
        }

        private void DrawWindow(int windowID)
        {
            GUI.BringWindowToFront(windowID);
            GUILayout.BeginVertical();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Tag");
            Drawer.DrawTags(ref targetTag);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            Drawer.DrawDouble("Start Size", ref startSize);
            Drawer.DrawDouble("End Size", ref endSize);
            Drawer.DrawDouble("Default Size", ref defaultSize);
            Drawer.DrawDouble("Speed", ref speed);
            Drawer.DrawBool("Invert", ref invert);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ease");
            Drawer.DrawEnumPlus("", ref ease, a => a);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (Drawer.Button("Done"))
            {
                AnimateAndDestroy();
            }

            GUILayout.Space(10);
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void AnimateAndDestroy()
        {
            if (isAnimating)
                return;
            else
                isAnimating = true;

            DOTween.To(() => windowRect.position, x => windowRect.position = x,
                    new Vector2(windowRect.position.x, Screen.height * -1.3f), 0.4f)
                .SetEase(Ease.InBack)
                .OnComplete(() => { Destroy(gameObject); });
        }
    }
}