using DG.Tweening;
using Discord;
using HarmonyLib;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime.Interop;
using Jint.Runtime.Interop.Attributes;
using JSNet.API;
using JSNet.Utils;
using JSON;
using Overlayer.Core;
using Overlayer.Core.Patches;
using Overlayer.Core.TextReplacing;
using Overlayer.Models;
using Overlayer.Tags;
using Overlayer.Tags.Attributes;
using Overlayer.Unity;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TMPro;
using UnityEngine;

namespace Overlayer.Scripting
{
    public static class Impl
    {
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
            registeredCustomTags?.ForEach(TagManager.RemoveTag);
            registeredCustomTags = null;
            globalVariables = null;
            harmony?.UnpatchAll(harmony.Id);
            harmony = null;
            jsTypes = null;
            Adofai.autoTextInjected = false;
            Adofai.startRadiusInjected = false;
            alreadyExecutedScripts = null;
            StaticCoroutine.Queue(StaticCoroutine.SyncRunner(TextManager.Refresh));
            DisposeWrapperAssembly();
        }
        public static void Reload()
        {
            Release();
            Initialize();
        }
        #region Impl APIs
        [Api("use")]
        public static void Use(Engine engine, params string[] tagsOrProxies)
        {
            string currentScript = Path.GetFileName(Main.CurrentExecutingScriptPath);
            for (int i = 0; i < tagsOrProxies.Length; i++)
            {
                var tagOrProxy = tagsOrProxies[i];
                if (TagManager.GetTag(tagOrProxy) != null)
                {
                    LazyPatchManager.PatchAll(tagOrProxy).ForEach(lp => lp.Locked = true);
                    engine.SetValue(tagOrProxy, TagManager.GetTag(tagOrProxy).Tag.GetterOriginal);
                    Main.Logger.Log($"[{currentScript}] Using '{tagOrProxy}' Tag.");
                }
                else
                {
                    bool isUri = false;
                    if (!isUri)
                    {
                        if (!tagOrProxy.EndsWith(".js"))
                            tagOrProxy += ".js";
                        if (!File.Exists(tagOrProxy))
                            tagOrProxy = Path.Combine(Main.ScriptPath, tagOrProxy);
                        if (!File.Exists(tagOrProxy))
                            throw new FileNotFoundException(tagsOrProxies[i]);
                    }
                    var name = Path.GetFileName(tagOrProxy);
                    var isProxy = false;
                    var time = MiscUtils.MeasureTime(() =>
                    {
                        var code = File.ReadAllText(tagOrProxy);
                        if (isProxy = code.StartsWith("// [Overlayer.Scripting JS Wrapper]"))
                        {
                            foreach (var (alias, member) in Main.ImportJSProxy(code))
                            {
                                if (member is Type t)
                                    engine.SetValue(alias, TypeReference.CreateTypeReference(engine, t));
                                else if (member is MethodInfo m)
                                    engine.SetValue(alias, m);
                            }
                        }
                        else
                        {
                            engine.Execute(JSUtils.RemoveImports(code));
                            alreadyExecutedScripts.Add(tagOrProxy);
                        }
                    });
                    if (isProxy)
                        Main.Logger.Log($"[{currentScript}] Using '{Path.GetFileName(tagOrProxy)}' Proxy. ({time.TotalMilliseconds}ms)");
                    else Main.Logger.Log($"Force Executed \"{name}\" Script Successfully. ({time.TotalMilliseconds}ms)");
                }
            }
        }
        [Api("exportTexts")]
        public static void ExportTexts(string path, OverlayerText[] texts)
        {
            File.WriteAllBytes(path, Main.ExportTexts(texts));
        }
        [Api("importTexts")]
        public static OverlayerText[] ImportTexts(byte[] rawTexts)
        {
            return Main.ImportTexts(rawTexts).ToArray();
        }
        [Api("deleteThis")]
        public static void DeleteThis(Engine engine)
        {
            File.Delete(Main.CurrentExecutingScriptPath);
        }
        [Api("generateProxy")]
        public static void GenerateProxy(string fileName, Type[] types, MethodInfo[] methods)
        {
            var generated = Main.GenerateJSProxy(
                proxyTypes: types?.Select(t => (t.Name, t)),
                proxyStaticMethods: methods?.Select(m => (m.Name, m)));
            File.WriteAllText(fileName, generated);
        }
        [Api("generateProxyWithAlias")]
        public static void GenerateProxyWithAlias(string fileName, Type[] types, string[] typeAliases, MethodInfo[] methods, string[] methodAliases)
        {
            List<(string, Type)> tt = new List<(string, Type)>();
            List<(string, MethodInfo)> mm = new List<(string, MethodInfo)>();
            for (int i = 0; i < types.Length; i++)
                tt.Add((typeAliases[i], types[i]));
            for (int i = 0; i < methods.Length; i++)
                mm.Add((methodAliases[i], methods[i]));
            var generated = Main.GenerateJSProxy(
                proxyTypes: tt,
                proxyStaticMethods: mm);
            File.WriteAllText(fileName, generated);
        }
        [RawReturn]
        [Api("resolveClrType")]
        public static Type ResolveType(Engine engine, string clrType)
        {
            if (!Main.allowUnsafe)
            {
                Main.Logger.Log("<color=red>Script uses unsafe API. to enable it, create a file named 'allowUnsafe.txt' in the mod folder.</color>");
                return null;
            }
            return MiscUtils.TypeByName(clrType);
        }
        [RawReturn]
        [Api("resolveClrMethod")]
        public static MethodInfo ResolveMethod(Engine engine, string clrType, string name)
        {
            if (!Main.allowUnsafe)
            {
                Main.Logger.Log("<color=red>Script uses unsafe API. to enable it, create a file named 'allowUnsafe.txt' in the mod folder.</color>");
                return null;
            }
            return MiscUtils.TypeByName(clrType)?.GetMethod(name, (BindingFlags)15420);
        }
        
