using Overlayer.Core.Interfaces;
using Overlayer.Models;
using Overlayer.Utils;
using System;
using System.Linq;
using UnityEngine;
using TKM = Overlayer.Core.Translation.TranslationKeys.Misc;

namespace Overlayer.Core
{
    public static class Drawer
    {
        public static bool DrawVector2(string label, ref Vector2 vec2, float lValue, float rValue)
        {
            bool changed = false;
            ButtonLabel($"<b>{label}</b>", () => Application.OpenURL(Main.DiscordLink));
            changed |= DrawSingleWithSlider("X", ref vec2.x, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Y", ref vec2.y, lValue, rValue, 300f);
            return changed;
        }
        public static bool DrawVector3(string label, ref Vector3 vec3, float lValue, float rValue)
        {
            bool changed = false;
            ButtonLabel($"<b>{label}</b>", () => Application.OpenURL(Main.DiscordLink));
            changed |= DrawSingleWithSlider("X", ref vec3.x, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Y", ref vec3.y, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Z", ref vec3.z, lValue, rValue, 300f);
            return changed;
        }
        public static void DrawGColor(string label, ref GColor color, bool canEnableGradient, Action onChange)
        {
            ButtonLabel(label, () => Application.OpenURL(Main.DiscordLink));
            DrawGColor(ref color, canEnableGradient).IfTrue(onChange);
        }
        public static bool DrawGColor(ref GColor color, bool canEnableGradient)
        {
            bool ge = color.gradientEnabled, prevGe = color.gradientEnabled;
            if (canEnableGradient && DrawBool(Main.Lang[TKM.EnableGradient], ref ge))
                color = color with { gradientEnabled = ge };
            color.gradientEnabled &= canEnableGradient;
            bool result = ge != prevGe;
            if (color.gradientEnabled)
            {
                Color tl = color.topLeft, tr = color.topRight,
                bl = color.bottomLeft, br = color.bottomRight;
                ExpandableGUI(color.topLeftStatus, Main.Lang[TKM.TopLeft], () => result |= DrawColor(ref tl));
                ExpandableGUI(color.topRightStatus, Main.Lang[TKM.TopRight], () => result |= DrawColor(ref tr));
                ExpandableGUI(color.bottomLeftStatus, Main.Lang[TKM.BottomLeft], () => result |= DrawColor(ref bl));
                ExpandableGUI(color.bottomRightStatus, Main.Lang[TKM.BottomRight], () => result |= DrawColor(ref br));
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
            bool result = false;
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
            return result;
        }
        public static void TitleButton(string label, string btnLabel, Action pressed, Action horizontal = null)
        {
            GUILayout.BeginHorizontal();
            ButtonLabel(label, () => Application.OpenURL(Main.DiscordLink));
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
        public static bool DrawStringArray(ref string[] array, Action<int> arrayResized = null, Action<int> elementRightGUI = null, Action<int, string> onElementChange = null)
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
                Drawer.ButtonLabel($"{i}: ", () => Application.OpenURL(Main.DiscordLink));
                cache = GUILayout.TextField(cache);
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
            Drawer.ButtonLabel(label, () => Application.OpenURL(Main.DiscordLink));
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
        public static bool DrawBool(string label, ref bool value)
        {
            bool prev = value;
            GUILayout.BeginHorizontal();
            Drawer.ButtonLabel(label, () => Application.OpenURL(Main.DiscordLink));
            value = GUILayout.Toggle(value, string.Empty);
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
            bool result = UnityModManagerNet.UnityModManager.UI.PopupToggleGroup(ref current, names, label, unique);
            @enum = EnumHelper<T>.GetValues()[current];
            return result;
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
                    Drawer.ButtonLabel($"{label}{obj}", () => Application.OpenURL(Main.DiscordLink));
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
            ButtonLabel(label, () => Application.OpenURL(Main.DiscordLink));
            if (!textArea)
                value = GUILayout.TextField(value);
            else value = GUILayout.TextArea(value);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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
        static readonly string[] ilovesuckyoubus =
        {
            "<color=#FF8800>나</color><color=#E9922A>는</color> <color=#BEA77F>석</color><color=#A8B2AA>큐</color><color=#92BCD4>버</color><color=#7DC7FF>스</color><color=#6AD0DE>를</color> <color=#45E39D>좋</color><color=#32EC7D>아</color><color=#1FF55D>해</color><color=#0DFF3C>요</color>",
            "<color=#1251FF>나</color><color=#0F6ED8>는</color> <color=#09A88C>석</color><color=#06C566>큐</color><color=#03E240>버</color><color=#00FF1A>스</color><color=#03E240>를</color> <color=#09A88C>사</color><color=#0C8BB2>랑</color><color=#0F6ED8>해</color><color=#1251FF>요</color>",
            "<color=#FF14C0>나</color><color=#EF26B2>는</color> <color=#D04A96>석</color><color=#C05C88>큐</color><color=#B16E7B>버</color><color=#A1806D>스</color><color=#92925F>를</color> <color=#73B644>연</color><color=#63C836>모</color><color=#54DA28>해</color><color=#44EC1A>요</color>",
            "<color=#FF94F4>나</color><color=#ED96F4>는</color> <color=#CB9AF6>석</color><color=#BA9CF7>큐</color><color=#A99EF8>버</color><color=#98A0F9>스</color><color=#87A2F9>를</color> <color=#65A6FB>사</color><color=#54A8FC>모</color><color=#43AAFD>해</color><color=#32ACFE>요</color>",
            "<color=#FF0000>나</color><color=#FF6D00>는</color> <color=#B6FF00>석</color><color=#48FF00>큐</color><color=#00FF24>버</color><color=#00FF91>스</color><color=#00FEFF>를</color> <color=#0024FF>귀</color><color=#4800FF>여</color><color=#B600FF>워</color><color=#FF00DA>해</color><color=#FF006D>요</color>",
            "<color=#FF73F6>나</color><color=#DC86F7>는</color> <color=#97AEFA>석</color><color=#75C2FC>큐</color><color=#52D6FD>버</color><color=#30EAFF>스</color><color=#52D6FD>를</color> <color=#97AEFA>존</color><color=#BA9AF9>경</color><color=#DC86F7>해</color><color=#FF73F6>요</color>",
            "<color=#30EAFF>나</color><color=#35EDE0>는</color> <color=#40F4A4>석</color><color=#45F886>큐</color><color=#4AFB68>버</color><color=#4FFF4A>스</color><color=#6DEF62>를</color> <color=#A7D093>예</color><color=#C4C1AC>뻐</color><color=#E1B2C4>해</color><color=#FEA2DD>요</color>",
            "<color=#47FFE0>나</color><color=#64E5E5>는</color> <color=#9EB1EF>석</color><color=#BB97F4>큐</color><color=#D87DF9>버</color><color=#F563FE>스</color><color=#D87DF9>를</color> <color=#9EB1EF>동</color><color=#81CBEA>경</color><color=#64E5E5>해</color><color=#47FFE0>요</color>",
        };
        static int index = 0;
        static float targetTime = 0;
        public static void ButtonLabel(string label, Action onPressed, params GUILayoutOption[] options)
        {
            if (targetTime > Time.time)
                label = ilovesuckyoubus[index];
            if (GUILayout.Button(label, GUI.skin.label, options))
            {
                targetTime = Time.time + 1.5f;
                if (++index == ilovesuckyoubus.Length) index = 0;
                Main.Logger.Log(ilovesuckyoubus[index]);
            }
        }
        public static bool Button(string label)
        {
            if (targetTime > Time.time)
                label = ilovesuckyoubus[index];
            return GUILayout.Button(label);
        }
        public static bool LabelButton(string label)
        {
            if (targetTime > Time.time)
                label = ilovesuckyoubus[index];
            return GUILayout.Button(label, GUI.skin.label);
        }
    }
}
