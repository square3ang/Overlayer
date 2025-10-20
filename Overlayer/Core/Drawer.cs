using HarmonyLib;
using Overlayer.CodeEditor;
using Overlayer.Models;
using Overlayer.Tags;
using Overlayer.Utils;
using RapidGUI;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityModManagerNet;
using IDrawable = Overlayer.Core.Interfaces.IDrawable;

namespace Overlayer.Core
{
    public static class Drawer
    {
        public static bool DrawVector2(string label, ref Vector2 vec2, float lValue, float rValue)
        {
            bool changed = false;
            GUILayout.Label($"<b>{label}</b>");
            changed |= DrawSingleWithSlider("X", ref vec2.x, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Y", ref vec2.y, lValue, rValue, 300f);
            return changed;
        }

        public static bool NeoDrawVector2(string label, ref Vector2 vec2, float lValue, float rValue, ref string[] field, ref int error, int errorBit) {
            bool changed = false;
            GUILayout.Label($"<b>{label}</b>");
            changed |= NeoDrawSingleWithSlider("X", ref vec2.x, lValue, rValue, 300f, ref field[0], ref error, errorBit);
            changed |= NeoDrawSingleWithSlider("Y", ref vec2.y, lValue, rValue, 300f, ref field[1], ref error, errorBit + 1);
            return changed;
        }

        public static bool DrawVector3(string label, ref Vector3 vec3, float lValue, float rValue)
        {
            bool changed = false;
            GUILayout.Label($"<b>{label}</b>");
            changed |= DrawSingleWithSlider("X", ref vec3.x, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Y", ref vec3.y, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Z", ref vec3.z, lValue, rValue, 300f);
            return changed;
        }

        public static bool NeoDrawVector3(string label, ref Vector3 vec3, float lValue, float rValue, ref string[] field, ref int error, int errorBit) {
            bool changed = false;
            GUILayout.Label($"<b>{label}</b>");
            changed |= NeoDrawSingleWithSlider("X", ref vec3.x, lValue, rValue, 300f, ref field[0], ref error, errorBit);
            changed |= NeoDrawSingleWithSlider("Y", ref vec3.y, lValue, rValue, 300f, ref field[1], ref error, errorBit + 1);
            changed |= NeoDrawSingleWithSlider("Z", ref vec3.z, lValue, rValue, 300f, ref field[2], ref error, errorBit + 2);
            return changed;
        }

        public static void DrawGColor(string label, ref GColor color, bool canEnableGradient, Action onChange)
        {
            GUILayout.Label(label);
            DrawGColor(ref color, canEnableGradient).IfTrue(onChange);
        }

        public static bool SelectionPopup(ref int selected, string[] options, string label,
            params GUILayoutOption[] layoutOptions)
        {
            if (label != "")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label);
            }

            var news = RGUI.SelectionPopup(selected, options, null, layoutOptions);
            var c = selected != news;

            selected = news;
            if (label != "")
                GUILayout.EndHorizontal();
            return c;
        }

