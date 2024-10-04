﻿using Overlayer.Core;
using UnityEngine;
using DG.Tweening;
using Overlayer.Core.Translatior;
using System.IO;

namespace Overlayer.Utils
{
    internal class Popup : MonoBehaviour
    {
        private Rect windowRect;
        private string version = "";
        private string[] contentLines; // Changed from string to string array
        private bool isInitaialize = false;
        private bool isAnimating = false;

        public void Initialize()
        {
            version = Main.ModVersion.ToString();

            string filePath = Path.Combine(Main.Mod.Path,"update.txt");
            if(File.Exists(filePath))
            {
                // Read all lines into the contentLines array
                contentLines = File.ReadAllLines(filePath);
                File.Delete(filePath);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            float maxWidth = 0;
            foreach(var line in contentLines)
            {
                float lineWidth = GUI.skin.label.CalcSize(new GUIContent(line)).x;
                if(lineWidth > maxWidth)
                    maxWidth = lineWidth;
            }
            float width = maxWidth + 40;
            float height = (contentLines.Length * 20) + 40;
            windowRect = new Rect((Screen.width - width) / 2,(Screen.height - height) / 2,width,height);
            isInitaialize = true;
        }

        private void OnGUI()
        {
            if(isInitaialize)
            {
                windowRect = GUILayout.Window(120,windowRect,DrawWindow,$"Overlayer {version} {Main.Lang.Get("UPDATE","Update")}");
            }
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(10);

            GUILayout.FlexibleSpace();

            foreach(var line in contentLines)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(line,GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(Drawer.Button($"<size=18>{Main.Lang.Get("OK","OK!")}</size>",GUILayout.Width(100),GUILayout.Height(40)))
            {
                AnimateAndDestroy();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void AnimateAndDestroy()
        {
            if(isAnimating)
                return;
            else
                isAnimating = true;

            DOTween.To(() => windowRect.position,x => windowRect.position = x,new Vector2(windowRect.position.x,Screen.height * -1.3f),0.4f)
                   .SetEase(Ease.InBack)
                   .OnComplete(() =>
                   {
                       Destroy(gameObject);
                   });
        }
    }
}
