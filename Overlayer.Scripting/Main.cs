using HarmonyLib;
using JSNet.API;
using JSNet.Utils;
using JSON;
using Overlayer.Core;
using Overlayer.Core.Patches;
using Overlayer.Core.TextReplacing;
using Overlayer.Patches;
using Overlayer.Tags;
using Overlayer.Unity;
using Overlayer.Utils;
using SA.GoogleDoc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace Overlayer.Scripting
{
    public static class Main
    {
        public static string ScriptPath => Path.Combine(Mod.Path, "Scripts");
        public static string ScriptProxyPath => Path.Combine(ScriptPath, "Proxies");
        public static Assembly Assembly { get; private set; }
        public static bool ScriptsRunning { get; private set; }
        public static string CurrentExecutingScript { get; private set; }
        public static string CurrentExecutingScriptPath { get; private set; }
        public static ModEntry Mod { get; private set; }
        public static ModLogger Logger { get; private set; }
        public static Settings Settings { get; private set; }
        public static Api JSApi { get; private set; }
        public static bool PatchesLocked { get; private set; }

        public static bool allowUnsafe { get; private set; }
        public static void Load(ModEntry modEntry)
        {
            Mod = modEntry;
            Logger = modEntry.Logger;
            Assembly = Assembly.GetExecutingAssembly();
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
        }
        public static bool OnToggle(ModEntry modEntry, bool toggle)
        {
            PatchGuard.Ignore(() =>
            {
                if (toggle)
                {
                    if (File.Exists(Path.Combine(modEntry.Path, "allowUnsafe.txt")))
                    {
                        allowUnsafe = true;
                    }
                    Settings = ModSettings.Load<Settings>(modEntry);
                    TagManager.Load(typeof(Expression));
                    TagManager.Load(typeof(PerformanceTags));

                    PatchGuard.ForceIgnore();
                    JSApi = new Api();
                    JSApi.RegisterType(typeof(Impl));
                    foreach (var tag in TagManager.All)
                        JSApi.Methods.Add((new ApiAttribute(tag.Name), tag.Tag.GetterOriginal));

                    /*foreach (var att in GetADOFAITagTypes())
                    {
                        var tuple = (new ApiAttribute(att.Name), att);
                        JSApi.Types.Add(tuple);
                    }*/

                    OverlayerText.OnApplyConfig += text =>
                    {
                        if (!PatchesLocked && TagManager.HasReference(typeof(Expression)))
                        {
                            LazyPatchManager.PatchAll().ForEach(lp => lp.Locked = true);
                            PatchesLocked = true;
                        }
                    };

                    RunScriptsNonBlocking();
                    PerformanceTags.Initialize();
                }
                else
                {
                    PerformanceTags.Release();
                    TagManager.Unload(typeof(Expression));
                    TagManager.Unload(typeof(PerformanceTags));
                    Impl.Release();
                    JSApi = null;
                    ModSettings.Save(Settings, modEntry);
                }
                Expression.expressions.Clear();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
            });
            return true;
        }
        static string SandboxJSCode = string.Empty;
        static string SandboxResult = "Not Executed.";
        public static void OnGUI(ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            if (Drawer.Button("Reload Scripts"))
                RunScriptsNonBlocking();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("Test Code:");
            SandboxJSCode = GUILayout.TextArea(SandboxJSCode, Drawer.myTextField);
            GUILayout.BeginHorizontal();
            if (Drawer.Button("Execute"))
            {
                Exception e;
                MiscUtils.ExecuteSafe(() =>
                {
                    BeginScript(true);
                    MiscUtils.ExecuteSafe(() => JSApi.PrepareInterpreter().Evaluate(JSUtils.RemoveImports(SandboxJSCode)), out e);
                    SandboxResult = e?.ToString() ?? "Success";
                }, out e);
                if (e != null) SandboxResult = e.ToString();
                EndScript();
            }
            if (Drawer.Button("Evaluate"))
            {
                Exception e;
                MiscUtils.ExecuteSafe(() =>
                {
                    BeginScript(true);
                    var result = MiscUtils.ExecuteSafe(() => JSApi.PrepareInterpreter().Evaluate(JSUtils.RemoveImports(SandboxJSCode)), out e);
                    SandboxResult = e?.ToString() ?? result?.ToString() ?? "null";
                }, out e);
                if (e != null) SandboxResult = e.ToString();
                EndScript();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label($"Result:\n{SandboxResult}");
            Drawer.DrawInt32("Performance Status Update Rate", ref Settings.PerfStatUpdateRate);
        }
        public static void OnSaveGUI(ModEntry modEntry)
        {
            ModSettings.Save(Settings, modEntry);
        }
        public static async Task RunScripts()
        {
            if (ScriptsRunning)
            {
                Logger.Log("Scripts Already Running! Aborting..");
                return;
            }
            ScriptsRunning = true;
            Logger.Log("Start Running Scripts..");
            Directory.CreateDirectory(ScriptPath);
            Directory.CreateDirectory(ScriptProxyPath);
            Logger.Log("Generating Script Implementations..");
            File.WriteAllText(Path.Combine(ScriptPath, "Impl.js"), JSApi.Generate());
            Logger.Log("Generating Script System Implementations..");
            File.WriteAllText(Path.Combine(ScriptProxyPath, "System.js"), GenerateJSProxy("c3nb", systemTypes.Select(t => (t.Name == "File" ? "IOFile" : t.Name, t)), null, new Version(1, 0, 0)));
            Logger.Log("Generating Script System Implementations..");
            File.WriteAllText(Path.Combine(ScriptProxyPath, "Reflection.js"), GenerateJSProxy("c3nb", reflectionTypes.Select(t => (t.Name, t)), null, new Version(1, 0, 0)));
            Logger.Log("Generating Script Harmony Implementations..");
            File.WriteAllText(Path.Combine(ScriptProxyPath, "Harmony.js"), GenerateJSProxy("c3nb", harmonyTypes.Select(t => (t.Name, t)), null, new Version(1, 0, 0)));
            Logger.Log("Generating Script Unity Implementations..");
            File.WriteAllText(Path.Combine(ScriptProxyPath, "Unity.js"), GenerateJSProxy("c3nb", unityTypes.Select(t => (t.Name, t)), null, new Version(1, 0, 0)));
            Logger.Log("Preparing Executing Scripts..");
            Impl.Reload();
            foreach (string script in Directory.GetFiles(ScriptPath, "*.js"))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(script);
                if (nameWithoutExt == "Impl" ||
                    nameWithoutExt == "CImpl")
                    continue;
                if (nameWithoutExt.EndsWith("_Proxy")) continue;
                if (nameWithoutExt.EndsWith("_Compilable")) continue;
                if (Impl.alreadyExecutedScripts.Contains(script)) continue;
                await RunScript(script, File.ReadAllText(script));
            }
            ScriptsRunning = false;
        }
        public static async Task<bool> RunScript(string path, string script)
        {
            return await Task.Run(() =>
            {
                string name = Path.GetFileName(path);
                try
                {
                    CurrentExecutingScript = script;
                    CurrentExecutingScriptPath = path;
                    BeginScript();
                    var time = MiscUtils.MeasureTime(() => JSApi.PrepareInterpreter().Execute(JSUtils.RemoveImports(script)));
                    EndScript();
                    Logger.Log($"Executed \"{name}\" Script Successfully. ({time.TotalMilliseconds}ms)");
                    return true;
                }
                catch (Exception e) { Logger.Log($"Exception At Executing Script \"{name}\":\n{e}"); return false; }
            });
        }
        public static async void RunScriptsNonBlocking()
        {
            await RunScripts();
        }
        public static void BeginScript(bool sandbox = false)
        {
            if (sandbox)
            {
                CurrentExecutingScript = SandboxJSCode;
                CurrentExecutingScriptPath = "Sandbox.js";
            }
        }
        public static void EndScript()
        {
            CurrentExecutingScript = null;
            CurrentExecutingScriptPath = null;
        }
        public static IEnumerable<Type> GetADOFAITagTypes()
        {
            var adofaiTags = TagManager.All.Where(t => t.DeclaringType == typeof(Tags.ADOFAI));
            return adofaiTags.Select(t => t.Tag.GetterOriginal.ReturnType).Distinct();
        }
        public static string GenerateJSProxy(string author = null, IEnumerable<(string, Type)> proxyTypes = null, IEnumerable<(string, MethodInfo)> proxyStaticMethods = null, Version version = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"// [Overlayer.Scripting JS Wrapper]");
            if (author != null)
                sb.AppendLine($"// Author: {author}");
            if (proxyTypes != null)
                sb.AppendLine($"// ProxyTypes: {string.Join("*", proxyTypes.Select(t => $"{t.Item2.FullName}&{t.Item1}"))}");
            if (proxyStaticMethods != null)
                sb.AppendLine($"// ProxyMethods: {string.Join("*", proxyStaticMethods.Select(t => $"{t.Item2.DeclaringType}^{t.Item2.Name}#{string.Join(",", t.Item2.GetParameters().Select(p => p.ParameterType.FullName))}&{t.Item1}"))}");
            if (version != null)
                sb.AppendLine($"// Version: {version}");
            Api api = new Api();
            if (proxyTypes != null)
                api.Types.AddRange(proxyTypes.Select(t => (new ApiAttribute(t.Item1), t.Item2)));
            if (proxyStaticMethods != null)
                api.Methods.AddRange(proxyStaticMethods.Select(t => (new ApiAttribute(t.Item1), t.Item2)));
            sb.AppendLine(api.Generate());
            return sb.ToString();
        }
        public static IEnumerable<(string, MemberInfo)> ImportJSProxy(string jsWrapper)
        {
            using (StringReader sr = new StringReader(jsWrapper))
            {
                List<string> comments = new List<string>();
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!line.StartsWith("//")) break;
                    comments.Add(line.Substring(2).TrimStart());
                }

                var proxyTypes = comments.Find(s => s.StartsWith("ProxyTypes:"));
                if (proxyTypes != null)
                {
                    var split = proxyTypes.Split(':');
                    if (split.Length > 1)
                    {
                        var types = split[1].TrimStart();
                        foreach (var clrType in types.Split('*').Select(typeString =>
                        {
                            var typeNameSplit = typeString.Split('&');
                            return (typeNameSplit[1], (MemberInfo)MiscUtils.TypeByName(typeNameSplit[0]));
                        }))
                            yield return clrType;
                    }
                }
                

                var proxyMethods = comments.Find(s => s.StartsWith("ProxyMethods:"));
                if (proxyMethods != null)
                {
                    var split = proxyMethods.Split(':');
                    if (split.Length > 1) 
                    {
                        var methods = split[1].TrimStart();
                        foreach (var staticMethod in methods.Split('*').Where(s => s.Any()).Select(methodString =>
                        {
                            var decTypeSplit = methodString.Split('^');
                            var decType = MiscUtils.TypeByName(decTypeSplit[0]);
                            var nameSplit = decTypeSplit[1].Split('#');
                            var name = nameSplit[0];
                            var parametersSplit = nameSplit[1].Split('&');
                            var parameters = parametersSplit[0].Split(',').Select(pType => MiscUtils.TypeByName(pType));
                            var alias = parametersSplit[1];
                            return (alias, (MemberInfo)decType?.GetMethod(name, (BindingFlags)15420, null, parameters.ToArray(), null));
                        }))
                            yield return staticMethod;
                    }
                }
            }
        }
        public static byte[] ExportTexts(IEnumerable<OverlayerText> texts)
        {
            JsonNode node = JsonNode.Empty;
            node["Texts"] = ModelUtils.WrapList(texts.Select(ot => ot.Config).ToList());
            var scripts = node["Scripts"].AsArray;
            foreach (var script in texts.SelectMany(t => t.PlayingReplacer.References.Union(t.NotPlayingReplacer.References).Select(ResolveScriptTag).Where(t => t is not null))
                .Select(st =>
                {
                    var node = JsonNode.Empty;
                    node["Name"] = st.Path != null ? Path.GetFileName(st.Path) : $"{Guid.NewGuid()}.js";
                    node["Script"] = st.Path != null ? File.ReadAllText(st.Path) : st.Script;
                    return node;
                }))
                scripts.Add(script);
            return Encoding.UTF8.GetBytes(node.ToString()).Compress();
        }
        public static List<OverlayerText> ImportTexts(byte[] raw)
        {
            JsonNode node = JsonNode.Parse(Encoding.UTF8.GetString(raw.Decompress()));
            List<OverlayerText> texts = new List<OverlayerText>(node["Texts"].Values.Select(TextConfigImporter.Import).Select(TextManager.CreateText));
            foreach (var script in node["Scripts"].Values)
            {
                JSApi.PrepareInterpreter().Execute(script["Script"], script["Name"]);
                File.WriteAllText(Path.Combine(ScriptPath, script["Name"]), script["Script"]);
            }
            TextManager.Refresh();
            return texts;
        }
        private static ScriptTag ResolveScriptTag(Tag tag)
        {
            return TagManager.All.Where(ot => ot.Name == tag.Name).FirstOrDefault() as ScriptTag;
        }
        public static Type[] unityTypes = new Type[]
        {
            typeof(Scene),
            typeof(SceneManager),
            typeof(Sprite),
            typeof(SpriteRenderer),
            typeof(Image),
            typeof(Vector2),
            typeof(Vector2Int),
            typeof(Vector3),
            typeof(Vector3Int),
            typeof(Vector4),
            typeof(Rect),
            typeof(Quaternion),
            typeof(Transform),
            typeof(RectTransform),
            typeof(GUI),
            typeof(GUILayout),
            typeof(GUISkin),
            typeof(GUIStyle),
            typeof(Texture),
            typeof(Texture2D),
            typeof(GameObject),
            typeof(Component),
            typeof(Shader),
            typeof(Matrix4x4),
            typeof(Text),
            typeof(TextMesh),
            typeof(TextMeshPro),
            typeof(TextMeshProUGUI),
            typeof(Material),
            typeof(Canvas)
        };
        public static Type[] systemTypes = new Type[]
        {
            // Real System
            typeof(Type),
            typeof(Array),
            typeof(Enum),

            // IO
            typeof(File),
            typeof(Path),
            typeof(Directory),

            // Collection
            typeof(IEnumerable),
            typeof(IEnumerator),
        };
        public static Type[] reflectionTypes = new Type[]
        {
            // Reflection
            typeof(MemberInfo),
            typeof(MethodBase),
            typeof(ConstructorInfo),
            typeof(Assembly),
            typeof(FieldInfo),
            typeof(MethodInfo),
            typeof(PropertyInfo),
            typeof(EventInfo),
            typeof(ParameterInfo),
            typeof(BindingFlags),

            // Emit
            typeof(DynamicMethod),
            typeof(AssemblyBuilder),
            typeof(EnumBuilder),
            typeof(TypeBuilder),
            typeof(MethodBuilder),
            typeof(PropertyBuilder),
            typeof(FieldBuilder),
            typeof(EventBuilder),
            typeof(ParameterBuilder),
            typeof(ParameterAttributes),
            typeof(MethodAttributes),
            typeof(FieldAttributes),
            typeof(TypeAttributes),
            typeof(PropertyAttributes),
            typeof(EventAttributes),
            typeof(ILGenerator),
            typeof(OpCode),
            typeof(OpCodes),
        };
        public static Type[] harmonyTypes = new Type[]
        {
            typeof(CodeInstruction),
            typeof(AccessTools),
        };
    }
}
