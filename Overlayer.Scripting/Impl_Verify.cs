using DG.Tweening;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using JSNet;
using JSNet.API;
using JSNet.Utils;
using System;
using System.Dynamic;
using UnityEngine;

namespace Overlayer.Scripting
{
    public static class Impl_Verify
    {
        [ThreadStatic]
        public static bool Verified = true;
        #region Impl APIs
        [Api("ignoreSafety")]
        public static void IgnoreSafety() { }
        [Api("use")]
        public static bool Use(Engine engine, params string[] tags) => false;
        [Api("resolveClrType")]
        public static dynamic ResolveType(string clrType) => SafetyCheckFail();
        [Api("resolveClrMethod")]
        public static dynamic ResolveMethod(string clrType, string name) => SafetyCheckFail();
        [Api("resolve")]
        public static dynamic Resolve(Engine engine, string clrType) => SafetyCheckFail();
        [Api("getClrGenericTypeName")]
        public static dynamic GetGenericClrTypeString(string genericType, string[] genericArgs) => SafetyCheckFail();
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
        public static dynamic Prefix(string typeColonMethodName, JsValue patch) => SafetyCheckFail();
        [Api("postfix")]
        public static dynamic Postfix(string typeColonMethodName, JsValue patch) => SafetyCheckFail();
        [Api("transpiler")]
        public static dynamic Transpiler(string typeColonMethodName, JsValue patch) => SafetyCheckFail();
        [Api("prefixWithArgs")]
        public static dynamic PrefixWithArgs(string typeColonMethodName, string[] argumentClrTypes, JsValue patch) => SafetyCheckFail();
        [Api("postfixWithArgs")]
        public static dynamic PostfixWithArgs(string typeColonMethodName, string[] argumentClrTypes, JsValue patch) => SafetyCheckFail();
        [Api("transpilerWithArgs")]
        public static dynamic TranspilerWithArgs(string typeColonMethodName, string[] argumentClrTypes, JsValue patch) => SafetyCheckFail();
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
        public static bool IsSafe(string expr)
        {
            Verified = true;
            try { Script.InterpretAPI(Main.JSVerifyApi, expr).Exec(); }
            catch { return false; }
            return Verified;
        }
        public static dynamic SafetyCheckFail()
        {
            Verified = false;
            throw new NotSafeScriptException();
            //return new DummyObject();
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
        public class DummyObject : DynamicObject
        {
            public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
            {
                result = new DummyObject();
                return true;
            }
            public override bool TryConvert(ConvertBinder binder, out object result)
            {
                result = new DummyObject();
                return true;
            }
            public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
            {
                result = new DummyObject();
                return true;
            }
            public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
            {
                return true;
            }
            public override bool TryDeleteMember(DeleteMemberBinder binder)
            {
                return true;
            }
            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
                result = new DummyObject();
                return true;
            }
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = new DummyObject();
                return true;
            }
            public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
            {
                result = new DummyObject();
                return true;
            }
            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                result = new DummyObject();
                return true;
            }
            public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
            {
                return true;
            }
            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                return true;
            }
            public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
            {
                result = new DummyObject();
                return true;
            }
        }
    }
}