        [Api("resolve")]
        public static TypeReference Resolve(Engine engine, string clrType)
        {
            if (!Main.allowUnsafe)
            {
                Main.Logger.Log("<color=red>Script uses unsafe API. to enable it, create a file named 'allowUnsafe.txt' in the mod folder.</color>");
                return null;
            }
            if (jsTypes.TryGetValue(engine, out var dict))
                if (dict.TryGetValue(clrType, out var t))
                    return t;
                else return dict[clrType] = TypeReference.CreateTypeReference(engine, MiscUtils.TypeByName(clrType));
            dict = jsTypes[engine] = new Dictionary<string, TypeReference>();
            return dict[clrType] = TypeReference.CreateTypeReference(engine, MiscUtils.TypeByName(clrType));
        }
        [Api("getAttr")]
        public static object GetAttr(object obj, string accessor = "")
        {
            if (!Main.allowUnsafe)
            {
                Main.Logger.Log("<color=red>Script uses unsafe API. to enable it, create a file named 'allowUnsafe.txt' in the mod folder.</color>");
                return null;
            }
            return OverlayerTag.RuntimeAccess(obj, accessor);
        }
        [Api("setAttr")]
        public static bool SetAttr(object obj, string accessor = "",  object value = null)
        {
            if (!Main.allowUnsafe)
            {
                Main.Logger.Log("<color=red>Script uses unsafe API. to enable it, create a file named 'allowUnsafe.txt' in the mod folder.</color>");
                return false;
            }
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
        [Api("wrapToJSObject")]
        public static JsValue WrapToJSObject(Engine engine, object obj) => JsValue.FromObject(engine, obj);
        [Api("unwrapFromJSObject")]
        public static object UnwrapFromJSObject(JsValue value) => value.ToObject();
        [Api("getScriptPath")]
        public static string GetScriptPath(string extra = "") => Path.Combine(Main.ScriptPath, extra);
        [Api("getClrGenericTypeName")]
        public static string GetGenericClrTypeString(Engine engine, string genericType, string[] genericArgs)
        {
            if (!Main.allowUnsafe)
            {
                Main.Logger.Log("<color=red>Script uses unsafe API. to enable it, create a file named 'allowUnsafe.txt' in the mod folder.</color>");
                return null;
            }
            
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
            return globalVariables.TryGetValue(name, out var value) ? value : null;
        }
        public delegate object CallWrapper(params object[] args);
        [Api("setGlobalVariable")]
        public static object SetGlobalVariable(Engine engine, string name, object obj)  
        {
            if (obj is Function fi)
            {
                FIWrapper wrapper = new FIWrapper(fi);
                obj = (CallWrapper)wrapper.Call;
            }
            return globalVariables[name] = obj;
        }
        [Api("registerTag")]
        public static void RegisterTag(Engine engine, string name, JsValue func, bool notplaying, string tooltip)
        {
            if (!(func is Function fi)) return;
            FIWrapper wrapper = new FIWrapper(fi);
            var tagWrapper = GenerateTagWrapper(wrapper);
            var tuple = (new ApiAttribute(name), tagWrapper);
            Main.JSApi.Methods.Add(tuple);
            Expression.expressions.Clear();
            string pathOrScript = Main.CurrentExecutingScriptPath == "Sandbox.js" ? Main.CurrentExecutingScript : Main.CurrentExecutingScriptPath;
            TagManager.SetTag(new ScriptTag(pathOrScript, tagWrapper, new Tags.Attributes.TagAttribute(name) { NotPlaying = notplaying }));
            StaticCoroutine.Queue(StaticCoroutine.SyncRunner(TextManager.Refresh));
            registeredCustomTags.Add(name);
            if (tooltip != null)
            {
                Tooltip.tooltip[name] = tooltip;
            }
            Main.Logger.Log($"Registered Tag \"{name}\" (NotPlaying:{notplaying})");
        }
        [Api("unregisterTag")]
        public static void UnregisterTag(Engine engine, string name)
        {
            Main.JSApi.Methods.RemoveAll(t => t.Item1.Name == name);
            Expression.expressions.Clear();
            TagManager.RemoveTag(name);
            Tooltip.tooltip.Remove(name);
            StaticCoroutine.Queue(StaticCoroutine.SyncRunner(TextManager.Refresh));
        }
        /*[Api("prefix")]
        public static bool Prefix(Engine engine, string typeColonMethodName, JsValue patch)
        {
            if (!(patch is Function func)) return false;
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
            if (!(patch is Function func)) return false;
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
            if (!(patch is Function func)) return false;
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
            if (!(patch is Function func)) return false;
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
            if (!(patch is Function func)) return false;
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
            if (!(patch is Function func)) return false;
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
        }*/
        [Api("isNoFailMode")]
        public static bool IsNoFailMode(Engine engine)
        {
            return scrController.instance?.noFail ?? false;
        }
        [Api("getLanguage", RequireTypes = new[] { typeof(SystemLanguage) })]
        public static SystemLanguage GetLanguage(Engine engine)
        {
            return RDString.language;
        }
        [Api("isAutoEnabled")]
        public static bool IsAutoEnabled(Engine engine)
        {
            return RDC.auto;
        }
        [Api("isWeakAutoEnabled")]
        public static bool IsWeakAutoEnabled(Engine engine)
        {
            return RDC.useOldAuto;
        }
        [Api("ease", RequireTypes = new Type[] { typeof(Ease) })]
        public static float EasedValue(Engine engine, Ease ease, float lifetime)
        {
            return DOVirtual.EasedValue(0, 1, lifetime, ease);
        }
        [Api("easeColor", RequireTypes = new Type[] { typeof(Color) })]
        public static Color EasedColor(Engine engine, Color color, Ease ease, float lifetime)
        {
            return color * DOVirtual.EasedValue(0, 1, lifetime, ease);
        }
        [Api("easeColorFromTo")]
        public static Color EasedColor(Engine engine, Color from, Color to, Ease ease, float lifetime)
        {
            return from + ((to - from) * DOVirtual.EasedValue(0, 1, lifetime, ease));
        }
        [Api("colorFromHexRGB")]
        public static Color FromHexRGB(Engine engine, string rgbHex)
        {
            return ColorUtility.TryParseHtmlString('#' + rgbHex, out var color) ? color : Color.clear;
        }
        [Api("colorFromHexRGBA")]
        public static Color FromHexRGBA(Engine engine, string rgbaHex)
        {
            return ColorUtility.TryParseHtmlString('#' + rgbaHex, out var color) ? color : Color.clear;
        }
        [Api("rgbToHSV")]
        public static float[] RgbToHSV(Color color)
        {
            float[] values = new float[3];
            Color.RGBToHSV(color, out values[0], out values[1], out values[2]);
            return values;
        }
        [Api("colorToHexRGB")]
        public static string ToHexRGB(Engine engine, Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
        [Api("colorToHexRGBA")]
        public static string ToHexRGBA(Engine engine, Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
        [Api("getTagValueSafe")]
        public static string GetTagValueSafe(Engine engine, string tagName, params string[] args)
        {
            return TagManager.GetTag(tagName)?.Tag.Getter.Invoke(null, args)?.ToString() ?? "";
        }
        [Api("parseFastInt")]
        public static int ParseFastInt(string str) => StringConverter.ToInt32(str);
        [Api("parseFastFloat")]
        public static double ParseFastFloat(string str) => StringConverter.ToDouble(str);
        [Api("getText", RequireTypes = new Type[]
        {
            typeof(OverlayerText),
            typeof(Replacer),
            typeof(TextConfig),
            typeof(GColor),
            typeof(TMPro.TextAlignmentOptions),
        })]
        public static OverlayerText GetText(int index)
        {
            if (index < 0 || index >= TextManager.Count) 
                return null;
            return TextManager.Get(index);
        }
        [Api("getTextByName")]
        public static OverlayerText GetTextByName(string name)
        {
            for (int i = 0; i < TextManager.Count; i++)
            {
                var text = TextManager.Get(i);
                if (text.Config.Name == name) return text;
            }
            return null;
        }
        [Api("createText")]
        public static OverlayerText CreateText()
        {
            return TextManager.CreateText(new TextConfig());
        }
        [Api("createTextFromJson")]
        public static OverlayerText CreateTextFromJson(string json)
        {
            return TextManager.CreateText(TextConfigImporter.Import(JsonNode.Parse(json)));
        }
        [Api("createTexture", RequireTypes = new[] { typeof(Texture2D) })]
        public static Texture2D CreateTexture(string imagePath)
        {
            if (!File.Exists(imagePath)) return null;
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(imagePath));
            return texture;
        }
        [Api("createTextureRaw")]
        public static Texture2D CreateTextureRaw(byte[] raw)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(raw);
            return texture;
        }
        [Api("createSprite")]
        public static Sprite CreateSprite(Texture2D texture)
        {
            if (!spriteCache.TryGetValue(texture, out Sprite sprite))
                sprite = spriteCache[texture] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        [Api("playSound")]
        public static void PlaySound(string path)
        {
            var sound = new Sound();
            sound.sound = path;
            AudioPlayer.Play(sound);
        }
        [Api("loadAudio", Comment = new string[]
        {
            "Load Audio(UnityEngine.AudioClip) With Callback (.mp3, .ogg, .aiff, .wav)"
        }, RequireTypes = new Type[] { typeof(AudioClip) })]
        public static void LoadAudio(string path, JsValue callback)
        {
            if (!(callback is Function func)) return;
            FIWrapper fi = new FIWrapper(func);
            AudioPlayer.LoadAudio(path, ac => fi.Call(ac));
        }
        [Api("setAudio", Comment = new string[]
        {
            "Set Audio(UnityEngine.AudioClip) With Callback (.mp3, .ogg, .aiff, .wav)"
        }, RequireTypes = new Type[] { typeof(AudioSource) })]
        public static void SetAudio(string path, AudioSource source) => AudioPlayer.LoadAudio(path, clip => source.clip = clip);
        [Api("httpGet")]
        public static string HttpGet(string url) => Overlayer.Main.HttpClient.GetStringAsync(url).GetAwaiter().GetResult();
        [Api(RequireTypes = new[] { typeof(KeyCode) })]
        public class On
        {
            [Api("rewind", Comment = new[]
            {
                "On ADOFAI Rewind (Level Start, Scene Moved, etc..)"
            })]
            public static void Rewind(Engine engine, JsValue func)
            {
                if (!(func is Function fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:Awake_Rewind"), new Action(() => wrapper.Call()));
            }
            [Api("hit", Comment = new[]
            {
                "On Tile Hit"
            })]
            public static void Hit(Engine engine, JsValue func)
            {
                if (!(func is Function fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:Hit"), new Action(() => wrapper.Call()));
            }
            [Api("dead", Comment = new[]
             {
                "On Dead"
            })]
            public static void Dead(Engine engine, JsValue func)
            {
                if (!(func is Function fi)) return;
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
                if (!(func is Function fi)) return;
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
                if (!(func is Function fi)) return;
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
                if (!(func is Function fi)) return;
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
                if (!(func is Function fi)) return;
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
                if (!(func is Function fi)) return;
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
                if (!(func is Function fi)) return;
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
                if (!(func is Function fi)) return;
                FIWrapper wrapper = new FIWrapper(fi);
                harmony.Postfix(MiscUtils.MethodByName("scrController:Update"), new Action(() =>
                {
                    if (Input.GetKeyDown(key)) wrapper.Call();
                }));
            }
            #endregion
        }
        [Api(Comment = new string[]
        {
            "These Methods Are Recommended To Use In 'On.rewind' Callback."
        },
        RequireTypes = new Type[]
        {
            typeof(SpriteRenderer),
            typeof(scrHitTextMesh),
            typeof(HitMargin),
            typeof(SfxSound),
            typeof(HitSound)
        })]
        public class Adofai
        {
            [Api("getPlanetRenderer", ReturnComment = "UnityEngine.SpriteRenderer (Planet SpriteRenderer)")]
            public static SpriteRenderer GetPlanetRenderer(scrPlanet planet)
            {
                return planet.GetOrAddRenderer();
            }
            [Api("scalePlanet")]
            public static void ScalePlanet(scrPlanet planet, Vector2 vec)
            {
                ScaleAll(new[]
                {
                    (mr.GetValue(planet.sprite) as SpriteRenderer)?.transform,
                    planet.coreParticles?.transform,
                    planet.tailParticles?.transform,
                    planet.sparks?.transform,
                    planet.ring?.transform,
                    planet.glow?.transform,
                    planet.deathExplosion?.transform,
                    planet.faceSprite?.transform,
                    planet.faceDetails?.transform,
                    planet.faceHolder?.transform,
                    planet.samuraiSprite?.transform,
                }, vec);
            }
            [Api("setDiscordRp")]
            public static void SetDiscordRp(string title, string state, string details)
            {
                string Validate(string s) => s.Length <= 60 ? s : s.Substring(0, 57) + "...";
                Discord.Discord discord = typeof(DiscordController).GetField("discord", (BindingFlags)15420).GetValue(DiscordController.instance) as Discord.Discord;
                Activity activity = default;
                activity.State = Validate(state);
                activity.Details = Validate(details);
                activity.Assets.LargeImage = "planets_icon_stars";
                activity.Assets.LargeText = title;
                discord.GetActivityManager().UpdateActivity(activity, delegate { });
            }
            [Api("setBuildText")]
            public static void SetBuildText(string text)
            {
                var betaText = UnityEngine.Object.FindObjectOfType<scrEnableIfBeta>();
                betaText.gameObject.SetActive(true);
                betaText.GetComponent<TMP_Text>().text = text;
            }
            [Api("setAutoText")]
            public static void SetAutoText(string text)
            {
                InjectAutoTextUpdate();
                var betaText = UnityEngine.Object.FindObjectOfType<scrShowIfDebug>();
                betaText.gameObject.SetActive(true);
                betaText.GetComponent<UnityEngine.UI.Text>().text = text;
            }
            [Api("configAutoText", ParamComment = new string[]
            {
                "UnityEngine.UI.Text Callback"
            })]
            public static void ConfigAutoText(Engine engine, JsValue configFunc)
            {
                if (configFunc is not Function func) return;
                FIWrapper wrapper = new FIWrapper(func);
                InjectAutoTextUpdate();
                var betaText = UnityEngine.Object.FindObjectOfType<scrShowIfDebug>();
                betaText.gameObject.SetActive(true);
                wrapper.Call(betaText.GetComponent<UnityEngine.UI.Text>());
            }
            [Api("setTileSprite")]
            public static void SetTileSprite(Sprite sprite, float scale)
            {
                foreach (var floor in UnityEngine.Object.FindObjectsOfType<scrFloor>())
                {
                    floor.floorRenderer.sprite = sprite;
                    floor.floorRenderer.transform.localScale = new Vector2(scale, scale);
                }
            }
            [Api("setTileIcon")]
            public static void SetTileIcon(Sprite sprite, float scale)
            {
                foreach (var floor in UnityEngine.Object.FindObjectsOfType<scrFloor>())
                {
                    floor.SetIconSprite(sprite);
                    floor.SetIconScale(scale);
                }
            }
            [Api("configTiles")]
            public static void ConfigTiles(Engine engine, JsValue configFunc)
            {
                if (configFunc is not Function func) return;
                FIWrapper wrapper = new FIWrapper(func);
                var list = scrLevelMaker.instance.listFloors;
                for (int i = 0; i < list.Count; i++)
                    wrapper.Call(wrapper.args.Length == 1 ? new object[] { list[i] } : new object[] { i, list[i] });
            }
            [Api("setJudgeText")]
            public static void SetJudgeText(HitMargin hitMargin, string text)
            {
                StaticCoroutine.Run(StaticCoroutine.SyncRunner(() =>
                {
                    if (cachedHitTexts(Tags.ADOFAI.Controller) == null) return;
                    foreach (var t in cachedHitTexts(Tags.ADOFAI.Controller)[hitMargin])
                        sHTM_text(t).text = text;
                }));
            }
            [Api("configJudgeText", ParamComment = new string[]
            {
                "scrHitTextMesh Callback"
            })]
            public static void ConfigJudgeText(Engine engine, HitMargin hitMargin, JsValue configFunc)
            {
                if (configFunc is not Function func) return;
                FIWrapper wrapper = new FIWrapper(func);
                StaticCoroutine.Run(StaticCoroutine.SyncRunner(() =>
                {
                    if (cachedHitTexts(Tags.ADOFAI.Controller) == null) return;
                    foreach (var t in cachedHitTexts(Tags.ADOFAI.Controller)[hitMargin])
                        wrapper.Call(func);
                }));
            }
            [Api("setSfxSound")]
            public static void SetSfxSound(SfxSound sfx, string audio) => AudioPlayer.LoadAudio(audio, clip => Tags.ADOFAI.RDC.soundEffects[(int)sfx] = clip);
            [Api("setHitSound")]
            public static void SetHitSound(HitSound hit, string audio) => AudioPlayer.LoadAudio(audio, clip => AudioManager.Instance.audioLib[$"snd{hit}"] = clip);
            [Api("setLobbyBgm")]
            public static void SetLobbyBgm(string audio) => AudioPlayer.LoadAudio(audio, clip =>
            {
                var lobbySource = scrConductor.instance.GetComponentsInChildren<AudioSource>()?.FirstOrDefault(a => a.clip?.name == "1-X-wav");
                if (lobbySource != null) lobbySource.clip = clip;
            });
            [Api("setStartRadius")]
            public static void SetStartRadius(float radius)
            {
                InjectStartRadius();
                startRadius = radius;
            }
            [Api("setOldAuto")]
            public static void SetWeakAuto(bool enabled)
            {
                RDC.useOldAuto = enabled;
            }
            [Api("getAngleFromFloor")]
            public static double GetAngleFromFloor(scrFloor floor) => Math.Abs(floor.entryangle - floor.exitangle) * Mathf.Rad2Deg;
            internal static AccessTools.FieldRef<scrController, Dictionary<HitMargin, scrHitTextMesh[]>> cachedHitTexts = AccessTools.FieldRefAccess<scrController, Dictionary<HitMargin, scrHitTextMesh[]>>("cachedHitTexts");
            internal static AccessTools.FieldRef<scrHitTextMesh, TextMesh> sHTM_text = AccessTools.FieldRefAccess<scrHitTextMesh, TextMesh>("text");
            private static float startRadius = 1;
            internal static bool autoTextInjected = false;
            internal static bool startRadiusInjected = false;
            private static void InjectAutoTextUpdate()
            {
                if (autoTextInjected) return;
                harmony.Patch(typeof(scrShowIfDebug).GetMethod("Update", (BindingFlags)15420), new HarmonyMethod(EmitUtils.Wrap(new Func<bool>(() => false))));
                autoTextInjected = true;
            }
            private static void InjectStartRadius()
            {
                if (startRadiusInjected) return;
                harmony.Patch(typeof(scrController).GetMethod("get_startRadius", (BindingFlags)15420), new HarmonyMethod(EmitUtils.Wrap(new RefFunc<float, bool>((ref float __result) =>
                {
                    __result = startRadius;
                    return false;
                }))));
                startRadiusInjected = true;
            }
            private static void ScaleAll(Transform[] t, Vector2 vec)
            {
                for (int i = 0; i < t.Length; i++)
                    if (t[i] != null)
                        t[i].localScale = vec;
            }
            public delegate R RefFunc<T, R>(ref T val);
        }
        #endregion
        static Harmony harmony;
        public static HashSet<string> alreadyExecutedScripts;
        public static List<string> registeredCustomTags;
        public static Dictionary<string, object> globalVariables;
        public static Dictionary<Texture2D, Sprite> spriteCache = new Dictionary<Texture2D, Sprite>();
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
        static MethodInfo transpilerAdapter = typeof(Impl).GetMethod("TranspilerAdapter", AccessTools.all);
        static AssemblyBuilder ApiAssembly;
        static ModuleBuilder ApiModule;
        static FieldInfo mr = typeof(PlanetRenderer).GetField("meshRenderer", (BindingFlags)15420);
        // From PlanetTweaks By tjwogud
        public static SpriteRenderer GetOrAddRenderer(this scrPlanet planet)
        {
            if (!planet) return null;
            SpriteRenderer renderer = planet.transform.Find("PlanetTweaksRenderer")?.GetComponent<SpriteRenderer>();
            if (!renderer)
            {
                GameObject obj = new GameObject("PlanetTweaksRenderer");
                obj.AddComponent<RendererController>();
                renderer = obj.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = (mr.GetValue(planet.sprite) as SpriteRenderer).sortingOrder + 1;
                renderer.sortingLayerID = planet.faceDetails.sortingLayerID;
                renderer.sortingLayerName = planet.faceDetails.sortingLayerName;
                renderer.transform.SetParent(planet.transform);
                renderer.transform.position = planet.transform.position;
            }
            return renderer;
        }
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
        public static MethodInfo WrapTranspiler(this Function func)
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
        // From PlanetTweaks By tjwogud
        public class RendererController : MonoBehaviour
        {
            private scrPlanet planet;
            private SpriteRenderer renderer;

            private void Awake()
            {
                planet = GetComponentInParent<scrPlanet>();
                renderer = planet.GetOrAddRenderer();
            }

            private void Update()
            {
                if (!planet)
                    planet = GetComponentInParent<scrPlanet>();
                if (!renderer)
                    renderer = planet.GetOrAddRenderer();
                if (planet && renderer)
                {
                    if (planet.dummyPlanets)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    renderer.enabled = planet.sprite.visible;
                }
            }
        }
    }
}