        public static bool SelectionPopupWithTooltip(ref int selected, string[] options, string label,
            Dictionary<string, string> tooltips, params GUILayoutOption[] layoutOptions)
        {
            if (label != "")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label);
            }

            var news = RGUI.SelectionPopup(selected, options, tooltips, layoutOptions);
            var c = selected != news;

            selected = news;
            if (label != "")
                GUILayout.EndHorizontal();
            return c;
        }


        public static bool DrawGColor(ref GColor color, bool canEnableGradient)
        {
            bool ge = color.gradientEnabled, prevGe = color.gradientEnabled;
            if (canEnableGradient && DrawBool(Main.Lang.Get("MISC_ENABLE_GRADIENT", "Enable Gradient"), ref ge))
                color = color with { gradientEnabled = ge };
            color.gradientEnabled &= canEnableGradient;
            bool result = ge != prevGe;
            if (color.gradientEnabled)
            {
                Color tl = color.topLeft,
                    tr = color.topRight,
                    bl = color.bottomLeft,
                    br = color.bottomRight;
                ExpandableGUI(color.topLeftStatus, Main.Lang.Get("MISC_TOP_LEFT", "Top Left"),
                    () => result |= DrawColor(ref tl));
                ExpandableGUI(color.topRightStatus, Main.Lang.Get("MISC_TOP_RIGHT", "Top Right"),
                    () => result |= DrawColor(ref tr));
                ExpandableGUI(color.bottomLeftStatus, Main.Lang.Get("MISC_BOTTOM_LEFT", "Bottom Left"),
                    () => result |= DrawColor(ref bl));
                ExpandableGUI(color.bottomRightStatus, Main.Lang.Get("MISC_BOTTOM_RIGHT", "Bottom Right"),
                    () => result |= DrawColor(ref br));
                if (result)
                {
                    color.topLeft = tl;
                    color.topRight = tr;
                    color.bottomLeft = bl;
                    color.bottomRight = br;
                }
            }
            else
            {
                Color dummy = color.topLeft;
                if (result = DrawColor(ref dummy)) color = dummy;
            }

            return result;
        }

        public static void ExpandableGUI(GUIStatus status, string label, Action drawer)
        {
            GUILayoutEx.ExpandableGUI(drawer, label, ref status.Expanded);
        }

        public static bool DrawColor(ref Color color)
        {
            /*bool result = false;
            result |= DrawSingleWithSlider("<color=#FF0000>R</color>", ref color.r, 0, 1, 300f);
            result |= DrawSingleWithSlider("<color=#00FF00>G</color>", ref color.g, 0, 1, 300f);
            result |= DrawSingleWithSlider("<color=#0000FF>B</color>", ref color.b, 0, 1, 300f);
            result |= DrawSingleWithSlider("A", ref color.a, 0, 1, 300f);
            string hex = ColorUtility.ToHtmlStringRGBA(color);
            if (DrawString("Hex:", ref hex))
            {
                result = true;
                ColorUtility.TryParseHtmlString("#" + hex, out color);
            }

            return result;*/
            var c = false;
            string hex = ColorUtility.ToHtmlStringRGBA(color);
            if (DrawString("Hex:", ref hex))
            {
                c = true;
                ColorUtility.TryParseHtmlString("#" + hex, out color);
            }

            var ncol = RGUI.Field(color, "");

            if (!c) c = color != ncol;
            color = ncol;

            return c;
        }

        public static bool DrawSingleWithSlider(string label, ref float value, float lValue, float rValue, float width)
        {
            GUILayout.BeginHorizontal();
            float newValue = GUILayoutEx.NamedSliderContent(label, value, lValue, rValue, width);
            GUILayout.EndHorizontal();
            bool result = newValue != value;
            value = newValue;
            return result;
        }

        public static bool NeoDrawSingleWithSlider(string label, ref float value, float lValue, float rValue, float width, ref string field, ref int error, int errorBit) {
            bool hasError = (error & (1 << errorBit)) != 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            GUILayout.Space(4f);

            bool changed = false;
            float currentValue = value;
            float sliderValue = GUILayout.HorizontalSlider(currentValue, lValue, rValue, mySlider, myThumb, GUILayout.Width(width));
            if(sliderValue != currentValue) {
                currentValue = sliderValue;
                field = currentValue.ToString();
                changed = true;
            }
            GUILayout.Space(8f);
            if(hasError) {
                GUI.color = new Color(1f, 0.5f, 0.5f);
            }
            string newField = GUILayout.TextField(field, 9, myTextField, GUILayout.Width(100f));
            if(newField != field) {
                field = newField;
                if(float.TryParse(field, out float parsedValue)) {
                    currentValue = parsedValue;
                    changed = true;
                    error &= ~(1 << errorBit);
                } else {
                    error |= (1 << errorBit);
                }
            }
            if(hasError) {
                GUI.color = Color.white;
                GUILayout.Space(8f);
                GUILayout.Label("<color=#FF8888>!!</color>");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            value = currentValue;

            return changed;
        }

        public static bool DrawStringArray(ref string[] array, Action<int> arrayResized = null,
            Action<int> elementRightGUI = null, Action<int, string> onElementChange = null)
        {
            bool result = false;
            GUILayout.BeginHorizontal();
            if (Drawer.Button("+"))
            {
                Array.Resize(ref array, array.Length + 1);
                arrayResized?.Invoke(array.Length);
                result = true;
            }

            if (array.Length > 0 && Drawer.Button("-"))
            {
                Array.Resize(ref array, array.Length - 1);
                arrayResized?.Invoke(array.Length);
                result = true;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            for (int i = 0; i < array.Length; i++)
            {
                string cache = array[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{i}: ");
                cache = GUILayout.TextField(cache, myTextField);
                elementRightGUI?.Invoke(i);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (cache != array[i])
                {
                    array[i] = cache;
                    onElementChange?.Invoke(i, cache);
                    result = true;
                }
            }

            return result;
        }

        public static bool DrawArray(string label, ref object[] array)
        {
            bool result = false;
            GUILayout.Label(label);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (Drawer.Button("+"))
                Array.Resize(ref array, array.Length + 1);
            if (array.Length > 0 && Drawer.Button("-"))
                Array.Resize(ref array, array.Length - 1);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            for (int i = 0; i < array.Length; i++)
                result |= DrawObject($"{i}: ", ref array[i]);
            GUILayout.EndVertical();
            return result;
        }

        public static bool DrawArray(ref string[] array)
        {
            bool result = false;
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (Drawer.Button("+"))
                Array.Resize(ref array, array.Length + 1);
            if (array.Length > 0 && Drawer.Button("-"))
                Array.Resize(ref array, array.Length - 1);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            for (int i = 0; i < array.Length; i++)
                result |= DrawString($"{i}: ", ref array[i]);
            GUILayout.EndVertical();
            return result;
        }

        private static bool isImageInited = false; 

        public static Texture2D textureSelected;
        public static Texture2D textureUnselected;

        public static Texture2D ali_Left;
        public static Texture2D ali_Right;
        public static Texture2D ali_Center;
        public static Texture2D ali_Justified;
        public static Texture2D ali_Flush;
        public static Texture2D ali_Geometry_Center;
        public static Texture2D ali_Top;
        public static Texture2D ali_Middle;
        public static Texture2D ali_Bottom;
        public static Texture2D ali_Baseline;
        public static Texture2D ali_Midline;
        public static Texture2D ali_Capline;

        public static Texture2D ali_Unknown;

        public static Texture2D openFolder;

        public static void InitializeImages() {
            if(isImageInited) {
                return;
            }

            dulgray = new Texture2D(1, 1);
            dulgray.SetPixel(0, 0, new Color(0.4f, 0.4f, 0.4f));
            dulgray.Apply();

            gray = new Texture2D(1, 1);
            gray.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f));
            gray.Apply();

            jittengray = new Texture2D(1, 1);
            jittengray.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f));
            jittengray.Apply();

            tfgray = new Texture2D(1, 1);
            tfgray.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f));
            tfgray.Apply();

            veryjittengray = new Texture2D(1, 1);
            veryjittengray.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f));
            veryjittengray.Apply();

            outlineimg = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            outlineimg.filterMode = FilterMode.Point;
            outlineimg.LoadImage(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,64,0,0,0,64,4,3,0,0,0,88,71,108,237,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,6,80,76,84,69,127,127,127,26,26,26,75,51,228,8,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,0,34,73,68,65,84,72,199,99,96,160,28,8,226,5,163,10,70,21,140,42,24,85,48,170,96,84,193,168,130,193,166,128,82,0,0,54,35,119,137,193,182,173,24,0,0,0,0,73,69,78,68,174,66,96,130});

            black = new Texture2D(1, 1);
            black.SetPixel(0, 0, Color.black);
            black.Apply();

            textureSelected = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,20,0,0,0,20,8,6,0,0,0,141,137,29,13,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,1,241,73,68,65,84,56,79,189,148,203,74,195,64,20,134,219,84,221,121,65,17,188,226,21,241,130,224,11,136,226,82,138,212,251,243,40,40,168,79,34,184,241,13,4,247,162,184,19,177,218,42,86,20,117,103,197,146,250,253,201,73,77,76,16,23,226,15,95,207,153,127,206,156,78,39,211,164,254,90,105,139,137,170,86,171,13,48,70,218,233,59,169,34,92,58,142,243,238,15,127,41,215,117,219,96,15,158,104,24,145,60,216,133,118,43,143,40,182,67,10,103,8,7,208,6,103,233,116,250,144,120,77,47,135,56,0,89,188,41,198,143,196,53,56,194,75,22,205,230,224,29,158,33,203,162,216,23,170,49,172,90,77,153,124,214,166,162,98,178,29,244,115,84,56,110,118,208,160,31,175,143,152,49,91,245,19,86,127,135,223,98,246,151,152,216,1,105,193,44,121,203,80,240,92,95,121,88,180,105,205,175,208,76,231,186,107,150,47,188,58,76,237,236,132,220,251,153,228,75,224,106,65,88,88,21,66,206,214,57,140,207,64,187,172,147,231,9,99,210,138,55,53,38,205,144,23,229,37,137,185,43,130,30,146,214,110,128,52,164,177,103,162,30,139,121,139,189,208,237,167,137,210,121,118,40,225,41,231,65,191,42,210,48,120,154,174,62,40,86,77,236,9,135,84,53,188,90,69,228,197,160,225,173,197,17,125,208,75,247,46,216,109,146,10,212,148,44,31,164,86,205,175,252,33,98,92,207,25,60,192,247,135,242,161,226,176,228,65,214,214,233,161,156,66,129,188,118,165,60,97,110,129,84,187,22,20,229,24,95,66,197,184,128,121,155,214,154,224,218,108,155,245,37,204,86,208,69,125,129,9,179,131,93,116,65,39,121,237,92,201,71,225,21,191,68,108,54,59,42,38,103,160,12,122,41,228,194,13,2,225,233,11,214,172,217,27,76,219,148,167,216,2,43,216,231,208,181,163,115,114,189,28,116,224,186,1,195,176,196,220,56,115,247,228,235,188,202,142,137,63,139,166,77,176,13,58,236,136,240,138,160,191,105,163,149,71,20,219,97,88,172,207,128,94,89,186,180,21,184,209,149,2,229,255,161,84,234,19,174,59,50,113,108,43,54,132,0,0,0,0,73,69,78,68,174,66,96,130});
            textureUnselected = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,20,0,0,0,20,8,6,0,0,0,141,137,29,13,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,1,165,73,68,65,84,56,79,189,148,77,75,66,65,20,134,245,106,237,250,20,23,66,16,125,16,169,155,254,64,24,173,37,42,202,126,81,66,246,75,130,22,5,253,128,160,125,36,109,35,45,139,138,162,218,165,16,218,243,122,207,69,47,215,207,136,94,120,56,51,239,156,57,206,204,29,39,244,215,10,91,236,168,70,163,49,10,73,154,9,215,9,85,224,198,113,156,154,219,29,80,245,122,61,6,135,240,70,65,159,228,65,1,226,150,238,83,96,133,36,102,8,199,16,131,98,56,28,62,37,150,169,229,16,231,32,139,183,66,255,149,152,131,115,188,206,162,216,58,212,224,29,178,76,10,252,160,10,195,174,229,84,105,175,217,144,95,12,198,65,219,81,98,202,236,174,34,39,109,249,143,20,157,52,187,37,6,14,64,218,48,171,175,200,221,161,152,206,181,96,150,43,188,40,166,86,118,73,187,231,151,111,23,185,14,115,138,160,85,70,229,233,160,53,144,228,112,167,104,158,17,27,242,6,17,185,117,194,9,36,168,49,43,175,89,16,205,88,44,89,28,88,20,45,129,118,181,160,190,87,208,219,166,126,113,40,181,29,81,51,122,5,31,44,46,89,28,70,243,20,213,49,221,186,93,68,127,132,131,125,249,229,71,185,130,123,218,17,179,93,97,238,131,180,101,86,95,145,235,93,155,188,89,45,97,78,131,46,234,7,164,205,238,42,234,44,195,39,185,207,196,9,179,253,98,48,3,85,208,163,176,9,129,237,227,105,155,57,43,246,5,171,54,212,84,96,130,37,28,113,19,116,183,174,105,235,113,208,129,235,6,44,194,54,99,41,198,158,104,239,241,148,93,16,123,139,162,227,144,7,29,182,79,120,21,208,223,116,204,210,125,234,249,69,153,31,1,61,89,186,180,223,112,199,234,202,160,246,127,40,20,250,1,233,9,112,215,122,210,129,174,0,0,0,0,73,69,78,68,174,66,96,130});

            ali_Left = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,59,73,68,65,84,40,21,99,96,32,21,252,39,1,144,106,54,68,61,49,22,144,103,50,76,23,49,54,32,171,129,233,35,158,70,214,141,206,38,222,20,124,42,209,77,197,199,199,103,14,110,57,124,38,194,228,144,117,3,0,220,141,235,21,156,62,195,48,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Right = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,57,73,68,65,84,40,21,99,96,32,7,252,39,1,144,99,62,66,15,49,22,33,84,19,203,34,198,84,100,53,196,154,139,95,29,178,137,232,108,252,58,113,201,162,155,130,143,143,203,12,226,196,241,153,12,147,3,0,243,20,235,21,144,83,40,29,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Center = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,53,73,68,65,84,40,21,99,96,32,21,252,39,17,144,106,62,68,61,177,150,144,103,58,178,46,92,54,33,171,33,143,141,203,100,116,113,218,154,14,179,141,60,91,64,186,96,38,224,162,97,38,3,0,249,14,231,25,147,183,207,136,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Justified = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,33,73,68,65,84,40,21,99,96,32,21,252,39,17,144,106,62,3,3,137,22,252,31,181,129,152,16,163,94,40,225,50,9,0,233,220,7,8,207,13,201,222,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Flush = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,31,73,68,65,84,40,21,99,96,32,21,252,39,17,144,106,62,3,3,137,22,252,31,181,129,152,16,163,125,40,1,0,20,206,30,240,110,195,221,86,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Geometry_Center = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,66,73,68,65,84,40,21,99,248,255,255,255,127,6,34,1,88,45,76,3,136,198,7,64,102,130,228,33,4,57,54,192,76,192,102,11,204,60,202,108,192,102,50,178,24,204,5,131,217,15,200,33,129,238,118,100,57,242,253,128,108,42,54,54,44,148,0,198,17,255,1,75,231,158,42,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Top = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,31,73,68,65,84,40,21,99,96,32,21,252,39,17,144,106,62,3,3,137,22,252,31,140,54,144,238,166,161,175,3,0,109,171,143,113,235,141,106,206,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Middle = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,29,73,68,65,84,40,21,99,96,24,6,224,63,137,128,116,47,147,104,193,255,193,104,3,233,110,34,81,7,0,11,141,143,113,27,214,77,68,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Bottom = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,26,73,68,65,84,40,21,99,96,24,5,180,8,129,255,36,2,210,221,64,162,5,255,105,111,3,0,169,96,143,113,164,0,106,130,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Baseline = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,88,73,68,65,84,40,21,165,142,75,14,0,33,8,67,123,255,75,99,48,211,166,226,4,19,101,67,233,227,7,188,70,68,68,221,241,231,169,167,133,234,250,4,155,153,201,107,77,31,4,204,4,181,166,63,7,18,102,200,4,180,200,189,205,244,33,215,26,170,166,215,174,151,55,18,156,66,23,58,145,75,58,46,198,107,50,110,196,0,71,154,135,121,228,19,250,118,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Midline = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,83,73,68,65,84,40,21,181,141,65,14,192,32,16,2,249,255,167,107,104,132,80,106,140,61,148,203,178,131,172,184,62,10,45,246,79,152,223,172,10,14,219,232,177,166,242,222,197,161,64,83,65,239,226,119,129,33,101,8,248,80,178,23,204,82,122,151,26,230,158,222,133,157,249,175,192,203,212,227,247,201,142,199,0,137,188,187,69,126,37,21,114,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Capline = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,87,73,68,65,84,40,21,157,142,219,14,128,48,12,66,249,255,159,118,193,4,130,104,118,177,47,165,156,209,14,215,97,161,139,249,29,207,111,190,2,134,45,244,88,93,188,103,249,16,80,23,232,89,254,29,32,100,217,4,188,40,189,151,153,161,212,14,181,153,115,234,199,55,8,86,229,11,51,193,37,51,110,166,107,54,254,136,1,140,25,183,73,53,167,3,97,0,0,0,0,73,69,78,68,174,66,96,130});
            ali_Unknown = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,52,73,68,65,84,40,21,99,96,160,20,252,71,3,120,205,3,169,69,87,128,77,12,174,6,155,36,54,49,58,106,128,91,5,101,224,117,14,69,138,65,154,73,50,29,221,54,162,248,84,179,1,0,31,60,63,193,68,133,15,100,0,0,0,0,73,69,78,68,174,66,96,130});
            openFolder = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,0,58,73,68,65,84,40,83,99,248,143,5,48,224,2,216,36,177,137,129,1,46,9,116,219,64,0,167,98,116,48,0,138,145,221,9,147,39,168,24,93,28,167,98,172,114,112,187,144,76,67,86,132,161,129,16,128,105,0,0,113,215,151,105,21,154,86,237,0,0,0,0,73,69,78,68,174,66,96,130});

            isImageInited = true;
        }

        public static Texture2D Base64ToTexture(string base64) {
            byte[] imageBytes = System.Convert.FromBase64String(base64);

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.LoadImage(imageBytes);
            return texture;
        }
        public static Texture2D CreateTextureFromByte(byte[] bytes) {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.LoadImage(bytes);
            return texture;
        }

        public static bool DrawBool(string label, ref bool value)
        {
            bool prev = value;

            GUILayout.BeginHorizontal();

            if (Main.Settings.useLegacyTheme)
            {
                value = GUILayout.Toggle(value, "");
            }
            else
            {
                var old = GUI.backgroundColor;
                GUI.backgroundColor = Color.clear;
                var newskin = new GUIStyle(GUI.skin.button);
                newskin.fontSize = 16;
                newskin.margin = new RectOffset(0, 0, 4, 0);
                newskin.padding = new RectOffset(0, 0, 0, 0);

                if (GUILayout.Button(value ? textureSelected : textureUnselected, newskin))
                {
                    value = !value;
                }

                GUI.backgroundColor = old;
            }

            if (GUILayout.Button(label, GUI.skin.label))
            {
                value = !value;
            }


            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            return prev != value;
        }

        public static bool DrawByte(string label, ref byte value)
        {
            string str = value.ToString();
            bool result = DrawString(label, ref str);
            value = StringConverter.ToUInt8(str);
            return result;
        }

        public static bool DrawDouble(string label, ref double value)
        {
            string str = value.ToString();
            bool result = DrawString(label, ref str);
            value = StringConverter.ToDouble(str);
            return result;
        }

        public static bool DrawEnum<T>(string label, ref T @enum, int unique = 0) where T : Enum
        {
            int current = EnumHelper<T>.IndexOf(@enum);
            string[] names = EnumHelper<T>.GetNames();
            bool result = SelectionPopup(ref current, names, "");
            @enum = EnumHelper<T>.GetValues()[current];
            return result;
        }

        public static bool DrawEnumPlus<T>(string label, ref T @enum, Func<string, string> translator, int unique = 0)
            where T : Enum
        {
            int current = EnumHelper<T>.IndexOf(@enum);
            string[] names = EnumHelper<T>.GetNames();
            string[] translatedNames = names.Select(name => translator(name)).ToArray();

            bool result =
                SelectionPopup(ref current, translatedNames, "");

            @enum = EnumHelper<T>.GetValues()[current];
            return result;
        }

        public static bool DrawTags(ref string value)
        {
            var tags = TagManager.tags.Keys.ToList();
            tags.Sort();
            var selected = tags.IndexOf(value);

            var tooltip = new Dictionary<string, string>();
            foreach (var toolt in Tags.Tooltip.tooltip)
            {
                tooltip[toolt.Key] = Main.Lang.Get("TOOLTIP_" + toolt.Key.ToUpper(), toolt.Value);
            }


            SelectionPopupWithTooltip(ref selected, tags.ToArray(), "", tooltip);
            value = tags[selected];
            return selected != tags.IndexOf(value);
        }

        public static bool DrawInt16(string label, ref short value)
        {
            string str = value.ToString();
            bool result = DrawString(label, ref str);
            value = StringConverter.ToInt16(str);
            return result;
        }

        public static bool DrawInt32(string label, ref int value)
        {
            string str = value.ToString();
            bool result = DrawString(label, ref str);
            value = StringConverter.ToInt32(str);
            return result;
        }

        public static bool NeoDrawInt32(string label, ref int value, ref string field, ref int error, int errorBit) {
            bool hasError = (error & (1 << errorBit)) != 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            GUILayout.Space(4f);

            bool changed = false;
            int currentValue = value;
            GUILayout.Space(8f);
            if(hasError) {
                GUI.color = new Color(1f, 0.5f, 0.5f);
            }
            string newField = GUILayout.TextField(field, 9, myTextField, GUILayout.Width(100f));
            if(newField != field) {
                field = newField;
                if(int.TryParse(field, out int parsedValue)) {
                    currentValue = parsedValue;
                    changed = true;
                    error &= ~(1 << errorBit);
                } else {
                    error |= (1 << errorBit);
                }
            }
            if(hasError) {
                GUI.color = Color.white;
                GUILayout.Space(8f);
                GUILayout.Label("<color=#FF8888>!!</color>");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            value = currentValue;

            return changed;
        }

        public static bool DrawInt64(string label, ref long value)
        {
            string str = value.ToString();
            bool result = DrawString(label, ref str);
            value = StringConverter.ToInt64(str);
            return result;
        }

        public static void DrawObject(string label, object value)
        {
            if (value == null) return;
            if (value is IDrawable drawable)
            {
                drawable.Draw();
                return;
            }

            Type t = value.GetType();
            if (!t.IsPrimitive && t != typeof(string)) return;
            var fields = t.GetFields();
            foreach (var field in fields)
            {
                var fValue = field.GetValue(value);
                if (DrawObject(field.Name, ref fValue))
                    field.SetValue(value, fValue);
            }

            var props = t.GetProperties();
            foreach (var prop in props.Where(p => p.CanRead && p.CanWrite))
            {
                var pValue = prop.GetValue(value);
                if (DrawObject(prop.Name, ref pValue))
                    prop.SetValue(value, pValue);
            }
        }

        public static bool DrawObject(string label, ref object obj)
        {
            bool result = false;
            switch (obj)
            {
                case bool bb:
                    result = DrawBool(label, ref bb);
                    obj = bb;
                    break;
                case sbyte sb:
                    result = DrawSByte(label, ref sb);
                    obj = sb;
                    break;
                case byte b:
                    result = DrawByte(label, ref b);
                    obj = b;
                    break;
                case short s:
                    result = DrawInt16(label, ref s);
                    obj = s;
                    break;
                case ushort us:
                    result = DrawUInt16(label, ref us);
                    obj = us;
                    break;
                case int i:
                    result = DrawInt32(label, ref i);
                    obj = i;
                    break;
                case uint ui:
                    result = DrawUInt32(label, ref ui);
                    obj = ui;
                    break;
                case long l:
                    result = DrawInt64(label, ref l);
                    obj = l;
                    break;
                case ulong ul:
                    result = DrawUInt64(label, ref ul);
                    obj = ul;
                    break;
                case float f:
                    result = DrawSingle(label, ref f);
                    obj = f;
                    break;
                case double d:
                    result = DrawDouble(label, ref d);
                    obj = d;
                    break;
                case string str:
                    result = DrawString(label, ref str);
                    obj = str;
                    break;
                default:
                    GUILayout.Label($"{label}{obj}");
                    break;
            }

            return result;
        }

        public static bool DrawSByte(string label, ref sbyte value)
        {
            string str = value.ToString();
            bool result = DrawString(label, ref str);
            value = StringConverter.ToInt8(str);
            return result;
        }

        public static bool DrawSingle(string label, ref float value)
        {
            string str = value.ToString();
            bool result = DrawString(label, ref str);
            value = StringConverter.ToFloat(str);
            return result;
        }

        public static bool NeoDrawSingle(string label, ref float value, ref string field, ref int error, int errorBit)
        {
            bool hasError = (error & (1 << errorBit)) != 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            GUILayout.Space(4f);

            bool changed = false;
            float currentValue = value;
            GUILayout.Space(8f);
            if(hasError) {
                GUI.color = new Color(1f, 0.5f, 0.5f);
            }
            string newField = GUILayout.TextField(field, 9, myTextField, GUILayout.Width(100f));
            if(newField != field) {
                field = newField;
                if(float.TryParse(field, out float parsedValue)) {
                    currentValue = parsedValue;
                    changed = true;
                    error &= ~(1 << errorBit);
                } else {
                    error |= (1 << errorBit);
                }
            }
            if(hasError) {
                GUI.color = Color.white;
                GUILayout.Space(8f);
                GUILayout.Label("<color=#FF8888>!!</color>");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            value = currentValue;

            return changed;
        }

        public static bool DrawString(string label, ref string value, bool textArea = false)
        {
            string prev = value;
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            if (!textArea)
                value = GUILayout.TextField(value, myTextField);
            else value = GUILayout.TextArea(value, myTextField);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return prev != value;
        }

        public static bool DrawOnlyString(ref string value, bool textArea = false) {
            string prev = value;
            if(!textArea)
                value = GUILayout.TextField(value, myTextField);
            else
                value = GUILayout.TextArea(value, myTextField);
            GUILayout.FlexibleSpace();
            return prev != value;
        }

        public static bool DrawCodeEditor(string label, string id, ref string value)
        {
            string prev = value;
            GUILayout.Label(label);
            var sk = new GUIStyle(GUI.skin.label);

            sk.margin = new RectOffset(0, 0, 0, 0);
            sk.wordWrap = false;
            sk.richText = false;
            value = codeEditor.Draw(value, sk, id);
            return prev != value;
        }

        public static bool DrawToggleGroup(string[] labels, bool[] toggleGroup)
        {
            bool result = false;
            for (int i = 0; i < labels.Length; i++)
                if (DrawBool(labels[i], ref toggleGroup[i]))
                {
                    result = true;
                    for (int j = 0; j < toggleGroup.Length; j++)
                        if (j == i) continue;
                        else toggleGroup[j] = false;
                    break;
                }

            return result;
        }

        public static bool DrawUInt16(string label, ref ushort value)
        {
            string str = value.ToString();
            bool result = DrawString(label, ref str);
            value = StringConverter.ToUInt16(str);
            return result;
        }

        public static bool DrawUInt32(string label, ref uint value)
        {
            string str = value.ToString();
            bool result = DrawString(label, ref str);
            value = StringConverter.ToUInt32(str);
            return result;
        }

        public static bool DrawUInt64(string label, ref ulong value)
        {
            string str = value.ToString();
            bool result = DrawString(label, ref str);
            value = StringConverter.ToUInt64(str);
            return result;
        }

        public static bool DrawAlignment(ref TextAlignmentOptions value) {
            if(value == TextAlignmentOptions.Converted) {
                GUI.color = Color.cyan;
                ButtonImage(ali_Unknown, GUILayout.Width(404));
                GUI.color = Color.white;
                return false;
            }

            int oldvalue = (int)value;
            int newvalue = oldvalue;

            GUILayout.BeginHorizontal();

            // Left 0
            GUI.color = (((int)value & (1 << 0)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Left, GUILayout.Width(40))) {
                newvalue &= ~0xFF;         // clear 0~7
                newvalue |= (1 << 0);
            }

            // Center 1
            GUI.color = (((int)value & (1 << 1)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Center, GUILayout.Width(40))) {
                newvalue &= ~0xFF;
                newvalue |= (1 << 1);
            }

            // Right 2
            GUI.color = (((int)value & (1 << 2)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Right, GUILayout.Width(40))) {
                newvalue &= ~0xFF;
                newvalue |= (1 << 2);
            }

            // Justified 3
            GUI.color = (((int)value & (1 << 3)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Justified, GUILayout.Width(40))) {
                newvalue &= ~0xFF;
                newvalue |= (1 << 3);
            }

            // Flush 4
            GUI.color = (((int)value & (1 << 4)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Flush, GUILayout.Width(40))) {
                newvalue &= ~0xFF;
                newvalue |= (1 << 4);
            }

            // Geometry_Center 5
            GUI.color = (((int)value & (1 << 5)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Geometry_Center, GUILayout.Width(40))) {
                newvalue &= ~0xFF;
                newvalue |= (1 << 5);
            }

            GUILayout.Space(20);

            // Top 8
            GUI.color = (((int)value & (1 << 8)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Top, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);  // clear 8~15
                newvalue |= (1 << 8);
            }

            // Middle 9
            GUI.color = (((int)value & (1 << 9)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Middle, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);
                newvalue |= (1 << 9);
            }

            // Bottom 10
            GUI.color = (((int)value & (1 << 10)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Bottom, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);
                newvalue |= (1 << 10);
            }

            // Baseline 11
            GUI.color = (((int)value & (1 << 11)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Baseline, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);
                newvalue |= (1 << 11);
            }

            // Midline 12
            GUI.color = (((int)value & (1 << 12)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Midline, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);
                newvalue |= (1 << 12);
            }

            // Capline 13
            GUI.color = (((int)value & (1 << 13)) != 0) ? Color.cyan : Color.white;
            if(ButtonImage(ali_Capline, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);
                newvalue |= (1 << 13);
            }

            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            if(Enum.IsDefined(typeof(TextAlignmentOptions), newvalue) && newvalue != oldvalue) {
                value = (TextAlignmentOptions)newvalue;
                return true;
            }

            return false;
        }

        public static bool DrawSelectFont(ref string fontPath) {
            bool result = false;
            GUILayout.BeginHorizontal();
            if(ButtonImage(openFolder, GUILayout.Width(40))) {
                var extensions = new[]
                {
                    new ExtensionFilter("Font Files", "ttf", "otf"),
                    new ExtensionFilter("All Files", "*")
                };

                string baseDir = Path.Combine(Main.Mod.Path, "Overlayer");
                string[] paths = StandaloneFileBrowser.OpenFilePanel(
                    Main.Lang.Get("SELECT_FONT_FILE", "Select Font File"),
                    baseDir,
                    extensions,
                    false
                );

                if(paths.Length > 0) {
                    string path = paths[0];

                    if(path.StartsWith(Main.Mod.Path, StringComparison.OrdinalIgnoreCase)) {
                        path = path.Replace(Main.Mod.Path, "{ModDir}")
                                   .Replace("\\", "/");
                    }

                    fontPath = path;
                    result = true;
                }
            }

            result |= Drawer.DrawOnlyString(ref fontPath);
            GUILayout.EndHorizontal();
            return result;
        }

        public static void Tooltip(string text, bool ignoreWidth = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                GUI.Box(new Rect(0, 0, 0, 0), "");
            }
            else
            {
                Vector2 mousePosition = Event.current.mousePosition;

                Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
                Rect labelPosition = new Rect(mousePosition.x, mousePosition.y - 40, 0, 0);

                if (!ignoreWidth)
                {
                    var windowwidth = ((Rect)AccessTools.Field(typeof(UnityModManager.UI), "mWindowRect")
                            .GetValue(UnityModManager.UI.Instance))
                        .width;
                    var scroll = (Vector2[])AccessTools.Field(typeof(UnityModManager.UI), "mScrollPosition")
                        .GetValue(UnityModManager.UI.Instance);
                    windowwidth += scroll[UnityModManager.UI.Instance.tabId].x;
                    if (labelPosition.x + textSize.x + 20 + 20 > windowwidth)
                    {
                        labelPosition.x = windowwidth - textSize.x - 20 - 20;
                    }
                }

                labelPosition.width = textSize.x + 20;
                labelPosition.height = textSize.y + 20;
                GUI.Box(labelPosition, "", RGUIStyle.darkWindow);

                labelPosition.x += 10;
                labelPosition.y += 10;
                GUI.Label(labelPosition, text);
            }
        }


        public static CodeEditor.CodeEditor codeEditor = new CodeEditor.CodeEditor("OverlayerCodeEditor",
            new CodeTheme()
            {
                background = "#333333",
                linenumbg = "#222222",
                color = "#FFFFFF",
                selection = "#264F78",
                cursor = "#D4D4D4"
            });
        

        public static Regex highlight = new Regex("{(.*?)}", RegexOptions.Compiled);
        public static Regex color = new Regex("<<b></b>color=(.*?)>", RegexOptions.Compiled);
        public static GUIStyle myButton;
        public static GUIStyle myTextField;
        public static GUIStyle mySlider;
        public static GUIStyle myThumb;

        public static void SetStyle(bool legacy) {
            if(legacy) {
                myButton.normal.background = GUI.skin.button.normal.background;
                myButton.active.background = GUI.skin.button.active.background;
                myButton.hover.background = GUI.skin.button.hover.background;
                myTextField.normal.background = GUI.skin.textField.normal.background;
                myTextField.focused.background = GUI.skin.textField.focused.background;
                myTextField.hover.background = GUI.skin.textField.hover.background;
                mySlider.normal.background = GUI.skin.horizontalSlider.normal.background;
                myThumb.normal.background = GUI.skin.horizontalSliderThumb.normal.background;
                myThumb.active.background = GUI.skin.horizontalSliderThumb.active.background;
                myThumb.hover.background = GUI.skin.horizontalSliderThumb.hover.background;
            } else if(isImageInited) {
                myButton.normal.background = gray;
                myButton.active.background = dulgray;
                myButton.hover.background = dulgray;
                myTextField.normal.background = tfgray;
                myTextField.focused.background = tfgray;
                myTextField.hover.background = tfgray;
                mySlider.normal.background = jittengray;
                myThumb.normal.background = gray;
                myThumb.active.background = dulgray;
                myThumb.hover.background = dulgray;
            }
        }

        public static Texture2D veryjittengray;
        public static Texture2D gray;
        public static Texture2D dulgray;
        public static Texture2D jittengray;
        public static Texture2D tfgray;
        public static Texture2D outlineimg;
        public static Texture2D black;

        static Drawer()
        {
            codeEditor.highlighter = str =>
            {
                str = str.Replace("<", "<<b></b>");

                var colorHighlighted = new List<string>();
                foreach (Match m in color.Matches(str))
                {
                    //Main.Logger.Log(m.Groups[1].Value);
                    if (!colorHighlighted.Contains(m.Groups[1].Value) && ColorUtility.TryParseHtmlString(m.Groups[1].Value, out _))
                    {
                        str = str.Replace("<<b></b>color=" + m.Groups[1].Value + ">",
                            "<<b></b>color=<color=" + m.Groups[1].Value + ">" + m.Groups[1].Value + "</color>>");
                        colorHighlighted.Add(m.Groups[1].Value);
                    }
                }

                var highlighted = new List<string>();
                
                foreach (Match match in highlight.Matches(str))
                {
                    if (highlighted.Contains(match.Groups[1].Value)) continue;
                    var name = match.Groups[1].Value.Split('(')[0].Split(':')[0];
                    if (TagManager.tags.ContainsKey(name))
                    {
                        if (name == "MovingMan" && Main.Settings.useMovingManEditor || name == "ColorRange" && Main.Settings.useColorRangeEditor || name == "EasedValue" && Main.Settings.useEasedValueEditor)
                        {
                            str = str.Replace("{" + match.Groups[1].Value + "}",
                                "<color=orange>{" + match.Groups[1].Value + "}</color>");
                        }
                        else if (name.EndsWith("Hex"))
                        {
                            try
                            {
                                var val = (string)TagManager.tags[name].Tag.Getter.Invoke(null,
                                    new object[] { "-1", Overlayer.Utils.Extensions.DefaultTrimStr });
                                str = str.Replace("{" + match.Groups[1].Value + "}",
                                    "<color=#" + val + ">{" + match.Groups[1].Value + "}</color>");
                            }
                            catch
                            {
                                str = str.Replace("{" + match.Groups[1].Value + "}",
                                    "<color=lightblue>{" + match.Groups[1].Value + "}</color>");
                            }
                        }
                        else
                        {
                            str = str.Replace("{" + match.Groups[1].Value + "}",
                                "<color=lightblue>{" + match.Groups[1].Value + "}</color>");
                        }
                    }
                    else
                    {
                        str = str.Replace("{" + match.Groups[1].Value + "}",
                            "<color=red>{" + match.Groups[1].Value + "}</color>");
                    }

                    highlighted.Add(match.Groups[1].Value);
                }

                return str;
            };

            InitializeImages();

            myButton = new GUIStyle(GUI.skin.button);
            myTextField = new GUIStyle(GUI.skin.textField);
            mySlider = new GUIStyle(GUI.skin.horizontalSlider);
            myThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
            SetStyle(Main.Settings.useLegacyTheme);
        }


        public static bool Button(string str, params GUILayoutOption[] options)
        {
            return GUILayout.Button(str, myButton, options);
        }

        public static bool ButtonImage(Texture2D texture, params GUILayoutOption[] options) {
            return GUILayout.Button(texture, myButton, options);
        }
    }
}