using DG.Tweening;
using HarmonyLib;
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
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace Overlayer.Core;

public static class Drawer {
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
        CodeEditor.CodeEditor.Initialize();
        InitializeImages();

        myButton = new GUIStyle(GUI.skin.button);
        myTextField = new GUIStyle(GUI.skin.textField);
        myTextFieldNoPad = new GUIStyle(myTextField);
        myTextField.padding.right = 40;
        mySlider = new GUIStyle(GUI.skin.horizontalSlider);
        myThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
        SetStyle(Main.Settings.LegacyTheme);
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
    public static Texture2D Icon_Image;
    public static Texture2D Icon_Wiki;

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
        outlineimg.LoadImage(ResourceManager.GetImageBytes("RGUIoutline.png"));

        black = new Texture2D(1, 1);
        black.SetPixel(0, 0, Color.black);
        black.Apply();

        Icon_Copy = CreateTextureFromByte(ResourceManager.GetImageBytes("copy.png"));
        Icon_Active = CreateTextureFromByte(ResourceManager.GetImageBytes("active.png"));
        Icon_Inactive = CreateTextureFromByte(ResourceManager.GetImageBytes("inactive.png"));
        Icon_Gradation = CreateTextureFromByte(ResourceManager.GetImageBytes("gradation.png"));
        Icon_UpDown = CreateTextureFromByte(ResourceManager.GetImageBytes("updown.png"));
        Icon_LeftRight = RotateTexture90(Icon_UpDown);
        Icon_XRotate = CreateTextureFromByte(ResourceManager.GetImageBytes("xrotate.png"));
        Icon_YRotate = RotateTexture90(Icon_XRotate);
        Icon_ZRotate = CreateTextureFromByte(ResourceManager.GetImageBytes("zrotate.png"));
        Icon_OpenFolder = CreateTextureFromByte(ResourceManager.GetImageBytes("openfolder.png"));
        Icon_Color = CreateTextureFromByte(ResourceManager.GetImageBytes("color.png"));
        Icon_Drag = CreateTextureFromByte(ResourceManager.GetImageBytes("drag.png"));
        Icon_Discord = CreateTextureFromByte(ResourceManager.GetImageBytes("discord.png"));
        Icon_Font = CreateTextureFromByte(ResourceManager.GetImageBytes("font.png"));
        Icon_FontSize = CreateTextureFromByte(ResourceManager.GetImageBytes("fontsize.png"));
        Icon_FontAlternate = CreateTextureFromByte(ResourceManager.GetImageBytes("fontalternate.png"));
        Icon_Github = CreateTextureFromByte(ResourceManager.GetImageBytes("github.png"));
        Icon_LineSpacing = CreateTextureFromByte(ResourceManager.GetImageBytes("linespacing.png"));
        Icon_Outline = CreateTextureFromByte(ResourceManager.GetImageBytes("outline.png"));
        Icon_OutlineWidth = CreateTextureFromByte(ResourceManager.GetImageBytes("outlinewidth.png"));
        Icon_Parse = CreateTextureFromByte(ResourceManager.GetImageBytes("parse.png"));
        Icon_Pause = CreateTextureFromByte(ResourceManager.GetImageBytes("pause.png"));
        Icon_Pencil = CreateTextureFromByte(ResourceManager.GetImageBytes("pencil.png"));
        Icon_Play = CreateTextureFromByte(ResourceManager.GetImageBytes("play.png"));
        Icon_Shadow = CreateTextureFromByte(ResourceManager.GetImageBytes("shadow.png"));
        Icon_ShadowDilate = CreateTextureFromByte(ResourceManager.GetImageBytes("shadowdilate.png"));
        Icon_ShadowSoftness = CreateTextureFromByte(ResourceManager.GetImageBytes("shadowsoftness.png"));
        Icon_X = CreateTextureFromByte(ResourceManager.GetImageBytes("x.png"));
        Icon_Power = CreateTextureFromByte(ResourceManager.GetImageBytes("power.png"));
        Icon_Plus = CreateTextureFromByte(ResourceManager.GetImageBytes("plus.png"));
        Icon_Opacity = CreateTextureFromByte(ResourceManager.GetImageBytes("opacity.png"));
        Icon_Image = CreateTextureFromByte(ResourceManager.GetImageBytes("image.png"));
        Icon_Wiki = CreateTextureFromByte(ResourceManager.GetImageBytes("wiki.png"));

        Icon_Up = CreateTextureFromByte(ResourceManager.GetImageBytes("up.png"));
        Icon_Down = RotateTexture90(RotateTexture90(Icon_Up));

