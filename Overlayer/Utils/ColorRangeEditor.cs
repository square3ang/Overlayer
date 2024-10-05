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

            float width = 300;
            float height = 300;
            windowRect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            isInitaialize = true;
            this.codesBefore = codesBefore;
            this.codesAfter = codesAfter;
        }

        public void OnGUI()
        {
            if (isInitaialize)
            {
                windowRect = GUILayout.Window(123, windowRect, windowID =>
                {
                    GUI.BringWindowToFront(windowID);
                    GUILayout.BeginVertical();
                    GUILayout.Space(10);
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Target Tag");
                    Drawer.DrawTags(ref targetTag);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    Drawer.DrawDouble("Value Min", ref valueMin);
                    Drawer.DrawDouble("Value Max", ref valueMax);
                    
                    GUILayout.Label("Color Min");
                    Drawer.DrawColor(ref colorMin);
                    GUILayout.Label("Color Max");
                    Drawer.DrawColor(ref colorMax);
                    
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
                }, $"", RGUIStyle.darkWindow);
            }
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