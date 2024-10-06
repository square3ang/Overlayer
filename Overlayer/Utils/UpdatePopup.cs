using System.Collections;
using Overlayer.Core;
using UnityEngine;
using DG.Tweening;
using Overlayer.Core.Translatior;
using System.IO;
using RapidGUI;

namespace Overlayer.Utils
{
    internal class UpdatePopup : MonoBehaviour
    {
        private Rect windowRect;
        private string version = "";
        private string[] contentLines;
        private bool isInitaialize = false;
        private bool isAnimating = false;
        private bool isSpawn = false;

        public void Initialize()
        {
            version = Main.ModVersion.ToString();

            string filePath = Path.Combine(Main.Mod.Path,"update.txt");
            if(File.Exists(filePath))
            {
                contentLines = File.ReadAllLines(filePath);
                File.Delete(filePath);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            var maxWidth = 0f;
            
            foreach(var line in contentLines)
            {
                float lineWidth = GUI.skin.label.CalcSize(new GUIContent(line)).x;
                if(lineWidth > maxWidth)
                    maxWidth = lineWidth;
            }
            float width = maxWidth + 40;
            float height = (contentLines.Length * 20) + 40;
            windowRect = new Rect((Screen.width - width) / 2f,(Screen.height - height) / 2f ,width,height);
            
            isInitaialize = true;
        }

        private void OnGUI()
        {
            if(isInitaialize)
            {
                if (!isSpawn && Event.current.type == EventType.Repaint)
                {
                    windowRect = GUILayout.Window(120, windowRect, DrawWindow, $"Overlayer {version} {Main.Lang.Get("UPDATE","Update")}", RGUIStyle.darkWindow);
                    windowRect.x = (int)(Screen.width * 0.5f - windowRect.width * 0.5f);
                    windowRect.y = (int)(Screen.height * 0.5f - windowRect.height * 0.5f);
                    isSpawn = true;
                }
                windowRect = GUILayout.Window(120,windowRect,DrawWindow,$"Overlayer {version} {Main.Lang.Get("UPDATE","Update")}", RGUIStyle.darkWindow);
            }
        }

        private void DrawWindow(int windowID)
        {
            GUI.BringWindowToFront(windowID);
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

            //GUI.DragWindow();
        }


        private IEnumerator DestroyCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }

        private void AnimateAndDestroy()
        {
            if(isAnimating)
                return;
            else
                isAnimating = true;
            StartCoroutine(DestroyCoroutine());
            DOTween.To(() => windowRect.position, x => windowRect.position = x,
                    new Vector2(windowRect.position.x, Screen.height * -1.3f), 0.4f)
                .SetEase(Ease.InBack);
            
        }
    }
}