        Icon_AliLeft = CreateTextureFromByte(ResourceManager.GetImageBytes("alileft.png"));
        Icon_AliRight = CreateTextureFromByte(ResourceManager.GetImageBytes("aliright.png"));
        Icon_AliCenter = CreateTextureFromByte(ResourceManager.GetImageBytes("alicenter.png"));
        Icon_AliJustified = CreateTextureFromByte(ResourceManager.GetImageBytes("alijustified.png"));
        Icon_AliFlush = CreateTextureFromByte(ResourceManager.GetImageBytes("aliflush.png"));
        Icon_AliGeometryCenter = CreateTextureFromByte(ResourceManager.GetImageBytes("aligeometrycenter.png"));
        Icon_AliTop = CreateTextureFromByte(ResourceManager.GetImageBytes("alitop.png"));
        Icon_AliMiddle = CreateTextureFromByte(ResourceManager.GetImageBytes("alimiddle.png"));
        Icon_AliBottom = CreateTextureFromByte(ResourceManager.GetImageBytes("alibottom.png"));
        Icon_AliBaseline = CreateTextureFromByte(ResourceManager.GetImageBytes("alibaseline.png"));
        Icon_AliMidline = CreateTextureFromByte(ResourceManager.GetImageBytes("alimidline.png"));
        Icon_AliCapline = CreateTextureFromByte(ResourceManager.GetImageBytes("alicapline.png"));
        Icon_AliUnknown = CreateTextureFromByte(ResourceManager.GetImageBytes("aliunknown.png"));

