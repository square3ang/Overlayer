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

namespace Overlayer.Core;

public static class Drawer {
    public static bool DrawVector2(string label, ref Vector2 vec2, float lValue, float rValue) {
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
        changed |= DrawSingleWithSlider(Icon_LeftRight, "X", ref vec2.x, lValue, rValue, 300f);
        GUI.color = new Color(0.68f, 1.0f, 0.68f);
        changed |= DrawSingleWithSlider(Icon_UpDown, "Y", ref vec2.y, lValue, rValue, 300f);
        GUI.color = old;
        return changed;
    }

    public static bool DrawVector3(string label, ref Vector3 vec3, float lValue, float rValue) {
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
        changed |= DrawSingleWithSlider(Icon_XRotate, "X", ref vec3.x, lValue, rValue, 300f);
        GUI.color = new Color(0.68f, 1.0f, 0.68f);
        changed |= DrawSingleWithSlider(Icon_YRotate, "Y", ref vec3.y, lValue, rValue, 300f);
        GUI.color = new Color(0.68f, 0.68f, 1.0f);
        changed |= DrawSingleWithSlider(Icon_ZRotate, "Z", ref vec3.z, lValue, rValue, 300f);

        GUI.color = old;
        return changed;
    }

    public static bool SelectionPopup(ref int selected, string[] options, string label,
        params GUILayoutOption[] layoutOptions) {
        if(label != "") {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
        }

        var news = RGUI.SelectionPopup(selected, options, null, layoutOptions);
        var c = selected != news;

        selected = news;
        if(label != "") {
            GUILayout.EndHorizontal();
        }

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
        if(label != "") {
            GUILayout.EndHorizontal();
        }

        return c;
    }

    public static bool SelectionPopupWithTooltip(ref int selected, string[] options, string label,
        Dictionary<string, string> tooltips, params GUILayoutOption[] layoutOptions) {
        if(label != "") {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
        }

        var news = RGUI.SelectionPopup(selected, options, tooltips, layoutOptions);
        var c = selected != news;

        selected = news;
        if(label != "") {
            GUILayout.EndHorizontal();
        }

        return c;
    }

    public static bool DrawColor(ref Color color) {
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
        if(DrawString("Hex:", ref hex)) {
            c = true;
            ColorUtility.TryParseHtmlString("#" + hex, out color);
        }

        var ncol = RGUI.Field(color, "");

        if(!c) {
            c = color != ncol;
        }

        color = ncol;

        return c;
    }

