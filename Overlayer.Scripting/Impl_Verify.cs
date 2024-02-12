using DG.Tweening;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime.Interop;
using JSNet.API;
using JSNet.Utils;
using Overlayer.Core;
using Overlayer.Tags;
using Overlayer.Unity;
using System;
using System.Reflection;
using UnityEngine;

namespace Overlayer.Scripting
{
    public static class Impl_Verify
    {
        [ThreadStatic]
        public static bool Verified = false;
        #region Impl APIs
        [Api("verifySafety")]
        public static bool VerifySafety() => false;
        [Api("use")]
        public static bool Use(Engine engine, params string[] tags) => false;
        [Api("resolveClrType")]
        public static Type ResolveType(string clrType) => SafetyCheckFail<Type>();
        [Api("resolveClrMethod")]
        public static MethodInfo ResolveMethod(string clrType, string name) => SafetyCheckFail<MethodInfo>();
        [Api("resolve")]
        public static TypeReference Resolve(Engine engine, string clrType) => SafetyCheckFail<TypeReference>();
        [Api("getClrGenericTypeName")]
        public static string GetGenericClrTypeString(string genericType, string[] genericArgs) => SafetyCheckFail(string.Empty);
        [Api("getGlobalVariable")]
        public static object GetGlobalVariable(string name) => 0;
        public delegate object CallWrapper(params object[] args);
        [Api("setGlobalVariable")]
        public static object SetGlobalVariable(string name, object obj)
        {
            if (obj is FunctionInstance fi)
                CheckSafety(fi);
            return null;
        }
        [Api("registerTag")]
        public static void RegisterTag(string name, JsValue func, bool notplaying) => CheckSafety(func);
        [Api("unregisterTag")]
        public static void UnregisterTag(string name) { }
        [Api("prefix")]
        public static bool Prefix(string typeColonMethodName, JsValue patch) => SafetyCheckFail<bool>();
        [Api("postfix")]
        public static bool Postfix(string typeColonMethodName, JsValue patch) => SafetyCheckFail<bool>();
        [Api("transpiler")]
        public static bool Transpiler(string typeColonMethodName, JsValue patch) => SafetyCheckFail<bool>();
        [Api("prefixWithArgs")]
        public static bool PrefixWithArgs(string typeColonMethodName, string[] argumentClrTypes, JsValue patch) => SafetyCheckFail<bool>();
        [Api("postfixWithArgs")]
        public static bool PostfixWithArgs(string typeColonMethodName, string[] argumentClrTypes, JsValue patch) => SafetyCheckFail<bool>();
        [Api("transpilerWithArgs")]
        public static bool TranspilerWithArgs(string typeColonMethodName, string[] argumentClrTypes, JsValue patch) => SafetyCheckFail<bool>();
        [Api("isNoFailMode")]
        public static bool IsNoFailMode() => false;
        [Api("getLanguage", RequireTypes = new[] { typeof(SystemLanguage) })]
        public static SystemLanguage GetLanguage() => SystemLanguage.English;
        [Api("isAutoEnabled")]
        public static bool IsAutoEnabled() => false;
        [Api("isWeakAutoEnabled")]
        public static bool IsWeakAutoEnabled() => false;
        [Api("ease", RequireTypes = new Type[] { typeof(Ease) })]
        public static float EasedValue(Ease ease, float lifetime) => lifetime;
        [Api("easeColor", RequireTypes = new Type[] { typeof(Color) })]
        public static Color EasedColor(Color color, Ease ease, float lifetime) => Color.clear;
        [Api("easeColorFromTo")]
        public static Color EasedColor(Color from, Color to, Ease ease, float lifetime) => Color.clear;
        [Api("colorFromHexRGB")]
        public static Color FromHexRGB(string rgbHex) => Color.clear;
        [Api("colorFromHexRGBA")]
        public static Color FromHexRGBA(string rgbaHex) => Color.clear;
        [Api("colorToHexRGB")]
        public static string ToHexRGB(Color color) => "FFFFFF";
        [Api("colorToHexRGBA")]
        public static string ToHexRGBA(Color color) => "FFFFFFFF";
        [Api("getTagValueSafe")]
        public static string GetTagValueSafe(string tagName, params string[] args) => "0";
        [Api(RequireTypes = new[] { typeof(KeyCode) })]
        public class On
        {
            [Api("rewind", Comment = new[]
            {
                "On ADOFAI Rewind (Level Start, Scene Moved, etc..)"
            })]
            public static void Rewind(JsValue func) => CheckSafety(func);
            [Api("hit", Comment = new[]
            {
                "On Tile Hit"
            })]
            public static void Hit(JsValue func) => CheckSafety(func);
            [Api("dead", Comment = new[]
             {
                "On Dead"
            })]
            public static void Dead(JsValue func) => CheckSafety(func);
            [Api("fail", Comment = new[]
             {
                "On Fail"
            })]
            public static void Fail(JsValue func) => CheckSafety(func);
            [Api("clear", Comment = new[]
             {
                "On Clear"
            })]
            public static void Clear(JsValue func) => CheckSafety(func);
            #region KeyEvents
            [Api("anyKey", Comment = new[]
            {
                "On Any Key Pressed"
            })]
            public static void AnyKey(JsValue func) => CheckSafety(func);
            [Api("anyKeyDown", Comment = new[]
            {
                "On Any Key Down"
            })]
            public static void AnyKeyDown(JsValue func) => CheckSafety(func);
            [Api("key", Comment = new[]
            {
                "On Key Pressed"
            })]
            public static void Key(KeyCode key, JsValue func) => CheckSafety(func);
            [Api("keyUp", Comment = new[]
            {
                "On Key Up"
            })]
            public static void KeyUp(KeyCode key, JsValue func) => CheckSafety(func);
            [Api("keyDown", Comment = new[]
            {
                "On Key Down"
            })]
            public static void KeyDown(KeyCode key, JsValue func) => CheckSafety(func);
            #endregion
        }
        #endregion
        public static T SafetyCheckFail<T>(T defaultValue = default)
        {
            Verified = false;
            return defaultValue;
        }
        public static void CheckSafety(JsValue func)
        {
            try
            {
                if (func is FunctionInstance fi)
                    new FIWrapper(fi).CallRaw();
            }
            catch { }
        }
    }
}