        Icon_EaseLinear = CreateTextureFromByte(ResourceManager.GetImageBytes("easelinear.png"));
        Icon_EaseInSine = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinsine.png"));
        Icon_EaseOutSine = CreateTextureFromByte(ResourceManager.GetImageBytes("easeoutsine.png"));
        Icon_EaseInOutSine = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinoutsine.png"));
        Icon_EaseInQuad = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinquad.png"));
        Icon_EaseOutQuad = CreateTextureFromByte(ResourceManager.GetImageBytes("easeoutquad.png"));
        Icon_EaseInOutQuad = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinoutquad.png"));
        Icon_EaseInCubic = CreateTextureFromByte(ResourceManager.GetImageBytes("easeincubic.png"));
        Icon_EaseOutCubic = CreateTextureFromByte(ResourceManager.GetImageBytes("easeoutcubic.png"));
        Icon_EaseInOutCubic = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinoutcubic.png"));
        Icon_EaseInQuart = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinquart.png"));
        Icon_EaseOutQuart = CreateTextureFromByte(ResourceManager.GetImageBytes("easeoutquart.png"));
        Icon_EaseInOutQuart = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinoutquart.png"));
        Icon_EaseInQuint = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinquint.png"));
        Icon_EaseOutQuint = CreateTextureFromByte(ResourceManager.GetImageBytes("easeoutquint.png"));
        Icon_EaseInOutQuint = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinoutquint.png"));
        Icon_EaseInExpo = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinexpo.png"));
        Icon_EaseOutExpo = CreateTextureFromByte(ResourceManager.GetImageBytes("easeoutexpo.png"));
        Icon_EaseInOutExpo = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinoutexpo.png"));
        Icon_EaseInCirc = CreateTextureFromByte(ResourceManager.GetImageBytes("easeincirc.png"));
        Icon_EaseOutCirc = CreateTextureFromByte(ResourceManager.GetImageBytes("easeoutcirc.png"));
        Icon_EaseInOutCirc = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinoutcirc.png"));
        Icon_EaseInElastic = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinelastic.png"));
        Icon_EaseOutElastic = CreateTextureFromByte(ResourceManager.GetImageBytes("easeoutelastic.png"));
        Icon_EaseInOutElastic = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinoutelastic.png"));
        Icon_EaseInBack = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinback.png"));
        Icon_EaseOutBack = CreateTextureFromByte(ResourceManager.GetImageBytes("easeoutback.png"));
        Icon_EaseInOutBack = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinoutback.png"));
        Icon_EaseInBounce = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinbounce.png"));
        Icon_EaseOutBounce = CreateTextureFromByte(ResourceManager.GetImageBytes("easeoutbounce.png"));
        Icon_EaseInOutBounce = CreateTextureFromByte(ResourceManager.GetImageBytes("easeinoutbounce.png"));

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
        byte[] imageBytes = Convert.FromBase64String(base64);

        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
        texture.LoadImage(imageBytes);
        return texture;
    }
    public static Texture2D CreateTextureFromByte(byte[] bytes) {
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
        texture.LoadImage(bytes);
        return texture;
    }

    public static void BeginTab(int tab = 1) {
        GUILayout.BeginHorizontal();
        GUILayout.Space(18f * tab);
        GUILayout.BeginVertical();
    }

    public static void EndTab() {
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
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

    public static bool DrawBool(string label, ref bool value) {
        bool prev = value;

        GUILayout.BeginHorizontal();

        if(Main.Settings.LegacyTheme) {
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

        if(Main.Settings.LegacyTheme) {
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

        if(Main.Settings.LegacyTheme) {
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
        Texture2D[] easeImages = [null, Icon_EaseLinear, Icon_EaseInSine, Icon_EaseOutSine, Icon_EaseInOutSine, Icon_EaseInQuad, Icon_EaseOutQuad, Icon_EaseInOutQuad, Icon_EaseInCubic, Icon_EaseOutCubic, Icon_EaseInOutCubic, Icon_EaseInQuart, Icon_EaseOutQuart, Icon_EaseInOutQuart, Icon_EaseInQuint, Icon_EaseOutQuint, Icon_EaseInOutQuint, Icon_EaseInExpo, Icon_EaseOutExpo, Icon_EaseInOutExpo, Icon_EaseInCirc, Icon_EaseOutCirc, Icon_EaseInOutCirc, Icon_EaseInElastic, Icon_EaseOutElastic, Icon_EaseInOutElastic, Icon_EaseInBack, Icon_EaseOutBack, Icon_EaseInOutBack, Icon_EaseInBounce, Icon_EaseOutBounce, Icon_EaseInOutBounce
        ];
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
            tooltip[tag] = Main.Lang.Get($"TAG_DESC_{tag.ToUpper()}", TagDesc.GetTagDesc(tag));
        }

        SelectionPopupWithTooltip(ref selected, tags.ToArray(), "", tooltip);
        value = tags[selected];
        return selected != tags.IndexOf(value);
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
        value = CodeEditor.CodeEditor.instance.Draw(value, sk, id);
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
        value = CodeEditor.CodeEditor.instance.Draw(value, sk, id);
        return prev != value;
    }

    public static bool DrawExpr<T>(string label, string id, ref ExprValue<T> value, Action drawNormal, Type tooltip = null)
        => DrawExprInternal(null, label, id, ref value, drawNormal, tooltip);
    public static bool DrawExpr<T>(Texture2D icon, string label, string id, ref ExprValue<T> value, Action drawNormal, Type tooltip = null)
        => DrawExprInternal(icon, label, id, ref value, drawNormal, tooltip);
    private static bool DrawExprInternal<T>(Texture2D icon, string label, string id, ref ExprValue<T> value, Action drawNormal, Type tooltip) {
        bool changed = false;
        Color old = GUI.color;
        bool isExpr = value.IsExpr;
        GUILayout.BeginHorizontal();
        if(icon) {
            GUILayout.Label(icon);
            GUILayout.Space(4);
        }
        if(Main.Settings.UiMode == Settings.EditorUIMode.Simple) {
            GUILayout.Label(label);
        } else {
            if(isExpr) {
                GUI.color = Color.cyan;
            }
            if(Button(label)) {
                if(!isExpr) {
                    value.Init();
                } else {
                    value.Dispose();
                    changed = true;
                }
            }
            if(isExpr) {
                GUI.color = old;
            }
            if(isExpr && tooltip != null && MiscUtils.IsHovering()) {
                if(tooltip == typeof(Vector2)) {
                    Main.tooltip = "X,Y | EX : 1,0.5";
                } else if(tooltip == typeof(Vector3)) {
                    Main.tooltip = "X,Y,Z | EX : 1,0.5,0.2";
                } else if(tooltip == typeof(GColor)) {
                    Main.tooltip = "R,G,B,A,R,G,B,A,R,G,B,A,R,G,B,A\n↖ ↗ ↙ ↘ | 0 - 1\n EX : 1,0.5,0,1,0,1,0,0.9,0,0,1,1,0,0.24,0,1";
                } else if(tooltip == typeof(Color)) {
                    Main.tooltip = "R,G,B,A | 0 - 1 | EX : 1,0.5,0,1";
                }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if(value.IsExpr) {
            if(DrawCodeEditor(Icon_Play, Main.Lang.Get("PLAYING_COMMAND", "Playing Command"), id + "_P", ref value.Playing)) {
                value.ApplyConfig();
            }
            if(DrawCodeEditor(Icon_Pause, Main.Lang.Get("NOT_PLAYING_COMMAND", "Not Playing Command"), id + "_n", ref value.NotPlaying)) {
                value.ApplyConfig();
            }
        } else {
            drawNormal.Invoke();
            changed = true;
        }

        return changed;
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

    public static void DrawSelectFont(System.Action<string> onFontSelected) {
        if(Button(Icon_OpenFolder, GUILayout.Width(40))) {
            Task.Run(() => {
                var extensions = new[] {
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
                    string finalPath = path;
                    Main.MainThreadDispatcher.Enqueue(() => onFontSelected?.Invoke(finalPath));
                }
            });
        }
    }

    private static Rect CalculatePosition(Vector2 mousePosition, float width, float height, bool ignoreWidth) {
        Rect labelPosition = new(mousePosition.x, mousePosition.y - height - 20, width + 20, height + 20);

        if(ignoreWidth) {
            return labelPosition;
        }

        var windowwidth = ((Rect)AccessTools.Field(typeof(UnityModManager.UI), "mWindowRect")
            .GetValue(UnityModManager.UI.Instance)).width;
        var scroll = (Vector2[])AccessTools.Field(typeof(UnityModManager.UI), "mScrollPosition")
            .GetValue(UnityModManager.UI.Instance);
        windowwidth += scroll[UnityModManager.UI.Instance.tabId].x;

        if(labelPosition.x + labelPosition.width > windowwidth) {
            labelPosition.x = windowwidth - labelPosition.width;
        }

        return labelPosition;
    }

    private static void DrawBackground(Rect rect) => GUI.Box(rect, "", RGUIStyle.darkWindow);

    public static bool Tooltip(string text, bool ignoreWidth = false) {
        if(string.IsNullOrEmpty(text)) {
            GUI.Box(new Rect(0, 0, 0, 0), "");
            return false;
        }

        Vector2 mousePosition = Event.current.mousePosition;

        float maxWidth = 660f;
        float height = GUI.skin.label.CalcHeight(new GUIContent(text), maxWidth);
        float width = Mathf.Min(GUI.skin.label.CalcSize(new GUIContent(text)).x, maxWidth);

        Rect pos = CalculatePosition(mousePosition, width, height, ignoreWidth);

        DrawBackground(pos);

        GUI.Label(new Rect(pos.x + 10, pos.y + 10, width, height), text);
        return true;
    }

    public static bool Tooltip(Texture2D image, bool ignoreWidth = false) {
        if(image == null) {
            GUI.Box(new Rect(0, 0, 0, 0), "");
            return false;
        }

        Vector2 mousePosition = Event.current.mousePosition;

        float maxWidth = 660f;
        float width = image.width;
        float height = image.height;

        if(width > maxWidth) {
            float ratio = maxWidth / width;
            width = maxWidth;
            height *= ratio;
        }

        Rect pos = CalculatePosition(mousePosition, width, height, ignoreWidth);

        DrawBackground(pos);

        GUI.DrawTexture(new Rect(pos.x + 10, pos.y + 10, width, height), image, ScaleMode.ScaleToFit);
        return true;
    }

    public static bool HoverTooltip(string tooltip) {
        bool hover = MiscUtils.IsHovering();
        if(hover && Main.Settings.Tooltip) {
            Main.tooltip = tooltip;
        }

        return hover;
    }

    public static bool HoverTooltip(Texture2D tooltip) {
        bool hover = MiscUtils.IsHovering();
        if(hover && Main.Settings.Tooltip) {
            Main.tooltipImage = tooltip;
        }

        return hover;
    }

    public static bool Button(string label, params GUILayoutOption[] options) => GUILayout.Button(label, myButton, options);
    public static bool Button(Texture2D icon, string text, params GUILayoutOption[] options) => GUILayout.Button(new GUIContent(text, icon), myButton, options);
    public static bool Button(Texture2D icon, params GUILayoutOption[] options) => GUILayout.Button(icon, myButton, options);
    public static void ButtonDummy(Texture2D icon, params GUILayoutOption[] options) {
        GUIStyle dummyStyle = new(myButton);
        dummyStyle.normal.background = myButton.normal.background;
        dummyStyle.hover.background = myButton.normal.background;
        dummyStyle.active.background = myButton.normal.background;
        dummyStyle.focused.background = myButton.normal.background;

        GUILayout.Button(icon, dummyStyle, options);
    }
}