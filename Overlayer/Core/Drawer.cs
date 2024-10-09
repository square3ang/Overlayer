using Overlayer.Models;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using Overlayer.CodeEditor;
using Overlayer.Tags;
using RapidGUI;
using UnityEngine;
using UnityModManagerNet;
using Extensions = UnityModManagerNet.Extensions;
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

        public static bool DrawVector3(string label, ref Vector3 vec3, float lValue, float rValue)
        {
            bool changed = false;
            GUILayout.Label($"<b>{label}</b>");
            changed |= DrawSingleWithSlider("X", ref vec3.x, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Y", ref vec3.y, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Z", ref vec3.z, lValue, rValue, 300f);
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

        public static void TitleButton(string label, string btnLabel, Action pressed, Action horizontal = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            if (Drawer.Button(btnLabel))
                pressed?.Invoke();
            horizontal?.Invoke();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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
                cache = GUILayout.TextField(cache, Main.Settings.useLegacyTheme ? GUI.skin.textField : myTextField);
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

        public static Texture2D textureSelected;
        public static Texture2D textureUnselected;

        public static void InitializeImages()
        {
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
            outlineimg.LoadImage(Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAEAAAABABAMAAABYR2ztAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAGUExURX9/fxoaGksz5AgAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAiSURBVEjHY2CgHAjiBaMKRhWMKhhVMKpgVMGogsGmgFIAADYjd4nBtq0YAAAAAElFTkSuQmCC"));

            black = new Texture2D(1, 1);
            black.SetPixel(0, 0, Color.black);
            black.Apply();

            string base64ImageSelected =
                "iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHxSURBVDhPvZTLSsNAFIbbVN15QRG84hXxguALiOJSitT78ygoqE8iuPENBPeiuBOx2ipWFHVnxZL6/clJTUwQF+IPX8+Zf86cTifTpP5aaYuJqlarDTBG2uk7qSJcOo7z7g9/Kdd122APnmgYkTzYhXYrjyi2QwpnCAfQBmfpdPqQeE0vhzgAWbwpxo/ENTjCSxbN5uAdniHLotgXqjGsWk2ZfNamomKyHfRzVDhudtCgH6+PmDFb9RNWf4ffYvaXmNgBacEsectQ8FxfeVi0ac2v0EznumuWL7w6TO3shNz7meRL4GpBWFgVQs7WOYzPQLusk+cJY9KKNzUmzZAX5SWJuSuCHpLWboA0pLFnoh6LeYu90O2nidJ5dijhKedBvyrSMHiarj4oVk3sCYdUNbxaReTFoOGtxRF90Ev3LthtkgrUlCwfpFbNr/whYlzPGTzA94fyoeKw5EHW1umhnEKBvHalPGFugVS7FhTlGF9CxbiAeZvWmuDabJv1JcxW0EV9gQmzg110QSd57VzJR+EVv0RsNjsqJmegDHop5MINAuHpC9as2RtM25Sn2AIr2OfQtaNzcr0cdOC6AcOwxNw4c/fk67zKjok/i6ZNsA067IjwiqC/aaOVRxTbYVisz4BeWbq0FbjRlQLl/6FU6hOuOzJxbCs2hAAAAABJRU5ErkJggg==";
            string base64ImageUnselected =
                "iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGlSURBVDhPvZRNS0JBFIb1au36FBdCEH0QqZv+QBitJSrKflFC9kuCFgX9gKB9JG0jLYuKotqlENrzes9FL9fPiF54ODPvnDnOzB0n9NcKW+yoRqMxCkmaCdcJVeDGcZya2x1Q9Xo9BofwRkGf5EEB4pbuU2CFJGYIxxCDYjgcPiWWqeUQ5yCLt0L/lZiDc7zOotg61OAdskwK/KAKw67lVGmv2ZBfDMZB21FiyuyuIidt+Y8UnTS7JQYOQNowq6/I3aGYzrVgliu8KKZWdkm755dvF7kOc4qgVUbl6aA1kORwp2ieERvyBhG5dcIJJKgxK69ZEM1YLFkcWBQtgXa1oL5X0NumfnEotR1RM3oFHywuWRxG8xTVMd26XUR/hIN9+eVHuYJ72hGzXWHug7RlVl+R612bvFktYU6DLuoHpM3uKuoswye5z8QJs/1iMANV0KOwCYHt42mbOSv2Bas21FRggiUccRN0t65p63HQgesGLMI2YynGnmjv8ZRdEHuLouOQBx22T3gV0N90zNJ96vlFmR8BPVm6tN9wx+rKoPZ/KBT6AekJcNd60oGuAAAAAElFTkSuQmCC";

            byte[] imageBytesSelected = System.Convert.FromBase64String(base64ImageSelected);
            textureSelected = new Texture2D(1, 1);
            textureSelected.LoadImage(imageBytesSelected);

            byte[] imageBytesUnselected = System.Convert.FromBase64String(base64ImageUnselected);
            textureUnselected = new Texture2D(1, 1);
            textureUnselected.LoadImage(imageBytesUnselected);
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
                value = GUILayout.TextField(value, Main.Settings.useLegacyTheme ? GUI.skin.textField : myTextField);
            else value = GUILayout.TextArea(value, Main.Settings.useLegacyTheme ? GUI.skin.textField : myTextField);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return prev != value;
        }

        public static bool DrawCodeEditor(string label, ref string value)
        {
            string prev = value;
            GUILayout.Label(label);
            var sk = new GUIStyle(GUI.skin.label);

            sk.margin = new RectOffset(0, 0, 0, 0);
            sk.wordWrap = false;
            sk.richText = false;
            value = codeEditor.Draw(value, sk);
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
                foreach (Match m in color.Matches(str))
                {
                    //Main.Logger.Log(m.Groups[1].Value);
                    if (ColorUtility.TryParseHtmlString(m.Groups[1].Value, out _))
                    {
                        str = str.Replace(m.Groups[1].Value,
                            "<color=" + m.Groups[1].Value + ">" + m.Groups[1].Value + "</color>");
                    }
                }

                foreach (Match match in highlight.Matches(str))
                {
                    var name = match.Groups[1].Value.Split('(')[0].Split(':')[0];
                    if (TagManager.tags.ContainsKey(name))
                    {
                        if (name == "MovingMan" && Main.Settings.useMovingManEditor)
                        {
                            str = str.Replace("{" + match.Groups[1].Value + "}",
                                "<color=lime>{" + match.Groups[1].Value + "}</color>");
                        }
                        else if (name == "ColorRange" && Main.Settings.useColorRangeEditor)
                        {
                            str = str.Replace("{" + match.Groups[1].Value + "}",
                                "<color=lime>{" + match.Groups[1].Value + "}</color>");
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
                }

                return str;
            };

            InitializeImages();


            myButton = new GUIStyle(GUI.skin.button);
            myButton.normal.background = gray;
            myButton.active.background = dulgray;
            myButton.hover.background = dulgray;

            myTextField = new GUIStyle(GUI.skin.textField);
            myTextField.normal.background = tfgray;
            myTextField.focused.background = tfgray;
            myTextField.hover.background = tfgray;
        }


        public static bool Button(string str, params GUILayoutOption[] options)
        {
            return GUILayout.Button(str, Main.Settings.useLegacyTheme ? GUI.skin.button : myButton, options);
        }
    }
}