    public static bool DrawSingleWithSlider(string label, ref float value, float lValue, float rValue, float width) {
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
        Action<int> elementRightGUI = null, Action<int, string> onElementChange = null) {
        bool result = false;
        GUILayout.BeginHorizontal();
        if(Drawer.Button("+")) {
            Array.Resize(ref array, array.Length + 1);
            arrayResized?.Invoke(array.Length);
            result = true;
        }

        if(array.Length > 0 && Drawer.Button("-")) {
            Array.Resize(ref array, array.Length - 1);
            arrayResized?.Invoke(array.Length);
            result = true;
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        for(int i = 0; i < array.Length; i++) {
            string cache = array[i];
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{i}: ");
            cache = GUILayout.TextField(cache, myTextField);
            elementRightGUI?.Invoke(i);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if(cache != array[i]) {
                array[i] = cache;
                onElementChange?.Invoke(i, cache);
                result = true;
            }
        }

        return result;
    }

    public static bool DrawArray(string label, ref object[] array) {
        bool result = false;
        GUILayout.Label(label);
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if(Drawer.Button("+")) {
            Array.Resize(ref array, array.Length + 1);
        }

        if(array.Length > 0 && Drawer.Button("-")) {
            Array.Resize(ref array, array.Length - 1);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        for(int i = 0; i < array.Length; i++) {
            result |= DrawObject($"{i}: ", ref array[i]);
        }

        GUILayout.EndVertical();
        return result;
    }

    public static bool DrawArray(ref string[] array) {
        bool result = false;
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if(Drawer.Button("+")) {
            Array.Resize(ref array, array.Length + 1);
        }

        if(array.Length > 0 && Drawer.Button("-")) {
            Array.Resize(ref array, array.Length - 1);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        for(int i = 0; i < array.Length; i++) {
            result |= DrawString($"{i}: ", ref array[i]);
        }

        GUILayout.EndVertical();
        return result;
    }

    private static bool isImageInited = false;

    public static Texture2D Icon_Copy;
    public static Texture2D Icon_Active;
    public static Texture2D Icon_Inactive;
    public static Texture2D Icon_Gradation;
    public static Texture2D Icon_UpDown;
    public static Texture2D Icon_LeftRight;
    public static Texture2D Icon_XRotate;
    public static Texture2D Icon_YRotate;
    public static Texture2D Icon_ZRotate;
    public static Texture2D Icon_OpenFolder;
    public static Texture2D Icon_Color;
    public static Texture2D Icon_Drag;
    public static Texture2D Icon_Discord;
    public static Texture2D Icon_Font;
    public static Texture2D Icon_FontSize;
    public static Texture2D Icon_FontAlternate;
    public static Texture2D Icon_Github;
    public static Texture2D Icon_LineSpacing;
    public static Texture2D Icon_Outline;
    public static Texture2D Icon_OutlineWidth;
    public static Texture2D Icon_Parse;
    public static Texture2D Icon_Pause;
    public static Texture2D Icon_Pencil;
    public static Texture2D Icon_Play;
    public static Texture2D Icon_Shadow;
    public static Texture2D Icon_ShadowDilate;
    public static Texture2D Icon_ShadowSoftness;
    public static Texture2D Icon_X;
    public static Texture2D Icon_Power;
    public static Texture2D Icon_Plus;
    public static Texture2D Icon_Opacity;

    public static Texture2D Icon_Up;
    public static Texture2D Icon_Down;

    public static Texture2D Icon_AliLeft;
    public static Texture2D Icon_AliRight;
    public static Texture2D Icon_AliCenter;
    public static Texture2D Icon_AliJustified;
    public static Texture2D Icon_AliFlush;
    public static Texture2D Icon_AliGeometryCenter;
    public static Texture2D Icon_AliTop;
    public static Texture2D Icon_AliMiddle;
    public static Texture2D Icon_AliBottom;
    public static Texture2D Icon_AliBaseline;
    public static Texture2D Icon_AliMidline;
    public static Texture2D Icon_AliCapline;
    public static Texture2D Icon_AliUnknown;

    public static Texture2D Icon_EaseLinear;
    public static Texture2D Icon_EaseInSine;
    public static Texture2D Icon_EaseOutSine;
    public static Texture2D Icon_EaseInOutSine;
    public static Texture2D Icon_EaseInQuad;
    public static Texture2D Icon_EaseOutQuad;
    public static Texture2D Icon_EaseInOutQuad;
    public static Texture2D Icon_EaseInCubic;
    public static Texture2D Icon_EaseOutCubic;
    public static Texture2D Icon_EaseInOutCubic;
    public static Texture2D Icon_EaseInQuart;
    public static Texture2D Icon_EaseOutQuart;
    public static Texture2D Icon_EaseInOutQuart;
    public static Texture2D Icon_EaseInQuint;
    public static Texture2D Icon_EaseOutQuint;
    public static Texture2D Icon_EaseInOutQuint;
    public static Texture2D Icon_EaseInExpo;
    public static Texture2D Icon_EaseOutExpo;
    public static Texture2D Icon_EaseInOutExpo;
    public static Texture2D Icon_EaseInCirc;
    public static Texture2D Icon_EaseOutCirc;
    public static Texture2D Icon_EaseInOutCirc;
    public static Texture2D Icon_EaseInElastic;
    public static Texture2D Icon_EaseOutElastic;
    public static Texture2D Icon_EaseInOutElastic;
    public static Texture2D Icon_EaseInBack;
    public static Texture2D Icon_EaseOutBack;
    public static Texture2D Icon_EaseInOutBack;
    public static Texture2D Icon_EaseInBounce;
    public static Texture2D Icon_EaseOutBounce;
    public static Texture2D Icon_EaseInOutBounce;

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

        outlineimg = new Texture2D(1, 1, TextureFormat.RGBA32, false, true) {
            filterMode = FilterMode.Point
        };
        outlineimg.LoadImage(ImageManager.GetResourceBytes("RGUIoutline.png"));

        black = new Texture2D(1, 1);
        black.SetPixel(0, 0, Color.black);
        black.Apply();

        Icon_Copy = CreateTextureFromByte(ImageManager.GetResourceBytes("copy.png"));
        Icon_Active = CreateTextureFromByte(ImageManager.GetResourceBytes("active.png"));
        Icon_Inactive = CreateTextureFromByte(ImageManager.GetResourceBytes("inactive.png"));
        Icon_Gradation = CreateTextureFromByte(ImageManager.GetResourceBytes("gradation.png"));
        Icon_UpDown = CreateTextureFromByte(ImageManager.GetResourceBytes("updown.png"));
        Icon_LeftRight = RotateTexture90(Icon_UpDown);
        Icon_XRotate = CreateTextureFromByte(ImageManager.GetResourceBytes("xrotate.png"));
        Icon_YRotate = RotateTexture90(Icon_XRotate);
        Icon_ZRotate = CreateTextureFromByte(ImageManager.GetResourceBytes("zrotate.png"));
        Icon_OpenFolder = CreateTextureFromByte(ImageManager.GetResourceBytes("openfolder.png"));
        Icon_Color = CreateTextureFromByte(ImageManager.GetResourceBytes("color.png"));
        Icon_Drag = CreateTextureFromByte(ImageManager.GetResourceBytes("drag.png"));
        Icon_Discord = CreateTextureFromByte(ImageManager.GetResourceBytes("discord.png"));
        Icon_Font = CreateTextureFromByte(ImageManager.GetResourceBytes("font.png"));
        Icon_FontSize = CreateTextureFromByte(ImageManager.GetResourceBytes("fontsize.png"));
        Icon_FontAlternate = CreateTextureFromByte(ImageManager.GetResourceBytes("fontalternate.png"));
        Icon_Github = CreateTextureFromByte(ImageManager.GetResourceBytes("github.png"));
        Icon_LineSpacing = CreateTextureFromByte(ImageManager.GetResourceBytes("linespacing.png"));
        Icon_Outline = CreateTextureFromByte(ImageManager.GetResourceBytes("outline.png"));
        Icon_OutlineWidth = CreateTextureFromByte(ImageManager.GetResourceBytes("outlinewidth.png"));
        Icon_Parse = CreateTextureFromByte(ImageManager.GetResourceBytes("parse.png"));
        Icon_Pause = CreateTextureFromByte(ImageManager.GetResourceBytes("pause.png"));
        Icon_Pencil = CreateTextureFromByte(ImageManager.GetResourceBytes("pencil.png"));
        Icon_Play = CreateTextureFromByte(ImageManager.GetResourceBytes("play.png"));
        Icon_Shadow = CreateTextureFromByte(ImageManager.GetResourceBytes("shadow.png"));
        Icon_ShadowDilate = CreateTextureFromByte(ImageManager.GetResourceBytes("shadowdilate.png"));
        Icon_ShadowSoftness = CreateTextureFromByte(ImageManager.GetResourceBytes("shadowsoftness.png"));
        Icon_X = CreateTextureFromByte(ImageManager.GetResourceBytes("x.png"));
        Icon_Power = CreateTextureFromByte(ImageManager.GetResourceBytes("power.png"));
        Icon_Plus = CreateTextureFromByte(ImageManager.GetResourceBytes("plus.png"));
        Icon_Opacity = CreateTextureFromByte(ImageManager.GetResourceBytes("opacity.png"));

        Icon_Up = CreateTextureFromByte(ImageManager.GetResourceBytes("up.png"));
        Icon_Down = RotateTexture90(RotateTexture90(Icon_Up));

        Icon_AliLeft = CreateTextureFromByte(ImageManager.GetResourceBytes("alileft.png"));
        Icon_AliRight = CreateTextureFromByte(ImageManager.GetResourceBytes("aliright.png"));
        Icon_AliCenter = CreateTextureFromByte(ImageManager.GetResourceBytes("alicenter.png"));
        Icon_AliJustified = CreateTextureFromByte(ImageManager.GetResourceBytes("alijustified.png"));
        Icon_AliFlush = CreateTextureFromByte(ImageManager.GetResourceBytes("aliflush.png"));
        Icon_AliGeometryCenter = CreateTextureFromByte(ImageManager.GetResourceBytes("aligeometrycenter.png"));
        Icon_AliTop = CreateTextureFromByte(ImageManager.GetResourceBytes("alitop.png"));
        Icon_AliMiddle = CreateTextureFromByte(ImageManager.GetResourceBytes("alimiddle.png"));
        Icon_AliBottom = CreateTextureFromByte(ImageManager.GetResourceBytes("alibottom.png"));
        Icon_AliBaseline = CreateTextureFromByte(ImageManager.GetResourceBytes("alibaseline.png"));
        Icon_AliMidline = CreateTextureFromByte(ImageManager.GetResourceBytes("alimidline.png"));
        Icon_AliCapline = CreateTextureFromByte(ImageManager.GetResourceBytes("alicapline.png"));
        Icon_AliUnknown = CreateTextureFromByte(ImageManager.GetResourceBytes("aliunknown.png"));

        Icon_EaseLinear = CreateTextureFromByte(ImageManager.GetResourceBytes("easelinear.png"));
        Icon_EaseInSine = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinsine.png"));
        Icon_EaseOutSine = CreateTextureFromByte(ImageManager.GetResourceBytes("easeoutsine.png"));
        Icon_EaseInOutSine = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinoutsine.png"));
        Icon_EaseInQuad = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinquad.png"));
        Icon_EaseOutQuad = CreateTextureFromByte(ImageManager.GetResourceBytes("easeoutquad.png"));
        Icon_EaseInOutQuad = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinoutquad.png"));
        Icon_EaseInCubic = CreateTextureFromByte(ImageManager.GetResourceBytes("easeincubic.png"));
        Icon_EaseOutCubic = CreateTextureFromByte(ImageManager.GetResourceBytes("easeoutcubic.png"));
        Icon_EaseInOutCubic = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinoutcubic.png"));
        Icon_EaseInQuart = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinquart.png"));
        Icon_EaseOutQuart = CreateTextureFromByte(ImageManager.GetResourceBytes("easeoutquart.png"));
        Icon_EaseInOutQuart = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinoutquart.png"));
        Icon_EaseInQuint = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinquint.png"));
        Icon_EaseOutQuint = CreateTextureFromByte(ImageManager.GetResourceBytes("easeoutquint.png"));
        Icon_EaseInOutQuint = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinoutquint.png"));
        Icon_EaseInExpo = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinexpo.png"));
        Icon_EaseOutExpo = CreateTextureFromByte(ImageManager.GetResourceBytes("easeoutexpo.png"));
        Icon_EaseInOutExpo = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinoutexpo.png"));
        Icon_EaseInCirc = CreateTextureFromByte(ImageManager.GetResourceBytes("easeincirc.png"));
        Icon_EaseOutCirc = CreateTextureFromByte(ImageManager.GetResourceBytes("easeoutcirc.png"));
        Icon_EaseInOutCirc = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinoutcirc.png"));
        Icon_EaseInElastic = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinelastic.png"));
        Icon_EaseOutElastic = CreateTextureFromByte(ImageManager.GetResourceBytes("easeoutelastic.png"));
        Icon_EaseInOutElastic = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinoutelastic.png"));
        Icon_EaseInBack = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinback.png"));
        Icon_EaseOutBack = CreateTextureFromByte(ImageManager.GetResourceBytes("easeoutback.png"));
        Icon_EaseInOutBack = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinoutback.png"));
        Icon_EaseInBounce = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinbounce.png"));
        Icon_EaseOutBounce = CreateTextureFromByte(ImageManager.GetResourceBytes("easeoutbounce.png"));
        Icon_EaseInOutBounce = CreateTextureFromByte(ImageManager.GetResourceBytes("easeinoutbounce.png"));

        isImageInited = true;

    }

    public static Texture2D RotateTexture90(Texture2D tex) {
        int w = tex.width;
        int h = tex.height;

        Texture2D rotTex = new(h, w, tex.format, false);
        Color[] original = tex.GetPixels();
        Color[] rotated = new Color[original.Length];

        for(int y = 0; y < h; y++) {
            for(int x = 0; x < w; x++) {
                rotated[(x * h) + (h - y - 1)] = original[(y * w) + x];
            }
        }

        rotTex.SetPixels(rotated);
        rotTex.Apply();
        return rotTex;
    }

    public static Texture2D Base64ToTexture(string base64) {
        byte[] imageBytes = System.Convert.FromBase64String(base64);

        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
        texture.LoadImage(imageBytes);
        return texture;
    }
    public static Texture2D CreateTextureFromByte(byte[] bytes) {
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
        texture.LoadImage(bytes);
        return texture;
    }

    public static void DrawLabel(Texture2D icon, string label) {
        GUILayout.BeginHorizontal();
        GUILayout.Label(icon);
        GUILayout.Space(4);
        GUILayout.Label(label);
    }

    public static bool DrawBool(string label, ref bool value) {
        bool prev = value;

        GUILayout.BeginHorizontal();

        if(Main.Settings.useLegacyTheme) {
            value = GUILayout.Toggle(value, "");
        } else {
            var old = GUI.backgroundColor;
            GUI.backgroundColor = Color.clear;
            var newskin = new GUIStyle(GUI.skin.button) {
                fontSize = 16,
                margin = new RectOffset(0, 0, 4, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };

            if(GUILayout.Button(value ? Icon_Active : Icon_Inactive, newskin)) {
                value = !value;
            }

            GUI.backgroundColor = old;
        }

        if(GUILayout.Button(label, GUI.skin.label)) {
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
            var newskin = new GUIStyle(GUI.skin.button) {
                fontSize = 16,
                margin = new RectOffset(0, 0, 4, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };

            if(GUILayout.Button(value ? Icon_Active : Icon_Inactive, newskin)) {
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
            var newskin = new GUIStyle(GUI.skin.button) {
                fontSize = 16,
                margin = new RectOffset(0, 0, 4, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };

            if(GUILayout.Button(value ? Icon_Active : Icon_Inactive, newskin)) {
                value = !value;
            }

            GUI.backgroundColor = old;
        }

        return prev != value;
    }

    public static bool DrawByte(string label, ref byte value) {
        string str = value.ToString();
        bool result = DrawString(label, ref str);
        value = StringConverter.ToUInt8(str);
        return result;
    }

    public static bool DrawDouble(string label, ref double value) {
        string str = value.ToString();
        bool result = DrawString(label, ref str);
        value = StringConverter.ToDouble(str);
        return result;
    }

    public static bool DrawEnum<T>(ref T @enum) where T : Enum {
        int current = EnumHelper<T>.IndexOf(@enum);
        string[] names = EnumHelper<T>.GetNames();
        bool result = SelectionPopup(ref current, names, "");
        @enum = EnumHelper<T>.GetValues()[current];
        return result;
    }

    public static bool DrawEase(ref Ease ease) {
        string[] names = Enum.GetNames(typeof(Ease));
        int current = (int)ease;
        Texture2D[] easeImages = new Texture2D[] { null, Icon_EaseLinear, Icon_EaseInSine, Icon_EaseOutSine, Icon_EaseInOutSine, Icon_EaseInQuad, Icon_EaseOutQuad, Icon_EaseInOutQuad, Icon_EaseInCubic, Icon_EaseOutCubic, Icon_EaseInOutCubic, Icon_EaseInQuart, Icon_EaseOutQuart, Icon_EaseInOutQuart, Icon_EaseInQuint, Icon_EaseOutQuint, Icon_EaseInOutQuint, Icon_EaseInExpo, Icon_EaseOutExpo, Icon_EaseInOutExpo, Icon_EaseInCirc, Icon_EaseOutCirc, Icon_EaseInOutCirc, Icon_EaseInElastic, Icon_EaseOutElastic, Icon_EaseInOutElastic, Icon_EaseInBack, Icon_EaseOutBack, Icon_EaseInOutBack, Icon_EaseInBounce, Icon_EaseOutBounce, Icon_EaseInOutBounce };
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
        where T : Enum {
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

    public static bool DrawInt16(string label, ref short value) {
        string str = value.ToString();
        bool result = DrawString(label, ref str);
        value = StringConverter.ToInt16(str);
        return result;
    }

    public static bool DrawInt32(string label, ref int value) {
        string str = value.ToString();
        bool result = DrawString(label, ref str);
        value = StringConverter.ToInt32(str);
        return result;
    }

    public static bool DrawInt64(string label, ref long value) {
        string str = value.ToString();
        bool result = DrawString(label, ref str);
        value = StringConverter.ToInt64(str);
        return result;
    }

    public static void DrawObject(string label, object value) {
        if(value == null) {
            return;
        }

        if(value is IDrawable drawable) {
            drawable.Draw();
            return;
        }

        Type t = value.GetType();
        if(!t.IsPrimitive && t != typeof(string)) {
            return;
        }

        var fields = t.GetFields();
        foreach(var field in fields) {
            var fValue = field.GetValue(value);
            if(DrawObject(field.Name, ref fValue)) {
                field.SetValue(value, fValue);
            }
        }

        var props = t.GetProperties();
        foreach(var prop in props.Where(p => p.CanRead && p.CanWrite)) {
            var pValue = prop.GetValue(value);
            if(DrawObject(prop.Name, ref pValue)) {
                prop.SetValue(value, pValue);
            }
        }
    }

    public static bool DrawObject(string label, ref object obj) {
        bool result = false;
        switch(obj) {
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

    public static bool DrawSByte(string label, ref sbyte value) {
        string str = value.ToString();
        bool result = DrawString(label, ref str);
        value = StringConverter.ToInt8(str);
        return result;
    }

    public static bool DrawSingle(string label, ref float value) {
        string str = value.ToString();
        bool result = DrawString(label, ref str);
        value = StringConverter.ToFloat(str);
        return result;
    }

    public static bool DrawString(string label, ref string value, bool textArea = false) => DrawString(null, label, ref value, textArea);
    public static bool DrawString(Texture2D icon, string label, ref string value, bool textArea = false) {
        string prev = value;
        GUILayout.BeginHorizontal();
        if(icon != null) {
            GUILayout.Label(icon);
        }
        GUILayout.Label(label);
        value = !textArea ? GUILayout.TextField(value, myTextField) : GUILayout.TextArea(value, myTextField);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return prev != value;
    }

    public static bool DrawOnlyString(ref string value, bool textArea = false) {
        string prev = value;
        value = !textArea ? GUILayout.TextField(value, myTextField) : GUILayout.TextArea(value, myTextField);

        GUILayout.FlexibleSpace();
        return prev != value;
    }

    public static bool DrawCodeEditor(string label, string id, ref string value) {
        string prev = value;
        GUILayout.Label(label);
        var sk = new GUIStyle(GUI.skin.label) {
            margin = new RectOffset(0, 0, 0, 0),
            wordWrap = false,
            richText = false
        };
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

        var sk = new GUIStyle(GUI.skin.label) {
            margin = new RectOffset(0, 0, 0, 0),
            wordWrap = false,
            richText = false
        };
        value = codeEditor.Draw(value, sk, id);
        return prev != value;
    }

    public static bool DrawToggleGroup(string[] labels, bool[] toggleGroup) {
        bool result = false;
        for(int i = 0; i < labels.Length; i++) {
            if(DrawBool(labels[i], ref toggleGroup[i])) {
                result = true;
                for(int j = 0; j < toggleGroup.Length; j++) {
                    if(j == i) {
                        continue;
                    } else {
                        toggleGroup[j] = false;
                    }
                }

                break;
            }
        }

        return result;
    }

    public static bool DrawUInt16(string label, ref ushort value) {
        string str = value.ToString();
        bool result = DrawString(label, ref str);
        value = StringConverter.ToUInt16(str);
        return result;
    }

    public static bool DrawUInt32(string label, ref uint value) {
        string str = value.ToString();
        bool result = DrawString(label, ref str);
        value = StringConverter.ToUInt32(str);
        return result;
    }

    public static bool DrawUInt64(string label, ref ulong value) {
        string str = value.ToString();
        bool result = DrawString(label, ref str);
        value = StringConverter.ToUInt64(str);
        return result;
    }

    public static bool DrawAlignment(ref TextAlignmentOptions value) {
        if(value == TextAlignmentOptions.Converted) {
            GUI.color = Color.cyan;
            Button(Icon_AliUnknown, GUILayout.Width(404));
            GUI.color = Color.white;
            return false;
        }

        int oldvalue = (int)value;
        int newvalue = oldvalue;

        GUILayout.BeginHorizontal();

        // Left 0
        GUI.color = (((int)value & (1 << 0)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliLeft, GUILayout.Width(40))) {
            newvalue &= ~0xFF;         // clear 0~7
            newvalue |= 1 << 0;
        }

        // Center 1
        GUI.color = (((int)value & (1 << 1)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliCenter, GUILayout.Width(40))) {
            newvalue &= ~0xFF;
            newvalue |= 1 << 1;
        }

        // Right 2
        GUI.color = (((int)value & (1 << 2)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliRight, GUILayout.Width(40))) {
            newvalue &= ~0xFF;
            newvalue |= 1 << 2;
        }

        // Justified 3
        GUI.color = (((int)value & (1 << 3)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliJustified, GUILayout.Width(40))) {
            newvalue &= ~0xFF;
            newvalue |= 1 << 3;
        }

        // Flush 4
        GUI.color = (((int)value & (1 << 4)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliFlush, GUILayout.Width(40))) {
            newvalue &= ~0xFF;
            newvalue |= 1 << 4;
        }

        // Geometry_Center 5
        GUI.color = (((int)value & (1 << 5)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliGeometryCenter, GUILayout.Width(40))) {
            newvalue &= ~0xFF;
            newvalue |= 1 << 5;
        }

        GUILayout.Space(20);

        // Top 8
        GUI.color = (((int)value & (1 << 8)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliTop, GUILayout.Width(40))) {
            newvalue &= ~(0xFF << 8);  // clear 8~15
            newvalue |= 1 << 8;
        }

        // Middle 9
        GUI.color = (((int)value & (1 << 9)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliMiddle, GUILayout.Width(40))) {
            newvalue &= ~(0xFF << 8);
            newvalue |= 1 << 9;
        }

        // Bottom 10
        GUI.color = (((int)value & (1 << 10)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliBottom, GUILayout.Width(40))) {
            newvalue &= ~(0xFF << 8);
            newvalue |= 1 << 10;
        }

        // Baseline 11
        GUI.color = (((int)value & (1 << 11)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliBaseline, GUILayout.Width(40))) {
            newvalue &= ~(0xFF << 8);
            newvalue |= 1 << 11;
        }

        // Midline 12
        GUI.color = (((int)value & (1 << 12)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliMidline, GUILayout.Width(40))) {
            newvalue &= ~(0xFF << 8);
            newvalue |= 1 << 12;
        }

        // Capline 13
        GUI.color = (((int)value & (1 << 13)) != 0) ? Color.cyan : Color.white;
        if(Button(Icon_AliCapline, GUILayout.Width(40))) {
            newvalue &= ~(0xFF << 8);
            newvalue |= 1 << 13;
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
        if(Button(Icon_OpenFolder, GUILayout.Width(40))) {
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

                if(path.StartsWith(Main.Mod.Path)) {
                    path = path.Replace(Main.Mod.Path, "{ModDir}")
                               .Replace("\\", "/");
                }

                fontPath = path;
                result = true;
            }
        }

        result |= DrawOnlyString(ref fontPath);
        GUILayout.EndHorizontal();
        return result;
    }

    public static void Tooltip(string text, bool ignoreWidth = false) {
        if(string.IsNullOrEmpty(text)) {
            GUI.Box(new Rect(0, 0, 0, 0), "");
        } else {
            Vector2 mousePosition = Event.current.mousePosition;

            Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
            Rect labelPosition = new(mousePosition.x, mousePosition.y - 40, 0, 0);

            if(!ignoreWidth) {
                var windowwidth = ((Rect)AccessTools.Field(typeof(UnityModManager.UI), "mWindowRect")
                        .GetValue(UnityModManager.UI.Instance))
                    .width;
                var scroll = (Vector2[])AccessTools.Field(typeof(UnityModManager.UI), "mScrollPosition")
                    .GetValue(UnityModManager.UI.Instance);
                windowwidth += scroll[UnityModManager.UI.Instance.tabId].x;
                if(labelPosition.x + textSize.x + 20 + 20 > windowwidth) {
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

    public static CodeEditor.CodeEditor codeEditor = new("OverlayerCodeEditor",
        new CodeTheme() {
            background = "#333333",
            linenumbg = "#222222",
            color = "#FFFFFF",
            selection = "#264F78",
            cursor = "#D4D4D4"
        });

    public static Regex highlight = new("{(.*?)}", RegexOptions.Compiled);
    public static Regex color = new("<<b></b>color=(.*?)>", RegexOptions.Compiled);
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

    static Drawer() {
        codeEditor.highlighter = str => {
            str = str.Replace("<", "<<b></b>");

            var colorHighlighted = new List<string>();
            foreach(Match m in color.Matches(str)) {
                //Main.Logger.Log(m.Groups[1].Value);
                if(!colorHighlighted.Contains(m.Groups[1].Value) && ColorUtility.TryParseHtmlString(m.Groups[1].Value, out _)) {
                    str = str.Replace("<<b></b>color=" + m.Groups[1].Value + ">",
                        "<<b></b>color=<color=" + m.Groups[1].Value + ">" + m.Groups[1].Value + "</color>>");
                    colorHighlighted.Add(m.Groups[1].Value);
                }
            }

            var highlighted = new List<string>();

            foreach(Match match in highlight.Matches(str)) {
                if(highlighted.Contains(match.Groups[1].Value)) {
                    continue;
                }

                var name = match.Groups[1].Value.Split('(')[0].Split(':')[0];
                if(TagManager.tags.ContainsKey(name)) {
                    if((name == "MovingMan" && Main.Settings.useMovingManEditor) || (name == "ColorRange" && Main.Settings.useColorRangeEditor) || (name == "EasedValue" && Main.Settings.useEasedValueEditor)) {
                        str = str.Replace("{" + match.Groups[1].Value + "}",
                            "<color=orange>{" + match.Groups[1].Value + "}</color>");
                    } else if(name.EndsWith("Hex")) {
                        try {
                            var val = (string)TagManager.tags[name].Tag.Getter.Invoke(null,
                                new object[] { "-1", Overlayer.Utils.Extensions.DefaultTrimStr });
                            str = str.Replace("{" + match.Groups[1].Value + "}",
                                "<color=#" + val + ">{" + match.Groups[1].Value + "}</color>");
                        } catch {
                            str = str.Replace("{" + match.Groups[1].Value + "}",
                                "<color=lightblue>{" + match.Groups[1].Value + "}</color>");
                        }
                    } else {
                        str = str.Replace("{" + match.Groups[1].Value + "}",
                            "<color=lightblue>{" + match.Groups[1].Value + "}</color>");
                    }
                } else {
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

    public static bool Button(string str, params GUILayoutOption[] options) => GUILayout.Button(str, myButton, options);

    public static bool Button(Texture2D icon, string text, params GUILayoutOption[] options) => GUILayout.Button(new GUIContent(text, icon), myButton, options);

    public static bool Button(Texture2D texture, params GUILayoutOption[] options) => GUILayout.Button(texture, myButton, options);

    public static void ButtonDummy(Texture2D texture, params GUILayoutOption[] options) {
        GUIStyle dummyStyle = new(myButton);
        dummyStyle.normal.background = myButton.normal.background;
        dummyStyle.hover.background = myButton.normal.background;
        dummyStyle.active.background = myButton.normal.background;
        dummyStyle.focused.background = myButton.normal.background;

        GUILayout.Button(texture, dummyStyle, options);
    }
}