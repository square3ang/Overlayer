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
    internal class ColorRangeEditor : MonoBehaviour
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
        public double valueMin = 0;
        public double valueMax = 100;
        public Color colorMin = Color.black;
        public Color colorMax = Color.white;
        public Ease ease = Ease.OutExpo;


        public void Initialize(string tag, string codesBefore, string codesAfter)
        {
            if (tag.Contains("("))
            {
                var arr = tag.Split('(')[1].Split(')')[0].Split(',');
                targetTag = arr[0];
                valueMin = double.Parse(arr[1]);
                valueMax = double.Parse(arr[2]);
                ColorUtility.TryParseHtmlString("#" + arr[3], out colorMin);
                ColorUtility.TryParseHtmlString("#" + arr[4], out colorMax);
                ease = EnumHelper<Ease>.Parse(arr[5]);
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
                var fmt = string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "ColorRange");
                
                if (!isSpawn && Event.current.type == EventType.Repaint)
                {
                    windowRect = GUILayout.Window(123,windowRect,DrawWindow,fmt, RGUIStyle.darkWindow);
                    windowRect.x = (int) ( Screen.width * 0.5f - windowRect.width * 0.5f );
                    windowRect.y = (int) ( Screen.height * 0.5f - windowRect.height * 0.5f );
                    isSpawn = true;
                }
                
                windowRect = GUILayout.Window(123,windowRect,DrawWindow,fmt, RGUIStyle.darkWindow);
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
            Drawer.DrawDouble(Main.Lang.Get("VALUE_MIN", "Min Value"), ref valueMin);
            Drawer.DrawDouble(Main.Lang.Get("VALUE_MAX", "Max Value"), ref valueMax);

            GUILayout.Label(Main.Lang.Get("COLOR_MIN", "Min Color"));
            Drawer.DrawColor(ref colorMin);
            GUILayout.Label(Main.Lang.Get("COLOR_MAX", "Max Color"));
            Drawer.DrawColor(ref colorMax);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Lang.Get("EASE", "Ease"));
            Drawer.DrawEnumPlus("", ref ease, a => a);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (Drawer.Button(Main.Lang.Get("DONE", "Done")))
            {
                AnimateAndDestroy();
            }

            GUILayout.Space(10);
            GUILayout.EndVertical();
            

            //GUI.DragWindow();
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