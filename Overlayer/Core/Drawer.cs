using DG.Tweening;
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

        public static bool DrawSize2(string label, ref Vector2 vec2, float lValue, float rValue, string uniqueID = null) {
            bool changed = false;
            GUILayout.Label($"<b>{label}</b>");
            Color old = GUI.color;
            GUI.color = new Color(1.0f, 0.68f, 0.68f);
            changed |= DrawSingleWithSlider(icon_LeftRight, "X", ref vec2.x, lValue, rValue, 300f);
            GUI.color = new Color(0.68f, 1.0f, 0.68f);
            changed |= DrawSingleWithSlider(icon_UpDown, "Y", ref vec2.y, lValue, rValue, 300f);
            GUI.color = old;
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

        public static bool DrawRotate3(string label, ref Vector3 vec3, float lValue, float rValue) {
            bool changed = false;
            GUILayout.Label($"<b>{label}</b>");
            Color old = GUI.color;
            GUI.color = new Color(1.0f, 0.68f, 0.68f);
            changed |= DrawSingleWithSlider(icon_XRotate, "X", ref vec3.x, lValue, rValue, 300f);
            GUI.color = new Color(0.68f, 1.0f, 0.68f);
            changed |= DrawSingleWithSlider(icon_YRotate, "Y", ref vec3.y, lValue, rValue, 300f);
            GUI.color = new Color(0.68f, 0.68f, 1.0f);
            changed |= DrawSingleWithSlider(icon_ZRotate, "Z", ref vec3.z, lValue, rValue, 300f);

            GUI.color = old;
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

        public static bool SelectionPopup(ref int selected, string[] options, Texture2D[] images, string label,
            params GUILayoutOption[] layoutOptions) {
            if(label != "") {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label);
            }

            var news = RGUI.SelectionPopup(selected, options, images, null, layoutOptions);
            var c = selected != news;

            selected = news;
            if(label != "")
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
            if (canEnableGradient && DrawBool(icon_Gradation, Main.Lang.Get("MISC_ENABLE_GRADIENT", "Enable Gradient"), ref ge))
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

        public static bool DrawSingleWithSlider(Texture2D icon, string label, ref float value, float lValue, float rValue, float width) {

            GUILayout.BeginHorizontal();
            GUILayout.Label(icon);
            GUILayout.Space(4f);
            float newValue = GUILayoutEx.NamedSliderContent(label, value, lValue, rValue, width);
            GUILayout.EndHorizontal();
            bool result = newValue != value;
            value = newValue;
            return result;
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

        public static Texture2D ease_Linear;
        public static Texture2D ease_InSine;
        public static Texture2D ease_OutSine;
        public static Texture2D ease_InOutSine;
        public static Texture2D ease_InQuad;
        public static Texture2D ease_OutQuad;
        public static Texture2D ease_InOutQuad;
        public static Texture2D ease_InCubic;
        public static Texture2D ease_OutCubic;
        public static Texture2D ease_InOutCubic;
        public static Texture2D ease_InQuart;
        public static Texture2D ease_OutQuart;
        public static Texture2D ease_InOutQuart;
        public static Texture2D ease_InQuint;
        public static Texture2D ease_OutQuint;
        public static Texture2D ease_InOutQuint;
        public static Texture2D ease_InExpo;
        public static Texture2D ease_OutExpo;
        public static Texture2D ease_InOutExpo;
        public static Texture2D ease_InCirc;
        public static Texture2D ease_OutCirc;
        public static Texture2D ease_InOutCirc;
        public static Texture2D ease_InElastic;
        public static Texture2D ease_OutElastic;
        public static Texture2D ease_InOutElastic;
        public static Texture2D ease_InBack;
        public static Texture2D ease_OutBack;
        public static Texture2D ease_InOutBack;
        public static Texture2D ease_InBounce;
        public static Texture2D ease_OutBounce;
        public static Texture2D ease_InOutBounce;

        public static Texture2D openFolder;

        public static Texture2D icon_Active;
        public static Texture2D icon_Color;
        public static Texture2D icon_Copy;
        public static Texture2D icon_Drag;
        public static Texture2D icon_Discord;
        public static Texture2D icon_Font;
        public static Texture2D icon_FontSize;
        public static Texture2D icon_FontAlternate;
        public static Texture2D icon_Gradation;
        public static Texture2D icon_Github;
        public static Texture2D icon_LineSpacing;
        public static Texture2D icon_Outline;
        public static Texture2D icon_OutlineWidth;
        public static Texture2D icon_Parse;
        public static Texture2D icon_Pause;
        public static Texture2D icon_Pencil;
        public static Texture2D icon_Play;
        public static Texture2D icon_Shadow;
        public static Texture2D icon_ShadowDilate;
        public static Texture2D icon_ShadowSoftness;
        public static Texture2D icon_UpDown;
        public static Texture2D icon_LeftRight;
        public static Texture2D icon_X;
        public static Texture2D icon_XRotate;
        public static Texture2D icon_YRotate;
        public static Texture2D icon_ZRotate;

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
            
            ease_Linear = CreateTextureFromByte(new byte[] { 0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x41,0x49,0x44,0x41,0x54,0x78,0x9C,0x8D,0xCD,0x41,0x0E,0x00,0x20,0x0C,0x02,0x41,0xF0,0xFF,0x7F,0xC6,0x43,0x8D,0x89,0xDA,0x56,0xF6,0x3C,0x01,0xC0,0x48,0x92,0x20,0x88,0x16,0x5C,0x0D,0x17,0x92,0x64,0x89,0x6F,0x58,0x2E,0x67,0x30,0xC5,0x15,0x7C,0x70,0x07,0x0F,0xFC,0x83,0x1B,0x3B,0x10,0x00,0x86,0x0B,0xE3,0x3E,0x86,0xD5,0xA2,0xD5,0x04,0xF9,0x44,0x30,0x02,0x0B,0xB1,0xFC,0x4B,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InSine = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x42,0x49,0x44,0x41,0x54,0x78,0x9C,0x95,0xCE,0x31,0x0E,0x00,0x20,0x08,0x43,0xD1,0x5F,0xEF,0x7F,0xE7,0x3A,0xE8,0x60,0x14,0x0C,0x74,0x24,0x2F,0xA5,0x50,0x8D,0xB1,0x4A,0xCE,0x76,0xAD,0x70,0x07,0x60,0x94,0x67,0x74,0x5A,0xDB,0x30,0x9C,0x91,0xB5,0x3D,0xF8,0x84,0x92,0x94,0xE2,0x1F,0x04,0x50,0xF4,0x32,0x82,0xAB,0xF9,0xBA,0x67,0x10,0x60,0x02,0xAF,0x2A,0x2D,0xF1,0xC0,0x8C,0x69,0x4A,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_OutSine = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x40,0x49,0x44,0x41,0x54,0x78,0x9C,0xA5,0xCF,0x31,0x12,0x00,0x20,0x08,0x03,0xC1,0x8B,0xFF,0xFF,0x73,0xAC,0xD4,0x02,0x54,0x46,0xA9,0x28,0x96,0x00,0xE2,0x50,0xB6,0x3D,0x7A,0x21,0x54,0x81,0x6B,0xE0,0x96,0x28,0x4D,0xD3,0xAA,0x30,0xE0,0x13,0x4C,0x93,0x77,0x30,0xAC,0xCF,0x9E,0x7A,0x86,0xE9,0x19,0xFF,0xEB,0x01,0x0C,0x35,0x08,0x74,0xDF,0x30,0x2D,0xF0,0x74,0x91,0x88,0x0E,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InOutSine = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x47,0x49,0x44,0x41,0x54,0x78,0x9C,0x95,0x90,0x41,0x0E,0xC0,0x30,0x08,0xC3,0x6C,0xFE,0xFF,0x67,0x7A,0xAA,0x84,0xD6,0x15,0xB6,0x1C,0xC1,0x0A,0x21,0x32,0x28,0x33,0x13,0x40,0xC4,0x2F,0xE0,0xD6,0x15,0xAE,0xA0,0x2A,0x40,0x4C,0x31,0x36,0xD8,0x9E,0x7F,0x46,0x68,0xE1,0xB7,0xF9,0x11,0xE3,0x97,0x63,0x07,0x47,0x05,0x27,0x33,0x8F,0x2E,0x9B,0xEF,0x83,0xB2,0x9B,0x6A,0x5A,0x36,0x22,0x2B,0xF5,0xE0,0x70,0xD3,0x6B,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InQuad = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x48,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0x90,0x41,0x0A,0x00,0x21,0x0C,0xC4,0x32,0xE2,0xFF,0xBF,0x3C,0x7B,0x51,0x29,0xA8,0x6B,0x05,0x73,0x2B,0x84,0x50,0x06,0xB2,0x18,0x2B,0xE5,0xD9,0xCE,0x47,0x9B,0x5C,0x9E,0x55,0xDD,0xE8,0xF7,0xB6,0xBC,0x2A,0x1E,0xDF,0x90,0x34,0x46,0x98,0xD6,0x88,0xC5,0x28,0x4E,0xE5,0x3F,0x11,0xA0,0xDE,0x6C,0x28,0x83,0x09,0xFE,0xAA,0xD8,0xF9,0x00,0xBF,0x9B,0x29,0xF6,0xE9,0x55,0xB7,0x7D,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_OutQuad = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x45,0x49,0x44,0x41,0x54,0x78,0x9C,0x8D,0x8E,0x41,0x0E,0xC0,0x20,0x0C,0xC3,0x1C,0xC4,0xFF,0xBF,0x9C,0x9D,0x86,0x8A,0x18,0x59,0x73,0xEA,0xC1,0xB2,0x2B,0xC2,0x6C,0xFB,0xBD,0x85,0xD0,0x1F,0x54,0x37,0x93,0x19,0x40,0xD2,0x12,0x8E,0x98,0x2E,0xE0,0x67,0xFE,0xF6,0xC2,0x66,0x4E,0xD0,0xF5,0x8D,0x98,0xEE,0xE4,0x0F,0xB8,0xC3,0x8D,0xB6,0x11,0xC0,0xD0,0x86,0x1F,0xB6,0xB8,0x29,0xF3,0xA8,0x67,0xCF,0xA1,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InOutQuad = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x47,0x49,0x44,0x41,0x54,0x78,0x9C,0x95,0x90,0x41,0x0E,0xC0,0x30,0x08,0xC3,0x6C,0xFE,0xFF,0x67,0x7A,0xAA,0x84,0xD6,0x15,0xB6,0x1C,0xC1,0x0A,0x21,0x32,0x28,0x33,0x13,0x40,0xC4,0x2F,0xE0,0xD6,0x15,0xAE,0xA0,0x2A,0x40,0x4C,0x31,0x36,0xD8,0x9E,0x7F,0x46,0x68,0xE1,0xB7,0xF9,0x11,0xE3,0x97,0x63,0x07,0x47,0x05,0x27,0x33,0x8F,0x2E,0x9B,0xEF,0x83,0xB2,0x9B,0x6A,0x5A,0x36,0x22,0x2B,0xF5,0xE0,0x70,0xD3,0x6B,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InCubic = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3E,0x49,0x44,0x41,0x54,0x78,0x9C,0x95,0x8E,0x41,0x0E,0x00,0x20,0x08,0xC3,0x0A,0xF1,0xFF,0x5F,0x9E,0x17,0x4D,0xF0,0xA0,0x8E,0xDD,0xC6,0x9A,0x06,0x70,0x23,0x94,0x0D,0xD6,0x04,0x25,0x01,0xD8,0x66,0x0B,0xDE,0xD6,0xD6,0x0B,0x5F,0xB3,0x6D,0xD5,0x4A,0xBD,0x8D,0x8E,0x2D,0x9E,0x63,0x44,0xD4,0x9E,0x9C,0xFD,0x0A,0x02,0x4C,0x6B,0x31,0x23,0xF6,0x9E,0x8A,0x0A,0xF9,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_OutCubic = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x42,0x49,0x44,0x41,0x54,0x78,0x9C,0x63,0x64,0xC0,0x02,0xFE,0xFF,0xFF,0xFF,0x1F,0x5D,0x8C,0x91,0x81,0x91,0x81,0x91,0x18,0x85,0x08,0x0D,0x78,0x14,0x32,0x32,0x32,0xA2,0xC8,0xB3,0x60,0x35,0x01,0x4D,0x11,0x56,0x77,0xE2,0x73,0x02,0x86,0x62,0x42,0x6A,0x98,0x88,0x55,0x48,0x92,0xA9,0x70,0x93,0xA9,0x6E,0x2A,0x44,0x31,0x03,0x03,0xD1,0x8A,0x01,0x86,0x27,0x25,0xF0,0x39,0xB4,0x95,0x31,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InOutCubic = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x41,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0x8E,0x31,0x0E,0x00,0x20,0x0C,0x02,0xC1,0xFF,0xFF,0x19,0x17,0x1D,0xB4,0x2D,0x76,0x90,0xA9,0x09,0xC7,0xA5,0x84,0x89,0x24,0xED,0x9B,0x20,0x86,0x83,0x8F,0x21,0x64,0xCA,0x95,0x9E,0x29,0x01,0xD3,0x37,0xDA,0x46,0x07,0x07,0xF3,0x17,0x6B,0x30,0xBF,0xAC,0xBC,0x01,0x92,0xAC,0xCD,0x75,0x17,0x32,0x01,0xC4,0x11,0x29,0xEB,0xA9,0x84,0x78,0x68,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InQuart = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3B,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0x8E,0x41,0x0A,0x00,0x20,0x00,0xC2,0x66,0xF4,0xFF,0x2F,0xDB,0x2D,0xA2,0xA2,0x0C,0xF2,0x3C,0xC7,0x20,0x9D,0x71,0x79,0x60,0x43,0xD0,0x36,0x40,0x6C,0x7E,0x86,0xE3,0x84,0xBF,0x19,0xA3,0x15,0x40,0x37,0x48,0x52,0x67,0xEA,0xFC,0x3E,0x67,0x68,0x2B,0x5F,0xAC,0x00,0x0D,0x93,0xF2,0x18,0x04,0x74,0x41,0x67,0xF4,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_OutQuart = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3F,0x49,0x44,0x41,0x54,0x78,0x9C,0x95,0x90,0x41,0x0A,0x00,0x20,0x0C,0xC3,0x52,0xF1,0xFF,0x5F,0xAE,0x17,0xBD,0x0C,0x26,0x5D,0x8E,0x6B,0x09,0x65,0xA2,0x60,0xDB,0xF5,0x06,0x20,0x84,0x92,0xE2,0x63,0x77,0x81,0x24,0x75,0x19,0xBE,0xFC,0xCC,0xF1,0x04,0x80,0x15,0x99,0xA6,0xD6,0xB1,0x79,0x36,0x23,0xFE,0x00,0x80,0x21,0x2E,0x1F,0xF3,0xFD,0x1D,0xF5,0xCA,0x35,0x64,0x5B,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InOutQuart = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x41,0x49,0x44,0x41,0x54,0x78,0x9C,0x9D,0x90,0x31,0x0E,0x00,0x20,0x0C,0x02,0x0F,0xD3,0xFF,0x7F,0x19,0xA7,0x2E,0x1A,0x6B,0x95,0xA9,0x03,0x5C,0x0A,0xA2,0x90,0x6D,0xE7,0x2D,0x84,0x3A,0xC6,0x54,0x54,0x64,0x00,0x49,0x47,0x60,0x49,0x7E,0x32,0x8E,0x56,0xFA,0xC7,0xDC,0x7E,0x61,0x23,0xDF,0x4A,0x45,0xBB,0x35,0x30,0x58,0x66,0xAC,0x76,0x9D,0x6E,0xFA,0x1C,0x03,0xEB,0xCE,0xAA,0x75,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InQuint = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3D,0x49,0x44,0x41,0x54,0x78,0x9C,0x95,0xCE,0x41,0x0E,0x00,0x20,0x0C,0x02,0xC1,0xC5,0xFF,0xFF,0x19,0x8F,0x55,0x63,0x2A,0x72,0x6A,0x9A,0x09,0x01,0xD2,0x18,0x8F,0x0F,0x1B,0xF7,0xE6,0xB1,0x6D,0x80,0x78,0xC6,0x37,0x8E,0x27,0x3C,0x9B,0x57,0xD8,0xE2,0x13,0x02,0xE8,0xF6,0xDC,0x80,0xA4,0x6A,0xAE,0xBB,0x85,0x00,0x13,0x19,0xAA,0x18,0x00,0x05,0xFF,0x9B,0x56,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_OutQuint = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3F,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0xCF,0x41,0x0A,0x00,0x20,0x0C,0x03,0xC1,0x8D,0xFF,0xFF,0x73,0x3C,0x8A,0x6D,0xC5,0x0A,0xE6,0x58,0x26,0x90,0x8A,0x10,0xDB,0x8E,0x37,0x00,0x21,0xD4,0x81,0xAB,0x50,0x40,0x49,0xAA,0xF0,0x48,0xED,0x03,0xDC,0x72,0x9B,0xF0,0x0C,0xD3,0x8C,0x6F,0xF8,0x2D,0xED,0xC7,0x00,0x0C,0x6D,0x3C,0x01,0xFB,0xCE,0x19,0xFA,0x7A,0xC6,0x32,0x47,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InOutQuint = CreateTextureFromByte(new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3C,0x49,0x44,0x41,0x54,0x78,0x9C,0x63,0x64,0xC0,0x03,0xFE,0xFF,0xFF,0xFF,0x1F,0xC6,0x66,0x64,0x60,0x64,0x60,0x24,0x46,0x21,0x41,0x80,0x4D,0x31,0x13,0xD1,0xBA,0x49,0x75,0x02,0x49,0x26,0x53,0xE6,0x0C,0x7C,0xA1,0x40,0x92,0xC9,0x2C,0xA4,0x84,0x27,0x13,0x03,0x23,0x6A,0xBC,0x30,0x32,0x32,0xE2,0x8C,0x28,0x00,0x38,0xCB,0x18,0x01,0xB2,0x6C,0x7C,0xC0,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            
            ease_InExpo = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x39,0x49,0x44,0x41,0x54,0x78,0x9C,0x63,0x60,0x20,0x16,0xFC,0x67,0xF8,0xCF,0x44,0x82,0x5A,0x86,0x01,0x06,0xFF,0xFF,0xFF,0x07,0xBB,0x81,0x68,0x37,0x33,0x90,0xAA,0x98,0x68,0x27,0x80,0x00,0x0B,0x31,0x8A,0xE0,0x8A,0xB1,0x09,0xE2,0x02,0x4C,0x0C,0x8C,0x8C,0x78,0x15,0x30,0x32,0x22,0x14,0x00,0x00,0x5E,0x3E,0x14,0x03,0x48,0xAD,0x63,0x5C,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_OutExpo = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3B,0x49,0x44,0x41,0x54,0x78,0x9C,0x63,0x64,0x40,0x02,0xFF,0xFF,0xFF,0xFF,0xCF,0x80,0x03,0x30,0x82,0x21,0x11,0x0A,0x61,0x80,0x85,0x01,0x0B,0x60,0x64,0x64,0x84,0x1B,0x82,0x15,0xFC,0x27,0xC2,0x64,0x92,0x14,0x32,0x11,0x65,0x1A,0x03,0x19,0x8A,0x69,0x08,0x88,0x0E,0x05,0xB0,0x62,0x30,0x22,0x0E,0x00,0x00,0x5B,0x81,0x15,0xFB,0xA7,0x71,0x4C,0xDF,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InOutExpo = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3C,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0x90,0xD1,0x0A,0x00,0x20,0x0C,0x02,0x75,0xF4,0xFF,0xBF,0x7C,0xBD,0x56,0xB0,0xD1,0xA8,0x7B,0x1B,0x8A,0x13,0xAD,0x04,0x80,0xF5,0xB6,0xAC,0xC8,0xCC,0x27,0x88,0x42,0x64,0x4F,0xFE,0x03,0x49,0xEA,0x75,0x67,0x75,0xCD,0x6F,0x54,0x2B,0x8C,0xCE,0x44,0x21,0xFB,0xFA,0xED,0x04,0x28,0x1B,0x15,0xFC,0x31,0x79,0x97,0xF1,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InCirc = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x42,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0xCE,0x51,0x0A,0x00,0x20,0x08,0x03,0xD0,0xAD,0xFB,0xDF,0x79,0x61,0x45,0x18,0x54,0x1A,0xB4,0x3F,0xF1,0x31,0x06,0x64,0x23,0xA8,0x3C,0x58,0xFC,0x8F,0xA4,0x56,0x9B,0x9E,0x91,0xC2,0x1A,0xAD,0x61,0x0C,0x7A,0x7C,0x6C,0xDE,0x35,0x32,0x42,0x24,0xA7,0xE1,0x6D,0x93,0x87,0x7D,0xC6,0x7A,0x1F,0xA1,0x3D,0x2A,0xFC,0x47,0x1F,0xFB,0x61,0xDA,0x96,0xF3,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_OutCirc = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x42,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0x8E,0x49,0x0A,0x00,0x30,0x08,0xC4,0x9A,0xFE,0xFF,0xCF,0x53,0x10,0x7A,0x70,0xA9,0x78,0x68,0xF4,0x34,0x06,0x1C,0x56,0x81,0x24,0xC5,0x0C,0x9B,0x81,0x78,0xA1,0x13,0x01,0x77,0xA7,0x12,0x09,0xD2,0x65,0xC7,0x80,0x87,0xE8,0xDE,0xAB,0xE9,0x9A,0xE4,0x89,0x97,0x6A,0xFC,0x63,0xDC,0xD5,0x64,0xDB,0x19,0x07,0xF4,0x08,0x21,0xF5,0x34,0x47,0x63,0x4E,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InOutCirc = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3F,0x49,0x44,0x41,0x54,0x78,0x9C,0x63,0x64,0xC0,0x03,0xFE,0xFF,0xFF,0xFF,0x1F,0xC6,0x66,0x04,0x43,0x22,0x14,0xC2,0x00,0x0B,0x03,0x01,0xC0,0xC8,0xC8,0x88,0xD3,0x40,0xBC,0x26,0x93,0xA4,0x90,0x89,0x28,0xDD,0x0C,0x64,0x28,0x26,0xDA,0x09,0x18,0x26,0x13,0xF2,0x14,0x0B,0xD1,0xBE,0x06,0x9B,0x8C,0x16,0x8C,0xF8,0xC2,0x15,0x00,0x6E,0xFA,0x1C,0x03,0x27,0x46,0x39,0x56,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InElastic = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x40,0x49,0x44,0x41,0x54,0x78,0x9C,0x63,0x60,0x20,0x16,0xFC,0x67,0xF8,0xCF,0x44,0x82,0x5A,0x86,0xA1,0x00,0xFE,0xFF,0xFF,0x8F,0xE2,0x50,0x16,0x74,0x01,0x18,0x60,0x64,0x64,0x64,0x44,0x17,0xC3,0x19,0x1A,0x30,0x43,0x90,0x0D,0xC3,0x1B,0x74,0x30,0xD3,0x61,0x1A,0x58,0xB0,0x59,0x87,0xCD,0xBD,0x10,0x41,0x30,0xC2,0x0F,0x60,0x1A,0x01,0x89,0x9F,0x21,0xF9,0xF0,0x88,0x31,0x83,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_OutElastic = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x40,0x49,0x44,0x41,0x54,0x78,0x9C,0xCD,0x8F,0x41,0x0A,0x00,0x20,0x08,0x04,0x77,0xA3,0xFF,0x7F,0x79,0x82,0x82,0xE8,0x60,0x68,0xB7,0x06,0x41,0x0F,0x8E,0xAC,0x92,0x24,0x00,0x65,0x20,0xDA,0x9E,0x59,0xC2,0x55,0xB4,0xDC,0xC3,0x23,0xC4,0x82,0xB3,0x85,0x93,0x1D,0xC3,0xB6,0xCF,0x9E,0x42,0xE5,0xD1,0x0F,0x78,0xCA,0xC9,0xAC,0x1A,0x03,0xDB,0xB7,0x1F,0xF4,0x93,0x13,0x63,0xA7,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InOutElastic = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x40,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0x90,0x31,0x0A,0x00,0x20,0x0C,0xC4,0x2E,0xFE,0xFF,0xCF,0xE7,0x20,0x82,0xB4,0x54,0x2C,0x98,0xAD,0x90,0x21,0x3D,0x54,0x60,0xDB,0xE7,0x8D,0xD0,0xA8,0xE4,0x88,0xE5,0x5A,0x06,0xD0,0x77,0x1C,0x7A,0x37,0xCF,0xCD,0xEA,0xCA,0x2D,0x48,0x7B,0x5E,0x56,0x48,0x19,0xD5,0x73,0x4B,0x6E,0xCC,0x39,0x01,0x4E,0x64,0x12,0x09,0x92,0x31,0xFF,0x29,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InBack = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3F,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0x8E,0x41,0x0A,0x00,0x20,0x10,0x02,0x35,0xFA,0xFF,0x97,0x8D,0xA2,0x43,0xD1,0x16,0x2E,0xE4,0x51,0x65,0x14,0x70,0x25,0xA8,0x24,0xBA,0xF8,0x2F,0x49,0x03,0x6B,0xDF,0xB0,0xCA,0x9A,0x54,0x4B,0x6B,0xF9,0x49,0x3E,0xA8,0xDD,0x88,0xA6,0x22,0xBF,0x66,0xBE,0x15,0x90,0xD7,0x90,0xDC,0xC3,0x06,0x54,0x0F,0x27,0xED,0xF9,0x76,0x8E,0xF0,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_OutBack = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x3B,0x49,0x44,0x41,0x54,0x78,0x9C,0xB5,0xCE,0xC1,0x0A,0x00,0x20,0x0C,0x02,0x50,0x8D,0xFE,0xFF,0x97,0x0D,0xC6,0xEA,0x10,0x23,0xDC,0xA1,0xE7,0x8E,0x22,0x23,0x2E,0x92,0x84,0x02,0x23,0x46,0x71,0x9B,0x28,0x90,0x7C,0x8F,0x28,0xC1,0x21,0xA3,0x38,0xDC,0x22,0x3A,0xAB,0x67,0xF9,0x8F,0xDE,0xBF,0x71,0x9E,0x05,0xC1,0xBD,0x25,0xEB,0xA4,0x21,0x12,0xE4,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InOutBack = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x43,0x49,0x44,0x41,0x54,0x78,0x9C,0x63,0x64,0xC0,0x03,0xFE,0xFF,0xFF,0xFF,0x1F,0xC6,0x66,0x04,0x43,0x22,0x14,0xC2,0x00,0x13,0x03,0x1E,0xC0,0x08,0x05,0x0C,0x84,0xC0,0x7F,0x2C,0x26,0x93,0xA4,0x90,0x89,0x28,0xDD,0x0C,0x64,0x28,0x26,0xDA,0x09,0x60,0x93,0x41,0x92,0xC4,0x7A,0x86,0x85,0x18,0x13,0xE1,0x26,0x33,0xA0,0x05,0x23,0xBE,0x70,0x05,0x00,0x81,0xB6,0x20,0x01,0x75,0xA9,0x72,0xE4,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InBounce = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x48,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0x90,0x5B,0x0A,0x00,0x20,0x08,0x04,0xD7,0xEE,0x7F,0xE7,0x0D,0xED,0x41,0x19,0xC5,0x7E,0x34,0x7F,0xC2,0x20,0xA3,0x80,0x00,0x49,0x82,0x60,0x51,0x64,0xC7,0x6D,0x69,0xAB,0x03,0x39,0xA1,0x23,0x67,0xFC,0x85,0xA9,0xF5,0xC8,0x88,0x6B,0x2E,0x07,0xD9,0x6D,0xCB,0x14,0xCC,0xA6,0x63,0xAF,0xB7,0xAC,0x62,0xCB,0xD8,0xE7,0x10,0xB2,0x34,0xA8,0xB0,0xC2,0x27,0xF5,0xD6,0x8A,0x2A,0x4B,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_OutBounce = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x44,0x49,0x44,0x41,0x54,0x78,0x9C,0x9D,0x90,0x41,0x0A,0x00,0x20,0x08,0x04,0x9D,0xFE,0xFF,0xE7,0x0D,0x42,0x3B,0x84,0x92,0x35,0x5E,0x44,0x96,0x75,0x15,0x3B,0x90,0x24,0x80,0xE8,0x63,0xCE,0xAA,0x44,0x6C,0x05,0x74,0x84,0xF8,0xA6,0xD4,0x55,0x4E,0xB5,0xE1,0x1A,0xE1,0x1F,0x35,0x5C,0xC7,0x8B,0xE1,0x12,0xB7,0xB3,0xB6,0x2E,0xDF,0xCE,0xC9,0x0B,0x2B,0x26,0x0E,0x20,0x2F,0xE6,0xEF,0x3B,0xD0,0x66,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});
            ease_InOutBounce = CreateTextureFromByte(new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x0B,0x00,0x00,0x00,0x0B,0x08,0x06,0x00,0x00,0x00,0xA9,0xAC,0x77,0x26,0x00,0x00,0x00,0x43,0x49,0x44,0x41,0x54,0x78,0x9C,0xAD,0x8E,0x49,0x0E,0x00,0x20,0x08,0xC4,0x66,0xFC,0xFF,0x9F,0xC7,0x93,0x06,0x45,0x96,0x83,0x3D,0x91,0x50,0x1A,0x88,0x04,0x49,0x5A,0x33,0x41,0x8C,0x4C,0xB6,0x08,0xFB,0x2E,0xAE,0xCA,0xD4,0x4B,0xD9,0xF2,0x7C,0x43,0x9D,0x1A,0x0A,0xD9,0x95,0xBF,0x54,0x8F,0x72,0xA7,0xC8,0x5B,0x22,0xC9,0xB8,0x1C,0xEF,0x1C,0x13,0xC4,0x11,0x29,0xEB,0x38,0xC2,0xB3,0x70,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82});

            openFolder = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,0,58,73,68,65,84,40,83,99,248,143,5,48,224,2,216,36,177,137,129,1,46,9,116,219,64,0,167,98,116,48,0,138,145,221,9,147,39,168,24,93,28,167,98,172,114,112,187,144,76,67,86,132,161,129,16,128,105,0,0,113,215,151,105,21,154,86,237,0,0,0,0,73,69,78,68,174,66,96,130});

            icon_Active = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,0,60,73,68,65,84,40,83,125,138,65,14,0,32,8,195,248,255,167,53,36,98,116,116,244,182,118,17,194,122,208,118,161,3,57,150,135,214,244,104,183,134,209,217,64,206,6,231,52,142,59,135,30,10,108,36,201,125,212,129,78,27,145,43,147,109,194,155,11,179,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Color = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,32,99,72,82,77,0,0,122,37,0,0,128,131,0,0,249,255,0,0,128,233,0,0,117,48,0,0,234,96,0,0,58,152,0,0,23,111,146,95,197,70,0,0,0,9,112,72,89,115,0,0,46,34,0,0,46,34,1,170,226,221,146,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,1,146,73,68,65,84,40,83,93,206,223,107,205,113,28,199,241,231,251,243,249,158,205,148,146,213,41,75,89,74,171,173,211,154,134,72,205,226,70,72,86,75,228,66,46,228,206,165,226,15,176,43,151,238,119,173,20,165,164,230,208,106,59,39,91,228,16,117,172,236,100,237,194,164,73,109,231,199,231,253,114,227,171,195,243,250,81,175,23,252,73,34,107,120,184,158,220,230,211,247,208,244,185,176,149,102,66,217,167,11,87,17,150,59,0,238,169,119,112,214,195,219,21,153,36,164,142,73,101,147,238,152,116,210,228,253,86,113,216,11,16,110,136,194,178,183,159,174,154,143,214,128,85,89,210,22,85,125,179,215,250,12,212,133,253,176,163,178,226,99,65,96,64,225,90,73,232,172,208,221,100,181,121,47,140,230,203,62,158,29,78,133,158,186,24,148,56,163,118,188,52,101,49,197,106,12,233,200,110,177,29,91,61,99,235,59,90,159,186,239,182,152,24,207,236,192,162,105,40,115,235,125,134,249,254,132,6,132,239,89,232,134,221,185,61,175,137,119,74,86,217,8,104,88,112,12,227,116,248,31,230,57,147,1,74,24,37,15,112,168,2,19,192,212,88,161,57,247,247,127,94,57,107,157,88,33,14,109,3,155,244,85,8,254,240,178,233,141,208,87,153,255,170,103,95,252,120,142,71,98,123,242,150,165,198,44,210,50,210,82,108,159,3,41,154,26,75,72,178,182,100,47,37,187,217,121,79,49,125,232,71,58,133,116,27,233,190,165,87,0,1,179,212,167,125,231,67,83,139,108,130,54,64,157,56,194,206,48,252,19,88,3,62,154,191,88,216,101,23,255,249,10,16,171,154,14,15,210,19,187,146,214,237,160,175,133,44,61,42,226,23,186,205,111,132,157,205,113,51,237,202,235,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Copy = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,91,73,68,65,84,40,83,141,144,65,14,0,33,8,3,25,255,255,103,246,98,13,52,172,113,78,10,5,10,132,145,153,233,49,1,64,13,220,196,98,44,240,70,45,87,63,19,42,150,102,221,196,49,52,91,122,84,27,108,142,170,112,10,94,161,46,234,227,39,218,132,63,27,2,32,114,227,201,216,11,123,238,201,70,59,72,12,167,171,184,205,15,199,49,63,252,149,217,232,243,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Drag = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,193,0,0,14,193,1,184,145,107,237,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,68,73,68,65,84,40,83,165,144,65,18,0,32,8,2,161,255,255,217,78,58,166,105,53,237,9,129,131,3,176,65,68,36,122,0,192,104,248,34,201,148,255,81,189,49,208,132,138,230,52,65,154,246,36,63,150,78,247,66,27,222,146,118,124,222,185,122,99,2,164,34,55,222,191,203,41,75,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Discord = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,16,0,0,0,16,8,6,0,0,0,31,243,255,97,0,0,0,9,112,72,89,115,0,0,14,196,0,0,14,196,1,149,43,14,27,0,0,1,49,73,68,65,84,56,141,173,146,177,46,68,65,20,134,191,123,35,217,69,44,130,16,44,61,149,82,52,104,104,36,68,233,9,84,84,18,222,64,34,17,219,104,117,106,79,161,161,217,78,167,32,36,138,141,69,130,172,253,20,59,43,147,155,43,65,252,205,204,124,231,156,127,102,206,12,252,183,212,84,45,231,240,178,154,102,121,18,37,76,3,91,192,42,208,7,108,3,221,33,92,7,142,128,39,224,12,168,36,73,82,141,221,123,213,123,127,174,7,117,0,160,125,164,29,96,248,23,55,29,4,118,1,18,181,11,184,1,250,127,97,64,184,206,120,10,172,100,138,5,26,57,5,141,16,107,171,7,88,75,129,229,76,226,6,208,5,156,68,236,148,86,67,215,51,38,75,168,213,168,57,181,175,99,232,92,196,23,35,126,23,241,171,20,152,140,28,75,234,76,152,207,71,124,33,20,79,1,67,17,159,64,253,200,60,209,147,122,145,225,77,245,82,173,103,223,51,81,95,129,66,112,124,11,99,129,124,189,3,77,160,24,214,205,14,90,63,110,31,40,1,47,192,49,112,13,204,2,101,90,191,245,22,56,7,70,129,205,96,240,12,236,181,27,51,162,30,170,53,181,242,205,238,168,7,234,163,90,81,199,242,18,138,106,49,167,182,29,47,168,157,223,197,255,164,79,161,154,5,127,52,128,9,180,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Font = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,192,0,0,14,192,1,106,214,137,9,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,60,73,68,65,84,40,83,99,252,255,255,255,127,6,18,0,19,186,0,33,192,8,179,129,145,145,145,17,38,136,79,140,100,27,176,130,255,80,128,46,206,64,142,13,195,65,3,60,156,25,144,194,26,29,32,199,7,201,54,0,0,216,197,28,4,162,20,76,243,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_FontSize = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,193,0,0,14,193,1,184,145,107,237,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,65,73,68,65,84,40,83,141,143,65,14,0,32,12,194,134,255,255,51,158,102,34,219,208,222,232,8,209,136,7,36,169,174,149,233,202,77,197,152,199,5,113,235,62,121,78,185,91,45,104,201,229,241,25,89,2,128,147,221,146,251,120,203,119,81,217,178,63,83,183,128,198,50,51,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_FontAlternate = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,32,99,72,82,77,0,0,122,37,0,0,128,131,0,0,249,255,0,0,128,233,0,0,117,48,0,0,234,96,0,0,58,152,0,0,23,111,146,95,197,70,0,0,0,9,112,72,89,115,0,0,46,34,0,0,46,34,1,170,226,221,146,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,61,73,68,65,84,40,83,189,144,65,10,0,32,8,4,215,254,255,231,233,100,200,18,133,4,205,69,150,149,65,148,190,0,224,57,145,164,81,203,27,107,25,32,34,194,237,149,190,57,109,62,183,120,233,57,57,158,241,244,141,22,19,123,247,55,217,31,241,206,166,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Gradation = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,32,99,72,82,77,0,0,122,37,0,0,128,131,0,0,249,255,0,0,128,233,0,0,117,48,0,0,234,96,0,0,58,152,0,0,23,111,146,95,197,70,0,0,0,9,112,72,89,115,0,0,46,34,0,0,46,34,1,170,226,221,146,0,0,0,67,73,68,65,84,40,83,237,209,177,17,64,80,20,69,193,155,83,130,18,244,163,5,197,252,81,139,46,84,162,2,49,43,38,48,243,114,27,159,236,68,65,74,12,97,14,107,156,71,52,209,233,197,34,251,37,19,99,216,194,31,63,226,146,247,165,47,55,28,80,23,186,195,198,66,231,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Github = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,16,0,0,0,16,8,6,0,0,0,31,243,255,97,0,0,0,9,112,72,89,115,0,0,16,38,0,0,16,38,1,131,182,76,21,0,0,1,59,73,68,65,84,56,141,141,146,189,78,2,65,20,133,207,44,134,159,106,43,9,133,46,9,165,210,99,107,167,111,96,77,183,79,32,143,129,141,49,60,129,20,246,26,10,123,10,67,43,13,177,34,89,11,178,38,132,206,207,230,174,142,147,93,244,38,155,157,123,230,126,103,238,252,56,5,1,196,146,174,36,157,75,234,154,252,38,233,89,210,189,115,46,15,25,31,78,129,13,213,177,1,210,50,208,1,147,61,96,24,19,192,249,6,35,155,120,5,238,128,188,4,202,129,91,96,105,249,168,128,187,192,206,196,177,105,109,96,0,28,218,55,0,218,54,119,99,181,59,32,57,144,148,74,106,90,51,45,73,114,206,101,146,50,111,151,239,222,184,101,255,166,164,84,192,194,28,63,129,147,202,19,254,217,238,169,213,2,44,4,108,45,201,254,130,61,147,204,152,109,36,169,110,122,125,31,20,196,55,19,73,90,91,18,3,253,127,172,222,151,20,91,186,22,48,245,174,234,17,104,236,129,27,86,83,196,84,192,101,113,133,192,139,189,133,107,160,231,129,61,211,150,252,142,139,162,96,6,124,0,103,86,148,3,71,158,65,82,242,176,102,126,107,29,96,5,60,216,184,17,180,30,5,240,10,232,132,251,75,128,185,87,84,171,48,152,3,199,85,135,84,3,134,192,19,16,121,186,51,109,232,27,75,210,23,234,141,241,221,64,141,228,76,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_LineSpacing = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,193,0,0,14,193,1,184,145,107,237,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,67,73,68,65,84,40,83,197,144,203,10,0,32,8,4,119,254,255,163,183,75,134,9,101,183,230,226,131,101,16,165,6,219,142,158,60,172,37,160,25,4,136,90,115,139,42,177,237,171,89,201,190,39,14,60,223,172,255,230,42,105,191,241,252,231,32,203,6,4,57,79,230,64,68,164,179,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Outline = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,9,112,72,89,115,0,0,46,35,0,0,46,35,1,120,165,63,118,0,0,1,8,73,68,65,84,40,145,125,145,45,203,194,96,24,133,207,243,232,254,128,109,205,95,97,177,152,150,6,162,176,160,40,140,253,192,5,203,146,136,8,242,76,22,86,86,150,21,147,89,12,238,122,195,139,83,195,60,112,210,125,157,251,131,91,146,144,68,191,223,39,73,18,14,135,3,143,199,131,251,253,206,110,183,99,181,90,97,140,225,197,73,18,195,225,144,178,44,233,146,115,14,223,247,255,3,158,231,81,85,21,0,121,158,51,30,143,219,110,65,16,180,181,211,233,132,181,22,197,113,220,194,31,99,191,92,215,53,0,243,249,28,229,121,14,192,104,52,234,12,204,102,51,0,178,44,195,60,159,79,172,181,50,198,232,151,0,221,110,55,89,224,39,248,169,166,105,100,157,115,146,164,201,100,210,9,46,151,75,73,146,115,78,90,44,22,0,84,85,213,121,195,249,124,6,32,12,67,212,235,245,40,138,2,128,186,174,137,162,168,5,215,235,117,11,239,247,251,247,227,124,223,231,120,60,118,62,110,187,221,50,24,12,222,129,151,163,40,98,179,217,112,189,94,185,92,46,164,105,202,116,58,253,90,239,15,97,2,17,168,231,209,18,78,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_OutlineWidth = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,192,0,0,14,192,1,106,214,137,9,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,60,73,68,65,84,40,83,165,80,65,14,0,32,8,130,254,255,103,186,34,171,53,141,27,136,48,5,126,33,73,169,1,0,83,112,35,201,50,95,78,94,40,155,167,122,79,111,37,207,204,183,195,92,159,37,183,145,223,72,190,1,15,152,27,255,10,235,208,28,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Parse =CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,58,73,68,65,84,40,83,99,96,128,130,255,80,0,227,227,18,195,46,8,5,24,114,184,20,194,0,76,158,9,93,2,31,32,77,49,33,39,192,0,92,29,33,13,116,112,51,140,129,203,41,88,197,49,2,31,139,24,0,131,174,79,189,38,145,72,95,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Pause = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,32,99,72,82,77,0,0,122,37,0,0,128,131,0,0,249,255,0,0,128,233,0,0,117,48,0,0,234,96,0,0,58,152,0,0,23,111,146,95,197,70,0,0,0,9,112,72,89,115,0,0,46,34,0,0,46,34,1,170,226,221,146,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,39,73,68,65,84,40,83,99,100,96,96,96,248,255,255,255,127,6,40,96,100,100,100,132,177,209,197,153,96,28,98,192,168,98,100,48,72,20,3,0,251,166,8,22,208,41,251,132,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Pencil = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,77,73,68,65,84,40,83,141,207,65,14,0,33,8,67,209,223,185,255,157,235,138,4,76,213,233,10,245,65,16,30,177,237,170,53,159,102,6,148,116,196,182,45,73,189,33,198,45,117,38,173,145,38,213,6,3,223,32,29,191,32,133,255,64,128,248,219,4,1,190,253,226,4,217,241,13,2,44,202,127,51,252,197,157,168,226,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Play = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,32,99,72,82,77,0,0,122,37,0,0,128,131,0,0,249,255,0,0,128,233,0,0,117,48,0,0,234,96,0,0,58,152,0,0,23,111,146,95,197,70,0,0,0,9,112,72,89,115,0,0,46,34,0,0,46,34,1,170,226,221,146,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,80,73,68,65,84,40,83,141,145,57,14,0,33,12,3,227,109,105,248,255,103,217,134,225,176,0,101,42,7,28,203,130,136,78,235,48,159,16,194,141,146,198,29,124,126,0,190,28,47,115,204,133,194,124,173,177,66,165,103,50,16,148,50,167,147,37,213,161,17,167,206,254,124,215,100,55,110,100,126,240,7,104,191,40,26,216,49,113,226,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_Shadow = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,12,0,0,0,12,8,6,0,0,0,86,117,92,231,0,0,0,9,112,72,89,115,0,0,46,35,0,0,46,35,1,120,165,63,118,0,0,0,126,73,68,65,84,40,145,205,142,49,10,131,64,16,69,7,91,59,27,27,143,36,8,66,188,129,55,240,18,246,222,66,48,176,125,138,92,36,157,177,241,4,130,62,11,71,34,155,69,183,244,53,51,197,155,63,95,228,54,0,79,32,240,149,19,54,114,191,11,145,66,103,233,147,30,2,31,253,48,250,124,168,69,36,214,125,185,74,175,52,121,214,105,206,106,52,42,77,252,72,109,49,209,212,47,255,188,109,185,117,72,59,47,32,114,85,121,0,6,24,128,30,232,128,236,232,172,39,108,165,70,143,30,227,157,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_ShadowDilate = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,189,0,0,14,189,1,71,251,144,173,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,65,73,68,65,84,40,83,205,142,59,14,0,32,12,66,197,251,223,25,39,12,88,99,210,205,55,245,67,41,99,124,1,206,1,73,170,6,16,251,104,92,40,252,96,230,170,186,185,65,17,191,216,46,183,8,66,223,194,25,134,207,69,43,70,139,5,108,156,16,24,120,118,6,225,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_ShadowSoftness = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,191,0,0,14,191,1,56,5,83,36,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,69,73,68,65,84,40,83,173,142,57,14,0,48,8,195,226,254,255,207,116,41,18,226,80,135,214,99,112,0,233,21,51,179,156,181,184,120,45,100,113,44,68,193,137,185,36,173,88,112,0,114,166,73,158,64,231,20,64,247,163,231,0,101,51,129,60,43,242,55,54,171,105,55,238,47,53,100,199,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_UpDown = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,193,0,0,14,193,1,184,145,107,237,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,64,73,68,65,84,40,83,165,144,65,14,0,48,8,194,234,254,255,103,118,221,24,49,49,235,77,68,73,128,128,36,185,6,80,46,156,198,170,186,246,215,144,62,250,193,31,41,1,96,185,208,49,50,143,24,181,241,212,210,245,28,73,9,0,27,36,91,27,253,99,171,254,109,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_LeftRight = RotateTexture90(icon_UpDown);
            icon_X = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,0,67,73,68,65,84,40,83,157,139,193,13,0,32,12,2,187,128,251,111,91,77,140,24,11,193,71,239,85,224,26,243,16,134,204,28,215,193,225,30,100,151,194,137,110,224,44,188,2,96,167,208,18,1,59,27,22,56,91,209,246,82,16,101,255,137,0,206,2,20,49,230,36,190,152,115,79,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_XRotate = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,195,0,0,14,195,1,199,111,168,100,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,60,73,68,65,84,40,83,181,141,65,14,0,32,8,195,58,255,255,231,121,51,74,192,104,140,189,177,21,128,87,108,59,102,0,154,135,76,146,52,156,69,38,44,204,98,73,246,1,64,85,113,76,117,160,197,96,199,63,249,138,14,255,191,23,250,188,148,56,85,0,0,0,0,73,69,78,68,174,66,96,130});
            icon_YRotate = RotateTexture90(icon_XRotate);
            icon_ZRotate = CreateTextureFromByte(new byte[] {137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,11,0,0,0,11,8,6,0,0,0,169,172,119,38,0,0,0,1,115,82,71,66,0,174,206,28,233,0,0,0,4,103,65,77,65,0,0,177,143,11,252,97,5,0,0,0,9,112,72,89,115,0,0,14,192,0,0,14,192,1,106,214,137,9,0,0,1,135,105,84,88,116,88,77,76,58,99,111,109,46,97,100,111,98,101,46,120,109,112,0,0,0,0,0,60,63,120,112,97,99,107,101,116,32,98,101,103,105,110,61,39,239,187,191,39,32,105,100,61,39,87,53,77,48,77,112,67,101,104,105,72,122,114,101,83,122,78,84,99,122,107,99,57,100,39,63,62,13,10,60,120,58,120,109,112,109,101,116,97,32,120,109,108,110,115,58,120,61,34,97,100,111,98,101,58,110,115,58,109,101,116,97,47,34,62,60,114,100,102,58,82,68,70,32,120,109,108,110,115,58,114,100,102,61,34,104,116,116,112,58,47,47,119,119,119,46,119,51,46,111,114,103,47,49,57,57,57,47,48,50,47,50,50,45,114,100,102,45,115,121,110,116,97,120,45,110,115,35,34,62,60,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,32,114,100,102,58,97,98,111,117,116,61,34,117,117,105,100,58,102,97,102,53,98,100,100,53,45,98,97,51,100,45,49,49,100,97,45,97,100,51,49,45,100,51,51,100,55,53,49,56,50,102,49,98,34,32,120,109,108,110,115,58,116,105,102,102,61,34,104,116,116,112,58,47,47,110,115,46,97,100,111,98,101,46,99,111,109,47,116,105,102,102,47,49,46,48,47,34,62,60,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,49,60,47,116,105,102,102,58,79,114,105,101,110,116,97,116,105,111,110,62,60,47,114,100,102,58,68,101,115,99,114,105,112,116,105,111,110,62,60,47,114,100,102,58,82,68,70,62,60,47,120,58,120,109,112,109,101,116,97,62,13,10,60,63,120,112,97,99,107,101,116,32,101,110,100,61,39,119,39,63,62,44,148,152,11,0,0,0,71,73,68,65,84,40,83,157,144,49,14,0,32,8,3,139,255,255,51,46,98,74,49,70,188,201,150,195,1,160,129,113,112,119,231,12,0,102,150,28,96,137,42,151,46,130,138,65,90,40,219,7,98,62,180,208,119,155,235,207,12,247,91,214,1,231,56,223,223,157,95,152,24,45,79,197,87,65,135,178,0,0,0,0,73,69,78,68,174,66,96,130});
            isImageInited = true;

        }

        public static Texture2D RotateTexture90(Texture2D tex) {
            int w = tex.width;
            int h = tex.height;

            Texture2D rotTex = new Texture2D(h, w, tex.format, false);
            Color[] original = tex.GetPixels();
            Color[] rotated = new Color[original.Length];

            for(int y = 0; y < h; y++) {
                for(int x = 0; x < w; x++) {
                    rotated[x * h + (h - y - 1)] = original[y * w + x];
                }
            }

            rotTex.SetPixels(rotated);
            rotTex.Apply();
            return rotTex;
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

        public static void DrawLabel(Texture2D icon, string label) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(icon);
            GUILayout.Space(4);
            GUILayout.Label(label);
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

        public static bool DrawBool(Texture2D icon, string label, ref bool value) {
            bool prev = value;

            GUILayout.BeginHorizontal();

            if(Main.Settings.useLegacyTheme) {
                value = GUILayout.Toggle(value, "");
            } else {
                var old = GUI.backgroundColor;
                GUI.backgroundColor = Color.clear;
                var newskin = new GUIStyle(GUI.skin.button);
                newskin.fontSize = 16;
                newskin.margin = new RectOffset(0, 0, 4, 0);
                newskin.padding = new RectOffset(0, 0, 0, 0);

                if(GUILayout.Button(value ? textureSelected : textureUnselected, newskin)) {
                    value = !value;
                }

                GUI.backgroundColor = old;
            }

            bool buttonPressed = false;
            buttonPressed |= GUILayout.Button(icon, GUI.skin.label);
            buttonPressed |= GUILayout.Button(label, GUI.skin.label);

            if(buttonPressed) {
                value = !value;
            }


            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            return prev != value;
        }

        public static bool DrawOnlyBool(ref bool value) {
            bool prev = value;

            if(Main.Settings.useLegacyTheme) {
                value = GUILayout.Toggle(value, "");
            } else {
                var old = GUI.backgroundColor;
                GUI.backgroundColor = Color.clear;
                var newskin = new GUIStyle(GUI.skin.button);
                newskin.fontSize = 16;
                newskin.margin = new RectOffset(0, 0, 4, 0);
                newskin.padding = new RectOffset(0, 0, 0, 0);

                if(GUILayout.Button(value ? textureSelected : textureUnselected, newskin)) {
                    value = !value;
                }

                GUI.backgroundColor = old;
            }

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

        public static bool DrawEnum<T>(ref T @enum) where T : Enum
        {
            int current = EnumHelper<T>.IndexOf(@enum);
            string[] names = EnumHelper<T>.GetNames();
            bool result = SelectionPopup(ref current, names, "");
            @enum = EnumHelper<T>.GetValues()[current];
            return result;
        }

        public static bool DrawEase(ref Ease ease) {
            string[] names = Enum.GetNames(typeof(Ease));
            int current = (int)ease;
            Texture2D[] easeImages = new Texture2D[] { null,ease_Linear,ease_InSine,ease_OutSine,ease_InOutSine,ease_InQuad,ease_OutQuad,ease_InOutQuad,ease_InCubic,ease_OutCubic,ease_InOutCubic,ease_InQuart,ease_OutQuart,ease_InOutQuart,ease_InQuint,ease_OutQuint,ease_InOutQuint,ease_InExpo,ease_OutExpo,ease_InOutExpo,ease_InCirc,ease_OutCirc,ease_InOutCirc,ease_InElastic,ease_OutElastic,ease_InOutElastic,ease_InBack,ease_OutBack,ease_InOutBack,ease_InBounce,ease_OutBounce,ease_InOutBounce };
            bool result = SelectionPopup(ref current, names, easeImages, "");
            if(result) {
                ease = (Ease)current;
                return true;
            }
            return false;
        }

        public static bool DrawEnum<T>(ref T @enum, Texture2D[] images) where T : Enum {
            int current = EnumHelper<T>.IndexOf(@enum);
            string[] names = EnumHelper<T>.GetNames();
            bool result = SelectionPopup(ref current, names, images, "");
            @enum = EnumHelper<T>.GetValues()[current];
            return result;
        }

        public static bool DrawEnumPlus<T>(ref T @enum, Func<string, string> translator)
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

        public static bool DrawEnumPlus<T>(ref T @enum, Texture2D[] images, Func<string, string> translator)
            where T : Enum {
            int current = EnumHelper<T>.IndexOf(@enum);
            string[] names = EnumHelper<T>.GetNames();
            string[] translatedNames = names.Select(name => translator(name)).ToArray();

            bool result =
                SelectionPopup(ref current, translatedNames, images, "");

            @enum = EnumHelper<T>.GetValues()[current];
            return result;
        }

        public static bool DrawTags(ref string value) {
            var tags = TagManager.tags.Keys.ToList();
            tags.Sort();
            var selected = tags.IndexOf(value);

            var tooltip = new Dictionary<string, string>();
            foreach(var tag in tags) {
                tooltip[tag] = Utils.Tooltip.GetTooltip(tag);
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

        public static bool DrawString(Texture2D icon, string label, ref string value, bool textArea = false) {
            string prev = value;
            GUILayout.BeginHorizontal();
            GUILayout.Label(icon);
            GUILayout.Label(label);
            if(!textArea)
                value = GUILayout.TextField(value, myTextField);
            else
                value = GUILayout.TextArea(value, myTextField);
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

        public static bool DrawCodeEditor(Texture2D icon, string label, string id, ref string value) {
            string prev = value;
            GUILayout.BeginHorizontal();
            GUILayout.Label(icon);
            GUILayout.Space(4);
            GUILayout.Label(label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

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
                Button(ali_Unknown, GUILayout.Width(404));
                GUI.color = Color.white;
                return false;
            }

            int oldvalue = (int)value;
            int newvalue = oldvalue;

            GUILayout.BeginHorizontal();

            // Left 0
            GUI.color = (((int)value & (1 << 0)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Left, GUILayout.Width(40))) {
                newvalue &= ~0xFF;         // clear 0~7
                newvalue |= (1 << 0);
            }

            // Center 1
            GUI.color = (((int)value & (1 << 1)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Center, GUILayout.Width(40))) {
                newvalue &= ~0xFF;
                newvalue |= (1 << 1);
            }

            // Right 2
            GUI.color = (((int)value & (1 << 2)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Right, GUILayout.Width(40))) {
                newvalue &= ~0xFF;
                newvalue |= (1 << 2);
            }

            // Justified 3
            GUI.color = (((int)value & (1 << 3)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Justified, GUILayout.Width(40))) {
                newvalue &= ~0xFF;
                newvalue |= (1 << 3);
            }

            // Flush 4
            GUI.color = (((int)value & (1 << 4)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Flush, GUILayout.Width(40))) {
                newvalue &= ~0xFF;
                newvalue |= (1 << 4);
            }

            // Geometry_Center 5
            GUI.color = (((int)value & (1 << 5)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Geometry_Center, GUILayout.Width(40))) {
                newvalue &= ~0xFF;
                newvalue |= (1 << 5);
            }

            GUILayout.Space(20);

            // Top 8
            GUI.color = (((int)value & (1 << 8)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Top, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);  // clear 8~15
                newvalue |= (1 << 8);
            }

            // Middle 9
            GUI.color = (((int)value & (1 << 9)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Middle, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);
                newvalue |= (1 << 9);
            }

            // Bottom 10
            GUI.color = (((int)value & (1 << 10)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Bottom, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);
                newvalue |= (1 << 10);
            }

            // Baseline 11
            GUI.color = (((int)value & (1 << 11)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Baseline, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);
                newvalue |= (1 << 11);
            }

            // Midline 12
            GUI.color = (((int)value & (1 << 12)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Midline, GUILayout.Width(40))) {
                newvalue &= ~(0xFF << 8);
                newvalue |= (1 << 12);
            }

            // Capline 13
            GUI.color = (((int)value & (1 << 13)) != 0) ? Color.cyan : Color.white;
            if(Button(ali_Capline, GUILayout.Width(40))) {
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
            if(Button(openFolder, GUILayout.Width(40))) {
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
        public static GUIStyle myTextFieldNoPad;
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
                myTextFieldNoPad.normal.background = GUI.skin.textField.normal.background;
                myTextFieldNoPad.focused.background = GUI.skin.textField.focused.background;
                myTextFieldNoPad.hover.background = GUI.skin.textField.hover.background;
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
                myTextFieldNoPad.normal.background = tfgray;
                myTextFieldNoPad.focused.background = tfgray;
                myTextFieldNoPad.hover.background = tfgray;
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
            myTextFieldNoPad = new GUIStyle(myTextField);
            myTextField.padding.right = 40;
            mySlider = new GUIStyle(GUI.skin.horizontalSlider);
            myThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
            SetStyle(Main.Settings.useLegacyTheme);
        }

        public static bool Button(string str, params GUILayoutOption[] options)
        {
            return GUILayout.Button(str, myButton, options);
        }

        public static bool Button(Texture2D icon, string text, params GUILayoutOption[] options) {
            return GUILayout.Button(new GUIContent(text, icon), myButton, options);
        }

        public static bool Button(Texture2D texture, params GUILayoutOption[] options) {
            return GUILayout.Button(texture, myButton, options);
        }

        public static void ButtonDummy(Texture2D texture, params GUILayoutOption[] options) {
            GUIStyle dummyStyle = new GUIStyle(myButton);
            dummyStyle.normal.background = myButton.normal.background;
            dummyStyle.hover.background = myButton.normal.background;
            dummyStyle.active.background = myButton.normal.background;
            dummyStyle.focused.background = myButton.normal.background;

            GUILayout.Button(texture, dummyStyle, options);
        }
    }
}