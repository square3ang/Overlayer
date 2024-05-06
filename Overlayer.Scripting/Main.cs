using JSNet;
using JSNet.API;
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
using System.Threading.Tasks;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace Overlayer.Scripting
{
    public static class Main
    {
        public static string ScriptPath => Path.Combine(Mod.Path, "Scripts");
        public static Assembly Assembly { get; private set; }
        public static bool ScriptsRunning { get; private set; }
        public static string CurrentExecutingScript { get; private set; }
        public static string CurrentExecutingScriptPath { get; private set; }
        public static ModEntry Mod { get; private set; }
        public static ModLogger Logger { get; private set; }
        public static Settings Settings { get; private set; }
        public static Api JSExecutionApi { get; private set; }
        public static Api JSExpressionApi { get; private set; }
        public static bool PatchesLocked { get; private set; }
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
            if (toggle)
            {
                Settings = ModSettings.Load<Settings>(modEntry);

                JSExecutionApi = new Api();
                JSExecutionApi.RegisterType(typeof(Impl));

                TagManager.Load(typeof(Expression));
                TagManager.Load(typeof(PerformanceTags));

                JSExpressionApi = new Api();
                JSExpressionApi.RegisterType(typeof(Impl));
                foreach (var tag in TagManager.All)
                    JSExpressionApi.Methods.Add((new ApiAttribute(tag.Name), tag.Tag.GetterOriginal));

                foreach (var att in GetADOFAITagTypes())
                {
                    var tuple = (new ApiAttribute(att.Name), att);
                    JSExecutionApi.Types.Add(tuple);
                    JSExpressionApi.Types.Add(tuple);
                }

                OverlayerText.OnApplyConfig += text =>
                {
                    if (!PatchesLocked && TagManager.HasReference(typeof(Expression)))
                    {
                        LazyPatchManager.PatchAll().ForEach(lp => lp.Locked = true);
                        PatchesLocked = true;
                    }
                };

                RunScriptsNonBlocking(ScriptPath);
                PerformanceTags.Initialize();
            }
            else
            {
                PerformanceTags.Release();
                TagManager.Unload(typeof(Expression));
                TagManager.Unload(typeof(PerformanceTags));
                Impl.Release();
                JSExecutionApi = null;
                JSExpressionApi = null;
                ModSettings.Save(Settings, modEntry);
            }
            Expression.expressions.Clear();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
            return true;
        }
        static Script prevScript;
        static string SandboxJSCode = string.Empty;
        static string SandboxResult = "Not Executed.";
        public static void OnGUI(ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload Scripts"))
                RunScriptsNonBlocking(ScriptPath);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("Test Code:");
            SandboxJSCode = GUILayout.TextArea(SandboxJSCode);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Execute"))
            {
                Exception e;
                MiscUtils.ExecuteSafe(() =>
                {
                    BeginScript(true);
                    prevScript = MiscUtils.ExecuteSafe(() => Script.InterpretAPI(JSExecutionApi, SandboxJSCode), out e);
                    prevScript?.Exec();
                    SandboxResult = e?.ToString() ?? "Success";
                }, out e);
                if (e != null) SandboxResult = e.ToString();
                EndScript();
            }
            if (GUILayout.Button("Evaluate"))
            {
                Exception e;
                MiscUtils.ExecuteSafe(() =>
                {
                    BeginScript(true);
                    prevScript = MiscUtils.ExecuteSafe(() => Script.InterpretAPI(JSExecutionApi, SandboxJSCode), out e);
                    var result = prevScript?.Eval();
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
        public static async Task RunScripts(string folderPath)
        {
            if (ScriptsRunning)
            {
                Logger.Log("Scripts Already Running! Aborting..");
                return;
            }
            ScriptsRunning = true;
            Logger.Log("Start Running Scripts..");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            Logger.Log("Generating Script Implementations..");
            File.WriteAllText(Path.Combine(ScriptPath, "Impl.js"), JSExpressionApi.Generate());
            Logger.Log("Preparing Executing Scripts..");
            Impl.Reload();
            foreach (string script in Directory.GetFiles(folderPath, "*.js", SearchOption.AllDirectories))
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
                    var time = MiscUtils.MeasureTime(() =>
                    {
                        var result = Script.InterpretAPI(JSExecutionApi, script);
                        result.Exec();
                        result.Dispose();
                    });
                    EndScript();
                    Logger.Log($"Executed \"{name}\" Script Successfully. ({time.TotalMilliseconds}ms)");
                    return true;
                }
                catch (Exception e) { Logger.Log($"Exception At Executing Script \"{name}\":\n{e}"); return false; }
            });
        }
        public static async void RunScriptsNonBlocking(string folderPath)
        {
            await RunScripts(folderPath);
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
    }
}
