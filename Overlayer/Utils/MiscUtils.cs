using Overlayer.Models;
using Overlayer.Tags.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace Overlayer.Utils;

public static class MiscUtils {
    static MiscUtils() => loadedAsss = AppDomain.CurrentDomain.GetAssemblies();
    public static Assembly[] loadedAsss { get; private set; }
    public static Type[] loadedTypes { get; private set; }
    public static void UpdateAssTypes() {
        cache = [];
        loadedAsss = loadedAsss != null ?
            loadedAsss.Union(AppDomain.CurrentDomain.GetAssemblies()).ToArray() :
            AppDomain.CurrentDomain.GetAssemblies();
        loadedTypes = loadedAsss.Select(ass => ExecuteSafe(ass.GetTypes, out _))
            .Where(t => t != null).SelectMany(ts => ts).ToArray();
    }
    public static Assembly AssByName(string assName) {
        return loadedAsss.FirstOrDefault(t => t.FullName == assName) ??
            loadedAsss.FirstOrDefault(t => t.GetName().Name == assName);
    }
    public static Delegate CreateDelegateAuto(this MethodInfo method) {
        var prms = method.GetParameters().Select(p => p.ParameterType);
        return method.ReturnType != typeof(void)
            ? method.CreateDelegate(Expression.GetFuncType(prms.Append(method.ReturnType).ToArray()))
            : method.CreateDelegate(Expression.GetActionType(prms.ToArray()));
    }
    public static void ExecuteSafe(Action exec, out Exception ex) {
        ex = null;
        try { exec.Invoke(); } catch(Exception e) { ex = e; }
    }
    public static T ExecuteSafe<T>(Func<T> exec, out Exception ex) {
        ex = null;
        try { return exec.Invoke(); } catch(Exception e) { ex = e; return default; }
    }
    public static T ExecuteSafe<T>(Func<T> exec, T defaultValue, out Exception ex) {
        ex = null;
        try { return exec.Invoke(); } catch(Exception e) { ex = e; return defaultValue; }
    }
    public static TimeSpan MeasureTime(Action a) {
        Stopwatch watch = new();
        watch.Start();
        a.Invoke();
        watch.Stop();
        return watch.Elapsed;
    }
    public static Color ShiftHue(Color color, float amount) {
        Color.RGBToHSV(color, out float hue, out _, out _);
        hue += amount;
        return Color.HSVToRGB(hue, 1, 1);
    }
    public static Color ShiftSaturation(Color color, float amount) {
        Color.RGBToHSV(color, out _, out float sat, out _);
        sat += amount;
        return Color.HSVToRGB(1, sat, 1);
    }
    public static Color ShiftValue(Color color, float amount) {
        Color.RGBToHSV(color, out _, out _, out float val);
        val += amount;
        return Color.HSVToRGB(1, 1, val);
    }
    private static Dictionary<string, Type> cache = [];
    public static Type TypeByName(string typeName) {
        if(cache.TryGetValue(typeName, out var t) && t != null) {
            return t;
        }

        loadedTypes ??= loadedAsss.Select(ass => ExecuteSafe(ass.GetTypes, out _)).Where(t => t != null).SelectMany(ts => ts).ToArray();
        return cache[typeName] = Type.GetType(typeName, false) ??
            loadedTypes.FirstOrDefault(t => t.FullName == typeName) ??
            loadedTypes.FirstOrDefault(t => t.Name == typeName);
    }
    public static MethodInfo MethodByName(string typeColonMethodName) {
        var typemethod = typeColonMethodName.Split(':');
        var target = TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15422);
        target ??= TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15420);
        return target;
    }

    public static bool SetAttr(object obj, string accessor = "", object value = null) {
        if(obj == null) {
            return false;
        }

        Type objType = obj is Type t ? t : obj.GetType();
        accessor = accessor.TrimEnd('.');
        object result = obj;
        string[] accessors = accessor.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
        if(accessors.Length < 1) {
            return false;
        }

        MemberInfo lastMember = null;
        Type type = objType;
        for(int i = 0; i < accessors.Length; i++) {
            MemberInfo[] members = type.GetMembers((BindingFlags)15420);
            var ignoreCase = type.GetCustomAttribute<IgnoreCaseAttribute>() != null;
            var foundMembers = ignoreCase ? members.Where(m => m.Name.Equals(accessors[i], StringComparison.OrdinalIgnoreCase)) : members.Where(m => m.Name == accessors[i]);
            lastMember = foundMembers.Where(m => m.MemberType is MemberTypes.Field or MemberTypes.Property).FirstOrDefault();
            if(i != accessors.Length - 1) {
                result = lastMember is FieldInfo f ? f.GetValue(result) : lastMember is PropertyInfo p ? p.GetValue(result) : null;
            } else {
                if(lastMember is FieldInfo f) {
                    if(value != null && value.GetType() != f.FieldType) {
                        value = Convert.ChangeType(value, f.FieldType);
                    }

                    f.SetValue(result, value);
                    return true;
                } else if(lastMember is PropertyInfo p && p.GetSetMethod(true) != null) {
                    if(value != null && value.GetType() != p.PropertyType) {
                        value = Convert.ChangeType(value, p.PropertyType);
                    }

                    p.SetValue(result, value);
                    return true;
                }
                return false;
            }
            if(result == null) {
                return false;
            }

            type = result.GetType();
        }
        return false;
    }
    public static byte[] ExportPNG(RenderTexture rt) {
        Texture2D tex = new(rt.width, rt.height, TextureFormat.RGBAFloat, false, true);
        var oldRt = RenderTexture.active;
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = oldRt;
        return tex.EncodeToPNG();
    }

    public static Vector2 AlignmentToPivot(TextAlignmentOptions alignment) {
        float x = ((int)alignment & (1 << 0)) != 0 ? 0f :
                  ((int)alignment & (1 << 2)) != 0 ? 1f : 0.5f;

        float y = ((int)alignment & (1 << 8)) != 0 ? 1f :
                  ((int)alignment & (1 << 10)) != 0f ? 0 : 0.5f;

        return new Vector2(x, y);
    }

    static Rect _lastRect;
    public static bool IsHovering() {
        if(Event.current.type == EventType.Repaint) {
            _lastRect = GUILayoutUtility.GetLastRect();
        }
        return _lastRect.Contains(Event.current.mousePosition);
    }

    private static float NextFloat(string s, ref int idx) {
        int start = idx;

        while(idx < s.Length && s[idx] != ',') {
            idx++;
        }

        float v = float.Parse(s.Substring(start, idx - start), System.Globalization.CultureInfo.InvariantCulture);
        idx++;

        return v;
    }
    public static GColor ParseGColor(string s) {
        int idx = 0;

        return new GColor {
            topLeft = new Color(
                NextFloat(s, ref idx), NextFloat(s, ref idx),
                NextFloat(s, ref idx), NextFloat(s, ref idx)
            ),
            topRight = new Color(
                NextFloat(s, ref idx), NextFloat(s, ref idx),
                NextFloat(s, ref idx), NextFloat(s, ref idx)
            ),
            bottomLeft = new Color(
                NextFloat(s, ref idx), NextFloat(s, ref idx),
                NextFloat(s, ref idx), NextFloat(s, ref idx)
            ),
            bottomRight = new Color(
                NextFloat(s, ref idx), NextFloat(s, ref idx),
                NextFloat(s, ref idx), NextFloat(s, ref idx)
            ),
            gradientEnabled = true
        };
    }
    public static Vector2 ParseVec2(string s) {
        int idx = 0;

        return new Vector2(
            NextFloat(s, ref idx),
            float.Parse(s.Substring(idx), System.Globalization.CultureInfo.InvariantCulture)
        );
    }
    public static Vector3 ParseVec3(string s) {
        int idx = 0;

        float x = NextFloat(s, ref idx);
        float y = NextFloat(s, ref idx);
        float z = float.Parse(s.Substring(idx), System.Globalization.CultureInfo.InvariantCulture);

        return new Vector3(x, y, z);
    }
    public static Color ParseColor(string s) {
        int idx = 0;

        float r = NextFloat(s, ref idx);
        float g = NextFloat(s, ref idx);
        float b = NextFloat(s, ref idx);
        float a = float.Parse(s.Substring(idx), System.Globalization.CultureInfo.InvariantCulture);

        return new Color(r, g, b, a);
    }
    public static TextAlignmentOptions ParseAlign(string s) => (TextAlignmentOptions)int.Parse(s);
}