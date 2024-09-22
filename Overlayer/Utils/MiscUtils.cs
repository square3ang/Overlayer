using Overlayer.Core.TextReplacing.Lexing;
using Overlayer.Tags.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Overlayer.Utils
{
    public static class MiscUtils
    {
        static MiscUtils()
        {
            loadedAsss = AppDomain.CurrentDomain.GetAssemblies();
        }
        public static Assembly[] loadedAsss { get; private set; }
        public static Type[] loadedTypes { get; private set; }
        public static void UpdateAssTypes()
        {
            cache = new Dictionary<string, Type>();
            loadedAsss = loadedAsss != null ?
                loadedAsss.Union(AppDomain.CurrentDomain.GetAssemblies()).ToArray() :
                AppDomain.CurrentDomain.GetAssemblies();
            loadedTypes = loadedAsss.Select(ass => ExecuteSafe(ass.GetTypes, out _))
                .Where(t => t != null).SelectMany(ts => ts).ToArray();
        }
        public static Assembly AssByName(string assName)
        {
            return loadedAsss.FirstOrDefault(t => t.FullName == assName) ??
                loadedAsss.FirstOrDefault(t => t.GetName().Name == assName);
        }
        public static Delegate CreateDelegateAuto(this MethodInfo method)
        {
            var prms = method.GetParameters().Select(p => p.ParameterType);
            if (method.ReturnType != typeof(void))
                return method.CreateDelegate(Expression.GetFuncType(prms.Append(method.ReturnType).ToArray()));
            return method.CreateDelegate(Expression.GetActionType(prms.ToArray()));
        }
        public static void ExecuteSafe(Action exec, out Exception ex)
        {
            ex = null;
            try { exec.Invoke(); }
            catch (Exception e) { ex = e; }
        }
        public static T ExecuteSafe<T>(Func<T> exec, out Exception ex)
        {
            ex = null;
            try { return exec.Invoke(); }
            catch (Exception e) { ex = e; return default; }
        }
        public static T ExecuteSafe<T>(Func<T> exec, T defaultValue, out Exception ex)
        {
            ex = null;
            try { return exec.Invoke(); }
            catch (Exception e) { ex = e; return defaultValue; }
        }
        public static TimeSpan MeasureTime(Action a)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            a.Invoke();
            watch.Stop();
            return watch.Elapsed;
        }
        public static Color ShiftHue(Color color, float amount)
        {
            Color.RGBToHSV(color, out float hue, out _, out _);
            hue += amount;
            return Color.HSVToRGB(hue, 1, 1);
        }
        public static Color ShiftSaturation(Color color, float amount)
        {
            Color.RGBToHSV(color, out _, out float sat, out _);
            sat += amount;
            return Color.HSVToRGB(1, sat, 1);
        }
        public static Color ShiftValue(Color color, float amount)
        {
            Color.RGBToHSV(color, out _, out _, out float val);
            val += amount;
            return Color.HSVToRGB(1, 1, val);
        }
        private static Dictionary<string, Type> cache = new Dictionary<string, Type>();
        public static Type TypeByName(string typeName)
        {
            if (cache.TryGetValue(typeName, out var t) && t != null) return t;
            if (loadedTypes == null)
                loadedTypes = loadedAsss.Select(ass => ExecuteSafe(ass.GetTypes, out _)).Where(t => t != null).SelectMany(ts => ts).ToArray();
            return cache[typeName] = Type.GetType(typeName, false) ??
                loadedTypes.FirstOrDefault(t => t.FullName == typeName) ??
                loadedTypes.FirstOrDefault(t => t.Name == typeName);
        }
        public static MethodInfo MethodByName(string typeColonMethodName)
        {
            var typemethod = typeColonMethodName.Split(':');
            var target = TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15422);
            target ??= TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15420);
            return target;
        }
        public static LexConfig CreateLexConfigFromString(string lexOption)
        {
            if (lexOption.Length != 6) return null;
            return new LexConfig()
            {
                TagStart = lexOption[0],
                TagEnd = lexOption[1],
                TagOptSeparator = lexOption[2],
                TagArgStart = lexOption[3],
                TagArgEnd = lexOption[4],
                TagArgSeparator = lexOption[5],
            };
        }
        public static bool SetAttr(object obj, string accessor = "", object value = null)
        {
            if (obj == null) return false;
            Type objType = obj is Type t ? t : obj.GetType();
            accessor = accessor.TrimEnd('.');
            object result = obj;
            string[] accessors = accessor.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (accessors.Length < 1) return false;
            MemberInfo lastMember = null;
            Type type = objType;
            for (int i = 0; i < accessors.Length; i++)
            {
                MemberInfo[] members = type.GetMembers((BindingFlags)15420);
                var ignoreCase = type.GetCustomAttribute<IgnoreCaseAttribute>() != null;
                var foundMembers = ignoreCase ? members.Where(m => m.Name.Equals(accessors[i], StringComparison.OrdinalIgnoreCase)) : members.Where(m => m.Name == accessors[i]);
                lastMember = foundMembers.Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property).FirstOrDefault();
                if (i != accessors.Length - 1)
                {
                    if (lastMember is FieldInfo f) result = f.GetValue(result);
                    else if (lastMember is PropertyInfo p) result = p.GetValue(result);
                    else result = null;
                }
                else
                {
                    if (lastMember is FieldInfo f)
                    {
                        if (value != null && value.GetType() != f.FieldType)
                            value = Convert.ChangeType(value, f.FieldType);
                        f.SetValue(result, value);
                        return true;
                    }
                    else if (lastMember is PropertyInfo p && p.GetSetMethod(true) != null)
                    {
                        if (value != null && value.GetType() != p.PropertyType)
                            value = Convert.ChangeType(value, p.PropertyType);
                        p.SetValue(result, value);
                        return true;
                    }
                    return false;
                }
                if (result == null) return false;
                type = result.GetType();
            }
            return false;
        }
        public static byte[] ExportPNG(RenderTexture rt)
        {
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false, true);
            var oldRt = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = oldRt;
            return tex.EncodeToPNG();
        }
    }
}