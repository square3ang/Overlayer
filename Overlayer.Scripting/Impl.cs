using DG.Tweening;
using HarmonyLib;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime.Interop;
using JSNet;
using JSNet.API;
using JSNet.Utils;
using Overlayer.Core;
using Overlayer.Core.Patches;
using Overlayer.Tags;
using Overlayer.Unity;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace Overlayer.Scripting
{
    public static class Impl
    {
        public class Context
        {
            public bool isFirstCall = true;
            public bool ignoreSafety = false;
        }
        public static void Initialize()
        {
            InitializeWrapperAssembly();
            alreadyExecutedScripts = new HashSet<string>();
            jsTypes = new Dictionary<Engine, Dictionary<string, TypeReference>>();
            harmony = new Harmony("Overlayer.Scripting.Impl");
            globalVariables = new Dictionary<string, object>();
            registeredCustomTags = new List<string>();
        }
        public static void Release()
        {
            apiMethods.RemoveAll(t => registeredCustomTags?.Contains(t.Item1.Name) ?? false);
            registeredCustomTags?.ForEach(TagManager.RemoveTag);
            registeredCustomTags = null;
            globalVariables = null;
            harmony?.UnpatchAll(harmony.Id);
            harmony = null;
            jsTypes = null;
            alreadyExecutedScripts = null;
            DisposeWrapperAssembly();
        }
        public static void Reload()
        {
            Release();
            Initialize();
        }
        #region Impl APIs
        [Api("ignoreSafety", Comment = new string[]
        {
            "Ignore Safety Check",
            "!!Warning!! This Function Breaks Full Safety!"
        })]
        public static void IgnoreSafety(Engine engine)
        {
            if (!engine.GetContext<Context>().isFirstCall)
                throw new InvalidOperationException("ignoreSafety Function Must Be Called At First!");
            engine.GetContext<Context>().ignoreSafety = true;
            SetNotFirstCall(engine);
        }
        [Api("use")]
        public static bool Use(Engine engine, params string[] tags)
        {
            SetNotFirstCall(engine);
            bool result = true;
            for (int i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                if (TagManager.GetTag(tag) != null)
                    LazyPatchManager.PatchAll(tag).ForEach(lp => lp.Locked = true);
                else
                {
                    if (!File.Exists(tag))
                        tag = Path.Combine(Main.ScriptPath, tag);
                    if (!File.Exists(tag))
                    {
                        result = false;
                        continue;
                    }

                    var name = Path.GetFileName(tag);
                    alreadyExecutedScripts.Add(tag);

                    var time = MiscUtils.MeasureTime(() =>
                    {
                        var result = Script.InterpretAPI(Main.JSApi, File.ReadAllText(tag));
                        result.Exec();
                        Main.JSApi.CombineInterpreter(engine);
                        result.Dispose();
                    });
                    Main.Logger.Log($"Force Executed \"{name}\" Script Successfully. ({time.TotalMilliseconds}ms)");
                }
            }
            return result;
        }
        [Api("resolveClrType")]
        public static Type ResolveType(Engine engine, string clrType)
        {
            SetNotFirstCall(engine);
            EnsureSafety(engine, "resolveClrType");
            return MiscUtils.TypeByName(clrType);
        }
        [Api("resolveClrMethod")]
        public static MethodInfo ResolveMethod(Engine engine, string clrType, string name)
        {
            SetNotFirstCall(engine);
            EnsureSafety(engine, "resolveClrMethod");
            return MiscUtils.TypeByName(clrType)?.GetMethod(name, (BindingFlags)15420);
        }
        [Api("resolve")]
        public static TypeReference Resolve(Engine engine, string clrType)
        {
            SetNotFirstCall(engine);
            EnsureSafety(engine, "resolve");
            if (jsTypes.TryGetValue(engine, out var dict))
                if (dict.TryGetValue(clrType, out var t))
                    return t;
                else return dict[clrType] = TypeReference.CreateTypeReference(engine, MiscUtils.TypeByName(clrType));
            dict = jsTypes[engine] = new Dictionary<string, TypeReference>();
            return dict[clrType] = TypeReference.CreateTypeReference(engine, MiscUtils.TypeByName(clrType));
        }
        [Api("getClrGenericTypeName")]
        public static string GetGenericClrTypeString(Engine engine, string genericType, string[] genericArgs)
        {
            SetNotFirstCall(engine);
            EnsureSafety(engine, "getClrGenericTypeName");
            string AggregateGenericArgs(Type[] types)
            {
                StringBuilder sb = new StringBuilder();
                int length = types.Length;
                for (int i = 0; i < length; i++)
                {
                    Type type = types[i];
                    sb.Append($"[{type?.FullName}, {type?.Assembly.GetName().Name}]");
                    if (i < length - 1)
                        sb.Append(',');
                }
                return sb.ToString();
            }
            var t = MiscUtils.TypeByName(genericType);
            var args = genericArgs.Select(MiscUtils.TypeByName);
            return $"{t?.FullName}[{AggregateGenericArgs(args.ToArray())}]";
        }
        [Api("getGlobalVariable")]
        public static object GetGlobalVariable(Engine engine, string name)
        {
            SetNotFirstCall(engine);
            return globalVariables.TryGetValue(name, out var value) ? value : null;
        }
        public delegate object CallWrapper(params object[] args);
        [Api("setGlobalVariable")]
        public static object SetGlobalVariable(Engine engine, string name, object obj)
        {
            SetNotFirstCall(engine);
            if (obj is FunctionInstance fi)
            {
                FIWrapper wrapper = new FIWrapper(fi);
                obj = (CallWrapper)wrapper.Call;
            }
            return globalVariables[name] = obj;
        }
        [Api("registerTag")]
        public static void RegisterTag(Engine engine, string name, JsValue func, bool notplaying)
        {
            SetNotFirstCall(engine);
            if (!(func is FunctionInstance fi)) return;
            FIWrapper wrapper = new FIWrapper(fi);
            var tagWrapper = GenerateTagWrapper(wrapper);
            TagManager.SetTag(new OverlayerTag(tagWrapper, new Tags.Attributes.TagAttribute(name) { NotPlaying = notplaying }));
            StaticCoroutine.Queue(StaticCoroutine.SyncRunner(TextManager.Refresh));
            apiMethods.Add((new ApiAttribute(name), tagWrapper));
            registeredCustomTags.Add(name);
            Main.Logger.Log($"Registered Tag \"{name}\" (NotPlaying:{notplaying})");
        }
        [Api("unregisterTag")]
        public static void UnregisterTag(Engine engine, string name)
        {
            SetNotFirstCall(engine);
            TagManager.RemoveTag(name);
            StaticCoroutine.Queue(StaticCoroutine.SyncRunner(TextManager.Refresh));
        }
        [Api("prefix")]
        public static bool Prefix(Engine engine, string typeColonMethodName, JsValue patch)
        {
            SetNotFirstCall(engine);
            EnsureSafety(engine, "prefix");
            if (!(patch is FunctionInstance func)) return false;
            var typemethod = typeColonMethodName.Split2(':');
            var target = MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15422);
            target ??= MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15420);
            if (target == null)
            {
                Main.Logger.Log($"{typeColonMethodName} Cannot Be Found.");
                return false;
            }
            var wrap = func.Wrap(target, true);
            if (wrap == null)
                return false;
            harmony.Patch(target, new HarmonyMethod(wrap));
            return true;
        }
        [Api("postfix")]
        public static bool Postfix(Engine engine, string typeColonMethodName, JsValue patch)
        {
            SetNotFirstCall(engine);
            EnsureSafety(engine, "postfix");
            if (!(patch is FunctionInstance func)) return false;
            var typemethod = typeColonMethodName.Split2(':');
            var target = MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15422);
            target ??= MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15420);
            if (target == null)
            {
                Main.Logger.Log($"{typeColonMethodName} Cannot Be Found.");
                return false;
            }
            var wrap = func.Wrap(target, false);
            if (wrap == null)
                return false;
            harmony.Patch(target, postfix: new HarmonyMethod(wrap));
            return true;
        }
        [Api("transpiler")]
        public static bool Transpiler(Engine engine, string typeColonMethodName, JsValue patch)
        {
            SetNotFirstCall(engine);
            EnsureSafety(engine, "transpiler");
            if (!(patch is FunctionInstance func)) return false;
            var typemethod = typeColonMethodName.Split2(':');
            var target = MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15422);
            target ??= MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15420);
            if (target == null)
            {
                Main.Logger.Log($"{typeColonMethodName} Cannot Be Found.");
                return false;
            }
            var wrap = func.WrapTranspiler();
            if (wrap == null)
                return false;
            harmony.Patch(target, transpiler: new HarmonyMethod(wrap));
            return true;
        }
        [Api("prefixWithArgs")]
        public static bool PrefixWithArgs(Engine engine, string typeColonMethodName, string[] argumentClrTypes, JsValue patch)
        {
            SetNotFirstCall(engine);
            EnsureSafety(engine, "prefixWithArgs");
            if (!(patch is FunctionInstance func)) return false;
            var typemethod = typeColonMethodName.Split2(':');
            var argTypes = argumentClrTypes.Select(MiscUtils.TypeByName).ToArray();
            var target = MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15422, null, argTypes, null);
            target ??= MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15420, null, argTypes, null);
            if (target == null)
            {
                Main.Logger.Log($"{typeColonMethodName} Cannot Be Found.");
                return false;
            }
            var wrap = func.Wrap(target, true);
            if (wrap == null)
                return false;
            harmony.Patch(target, new HarmonyMethod(wrap));
            return true;
        }
        [Api("postfixWithArgs")]
        public static bool PostfixWithArgs(Engine engine, string typeColonMethodName, string[] argumentClrTypes, JsValue patch)
        {
            SetNotFirstCall(engine);
            EnsureSafety(engine, "postfixWithArgs");
            if (!(patch is FunctionInstance func)) return false;
            var typemethod = typeColonMethodName.Split2(':');
            var argTypes = argumentClrTypes.Select(MiscUtils.TypeByName).ToArray();
            var target = MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15422, null, argTypes, null);
            target ??= MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15420, null, argTypes, null);
            if (target == null)
            {
                Main.Logger.Log($"{typeColonMethodName} Cannot Be Found.");
                return false;
            }
            var wrap = func.Wrap(target, false);
            if (wrap == null)
                return false;
            harmony.Patch(target, postfix: new HarmonyMethod(wrap));
            return true;
        }
        [Api("transpilerWithArgs")]
        public static bool TranspilerWithArgs(Engine engine, string typeColonMethodName, string[] argumentClrTypes, JsValue patch)
        {
            SetNotFirstCall(engine);
            EnsureSafety(engine, "transpilerWithArgs");
            if (!(patch is FunctionInstance func)) return false;
            var typemethod = typeColonMethodName.Split2(':');
            var argTypes = argumentClrTypes.Select(MiscUtils.TypeByName).ToArray();
            var target = MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15422, null, argTypes, null);
            target ??= MiscUtils.TypeByName(typemethod[0]).GetMethod(typemethod[1], (BindingFlags)15420, null, argTypes, null);
            if (target == null)
            {
                Main.Logger.Log($"{typeColonMethodName} Cannot Be Found.");
                return false;
            }
            var wrap = func.WrapTranspiler();
            if (wrap == null)
                return false;
            harmony.Patch(target, transpiler: new HarmonyMethod(wrap));
            return true;
        }
        [Api("isNoFailMode")]
        public static bool IsNoFailMode(Engine engine)
        {
            SetNotFirstCall(engine);
            return scrController.instance?.noFail ?? false;
        }
        [Api("getLanguage", RequireTypes = new[] { typeof(SystemLanguage) })]
        public static SystemLanguage GetLanguage(Engine engine)
        {
            SetNotFirstCall(engine);
            return RDString.language;
        }
        [Api("isAutoEnabled")]
        public static bool IsAutoEnabled(Engine engine)
        {
            SetNotFirstCall(engine);
            return RDC.auto;
        }
        [Api("isWeakAutoEnabled")]
        public static bool IsWeakAutoEnabled(Engine engine)
        {
            SetNotFirstCall(engine);
            return RDC.useOldAuto;
        }
        [Api("ease", RequireTypes = new Type[] { typeof(Ease) })]
        public static float EasedValue(Engine engine, Ease ease, float lifetime)
        {
            SetNotFirstCall(engine);
            return DOVirtual.EasedValue(0, 1, lifetime, ease);
        }
        [Api("easeColor", RequireTypes = new Type[] { typeof(Color) })]
        public static Color EasedColor(Engine engine, Color color, Ease ease, float lifetime)
        {
            SetNotFirstCall(engine);
            return color * DOVirtual.EasedValue(0, 1, lifetime, ease);
        }
        [Api("easeColorFromTo")]
        public static Color EasedColor(Engine engine, Color from, Color to, Ease ease, float lifetime)
        {
            SetNotFirstCall(engine);
            return from + ((to - from) * DOVirtual.EasedValue(0, 1, lifetime, ease));
        }
        [Api("colorFromHexRGB")]
        public static Color FromHexRGB(Engine engine, string rgbHex)
        {
            SetNotFirstCall(engine);
            return ColorUtility.TryParseHtmlString('#' + rgbHex, out var color) ? color : Color.clear;
        }
        [Api("colorFromHexRGBA")]
        public static Color FromHexRGBA(Engine engine, string rgbaHex)
        {
            SetNotFirstCall(engine);
            return ColorUtility.TryParseHtmlString('#' + rgbaHex, out var color) ? color : Color.clear;
        }
        [Api("colorToHexRGB")]
        public static string ToHexRGB(Engine engine, Color color)
        {
            SetNotFirstCall(engine);
            return ColorUtility.ToHtmlStringRGB(color);
        }
        [Api("colorToHexRGBA")]
        public static string ToHexRGBA(Engine engine, Color color)
        {
            SetNotFirstCall(engine);
            return ColorUtility.ToHtmlStringRGBA(color);
        }
        [Api("getTagValueSafe")]
        public static string GetTagValueSafe(Engine engine, string tagName, params string[] args)
        {
            SetNotFirstCall(engine);
            return TagManager.GetTag(tagName)?.Tag.Getter.Invoke(null, args)?.ToString() ?? "";
        }
        [Api(RequireTypes = new[] { typeof(KeyCode) })]
        public class On
        {
            [Api("rewind", Comment = new[]
            {
                "On ADOFAI Rewind (Level Start, Scene Moved, etc..)"
            })]
            public static void Rewind(Engine engine, JsValue func)
            {
                SetNotFirstCall(engine);
                Postfix(engine, "scrController:Awake_Rewind", func);
            }
            [Api("hit", Comment = new[]
            {
                "On Tile Hit"
            })]
            public static void Hit(Engine engine, JsValue func)
            {
                SetNotFirstCall(engine);
                Postfix(engine, "scrController:Hit", func);
            }
            [Api("dead", Comment = new[]
             {
                "On Dead"
            })]
            public static void Dead(Engine engine, JsValue func)
            {
                SetNotFirstCall(engine);
                if (!(func is FunctionInstance fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:FailAction"), new Action<scrController>(__instance =>
                {
                    if (!__instance.noFail) wrapper.Call();
                }));
            }
            [Api("fail", Comment = new[]
             {
                "On Fail"
            })]
            public static void Fail(Engine engine, JsValue func)
            {
                SetNotFirstCall(engine);
                if (!(func is FunctionInstance fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:FailAction"), new Action<scrController>(__instance =>
                {
                    wrapper.Call();
                }));
            }
            [Api("clear", Comment = new[]
             {
                "On Clear"
            })]
            public static void Clear(Engine engine, JsValue func)
            {
                SetNotFirstCall(engine);
                if (!(func is FunctionInstance fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:OnLandOnPortal"), new Action<scrController>(__instance =>
                {
                    if (__instance.gameworld) wrapper.Call();
                }));
            }
            #region KeyEvents
            [Api("anyKey", Comment = new[]
            {
                "On Any Key Pressed"
            })]
            public static void AnyKey(Engine engine, JsValue func)
            {
                SetNotFirstCall(engine);
                if (!(func is FunctionInstance fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:Update"), new Action(() =>
                {
                    if (Input.anyKey) wrapper.Call();
                }));
            }
            [Api("anyKeyDown", Comment = new[]
            {
                "On Any Key Down"
            })]
            public static void AnyKeyDown(Engine engine, JsValue func)
            {
                SetNotFirstCall(engine);
                if (!(func is FunctionInstance fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:Update"), new Action(() =>
                {
                    if (Input.anyKeyDown) wrapper.Call();
                }));
            }
            [Api("key", Comment = new[]
            {
                "On Key Pressed"
            })]
            public static void Key(Engine engine, KeyCode key, JsValue func)
            {
                SetNotFirstCall(engine);
                if (!(func is FunctionInstance fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:Update"), new Action(() =>
                {
                    if (Input.GetKey(key)) wrapper.Call();
                }));
            }
            [Api("keyUp", Comment = new[]
            {
                "On Key Up"
            })]
            public static void KeyUp(Engine engine, KeyCode key, JsValue func)
            {
                SetNotFirstCall(engine);
                if (!(func is FunctionInstance fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:Update"), new Action(() =>
                {
                    if (Input.GetKeyUp(key)) wrapper.Call();
                }));
            }
            [Api("keyDown", Comment = new[]
            {
                "On Key Down"
            })]
            public static void KeyDown(Engine engine, KeyCode key, JsValue func)
            {
                SetNotFirstCall(engine);
                if (!(func is FunctionInstance fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:Update"), new Action(() =>
                {
                    if (Input.GetKeyDown(key)) wrapper.Call();
                }));
            }
            #endregion
        }
        #endregion
        public static void EnsureSafety(Engine engine, string caller)
        {
            if (!engine.GetContext<Context>().ignoreSafety)
                throw new NotSafeScriptException($"Safety Error! Cannot Call '{caller}' Function Without Ignore Safety!");
        }
        public static void SetNotFirstCall(Engine engine) => engine.GetContext<Context>().isFirstCall = false;
        static Harmony harmony;
        public static HashSet<string> alreadyExecutedScripts;
        public static List<string> registeredCustomTags;
        public static Dictionary<string, object> globalVariables;
        static Dictionary<Engine, Dictionary<string, TypeReference>> jsTypes;
        [Obsolete("Internal Only!", true)]
        public static object[] StrArrayToObjArray(string[] arr)
        {
            object[] newArr = new object[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                newArr[i] = arr[i];
            return newArr;
        }
        static int uniqueId = 0;
        static bool wrapperInitialized = false;
        static MethodInfo satoa = typeof(Impl).GetMethod(nameof(StrArrayToObjArray), (BindingFlags)15420);
        static MethodInfo call_fi = typeof(FIWrapper).GetMethod("Call");
        static MethodInfo call_udfi = typeof(UDFWrapper).GetMethod("Call");
        static MethodInfo transpilerAdapter = typeof(JSUtils).GetMethod("TranspilerAdapter", AccessTools.all);
        internal static List<(ApiAttribute, MethodInfo)> apiMethods;
        static AssemblyBuilder ApiAssembly;
        static System.Reflection.Emit.ModuleBuilder ApiModule;
        static MethodInfo GenerateTagWrapper(FIWrapper wrapper)
        {
            Type[] paramTypes = wrapper.args.Select(t => typeof(string)).ToArray();
            var name = wrapper.fi.ToString().Replace("function ", string.Empty).Replace("() { [native code] }", string.Empty);
            if (string.IsNullOrWhiteSpace(name)) name = "Anonymous";
            TypeBuilder wrapperType = ApiModule.DefineType($"{name}_WrapperType${uniqueId++}", TypeAttributes.Public);
            MethodBuilder wrapperMethod = wrapperType.DefineMethod($"{name}_WrapperMethod", MethodAttributes.Public | MethodAttributes.Static, typeof(object), paramTypes);
            FieldBuilder wrapperField = wrapperType.DefineField("wrapper", typeof(FIWrapper), FieldAttributes.Public | FieldAttributes.Static);
            for (int i = 0; i < wrapper.args.Length; i++)
                wrapperMethod.DefineParameter(i + 1, ParameterAttributes.None, wrapper.args[i]);
            ILGenerator il = wrapperMethod.GetILGenerator();
            if (paramTypes.Length > 0)
            {
                LocalBuilder strArray = il.MakeArray<string>(paramTypes.Length);
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc, strArray);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, i);
                    il.Emit(OpCodes.Stelem_Ref);
                }
                il.Emit(OpCodes.Ldsfld, wrapperField);
                il.Emit(OpCodes.Ldloc, strArray);
                il.Emit(OpCodes.Call, satoa);
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, wrapperField);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Newarr, typeof(object));
            }
            il.Emit(OpCodes.Call, call_fi);
            il.Emit(OpCodes.Ret);
            var resultT = wrapperType.CreateType();
            resultT.GetField("wrapper").SetValue(null, wrapper);
            return resultT.GetMethod($"{name}_WrapperMethod");
        }
        public static MethodInfo WrapTranspiler(this FunctionInstance func)
        {
            if (func == null) return null;
            FIWrapper holder = new FIWrapper(func);

            TypeBuilder type = EmitUtils.NewType();
            MethodBuilder methodB = type.DefineMethod("Wrapper_Transpiler", MethodAttributes.Public | MethodAttributes.Static, typeof(IEnumerable<CodeInstruction>),
                new[] { typeof(IEnumerable<CodeInstruction>), typeof(MethodBase), typeof(ILGenerator) });
            FieldBuilder holderfld = type.DefineField("holder", typeof(FIWrapper), FieldAttributes.Public | FieldAttributes.Static);

            var il = methodB.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldsfld, holderfld);
            il.Emit(OpCodes.Call, transpilerAdapter);
            il.Emit(OpCodes.Ret);

            Type t = type.CreateType();
            t.GetField("holder").SetValue(null, holder);
            return t.GetMethod("Wrapper_Transpiler");
        }
        [Obsolete("Internal Only!", true)]
        public static IEnumerable<CodeInstruction> TranspilerAdapter(IEnumerable<CodeInstruction> instructions, MethodBase original, ILGenerator il, FIWrapper func)
        {
            object[] args = new object[func.args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var argName = func.args[i];
                if (argName.StartsWith("il"))
                    args[i] = il;
                else if (argName.StartsWith("o") ||
                    argName.StartsWith("m"))
                    args[i] = original;
                else if (argName.StartsWith("ins"))
                    args[i] = instructions.ToArray();
                else args[i] = JsValue.Undefined;
            }
            var result = func.CallRaw(args);
            if (JSUtils.IsNull(result)) return Enumerable.Empty<CodeInstruction>();
            else return result.AsArray().Select(v => (CodeInstruction)v.ToObject());
        }
        public static void InitializeWrapperAssembly()
        {
            if (wrapperInitialized) return;
            uniqueId = 0;
            ApiAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Overlayer.Scripting.ImplAss"), AssemblyBuilderAccess.RunAndCollect);
            ApiModule = ApiAssembly.DefineDynamicModule("Overlayer.Scripting.ImplAss");
            wrapperInitialized = true;
        }
        public static void DisposeWrapperAssembly()
        {
            ApiAssembly = null;
            ApiModule = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
            wrapperInitialized = false;
        }
        private static MethodInfo Postfix<T>(this Harmony harmony, MethodBase target, T del) where T : Delegate
           => harmony.Patch(target, postfix: new HarmonyMethod(del.Wrap()));
        private static MethodInfo Prefix<T>(this Harmony harmony, MethodBase target, T del) where T : Delegate
            => harmony.Patch(target, new HarmonyMethod(del.Wrap()));
    }
}
