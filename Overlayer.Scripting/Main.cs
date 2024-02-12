using JSNet;
using JSNet.API;
using Overlayer.Core;
using Overlayer.Tags;
using Overlayer.Unity;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static bool CurrentExecutingScriptIsSafe { get; private set; }
        public static bool IsFullySafe { get; private set; } = true;
        public static ModEntry Mod { get; private set; }
        public static ModLogger Logger { get; private set; }
        public static Api JSApi { get; private set; }
        public static Api JSVerifyApi { get; private set; }
        public static void Load(ModEntry modEntry)
        {
            Mod = modEntry;
            Logger = modEntry.Logger;
            Assembly = Assembly.GetExecutingAssembly();
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
        }
        public static bool OnToggle(ModEntry modEntry, bool toggle)
        {
            if (toggle)
            {
                FieldInfo apiMethodsField = typeof(Api).GetField("ApiMethods", (BindingFlags)15420);
                JSApi = new Api();
                Impl.apiMethods = apiMethodsField.GetValue(JSApi) as List<(ApiAttribute, MethodInfo)>;
                JSApi.RegisterType(typeof(Impl));

                JSVerifyApi = new Api();
                JSVerifyApi.RegisterType(typeof(Impl_Verify));

                TagManager.Load(typeof(Expression));
                foreach (var tag in TagManager.All)
                    Impl.apiMethods.Add((new ApiAttribute(tag.Name), tag.Tag.GetterOriginal));
                RunScriptsNonBlocking(ScriptPath);
            }
            else
            {
                TagManager.RemoveTag("Expression");
                JSApi = null;
                JSVerifyApi = null;
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
                    prevScript = MiscUtils.ExecuteSafe(() => Script.InterpretAPI(JSApi, SandboxJSCode), out e);
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
                    prevScript = MiscUtils.ExecuteSafe(() => Script.InterpretAPI(JSApi, SandboxJSCode), out e);
                    var result = prevScript?.Eval();
                    SandboxResult = e?.ToString() ?? result?.ToString() ?? "null";
                }, out e);
                if (e != null) SandboxResult = e.ToString();
                EndScript();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label($"Result:\n{SandboxResult}");
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
            File.WriteAllText(Path.Combine(ScriptPath, "Impl.js"), JSApi.Generate());
            Logger.Log("Preparing Executing Scripts..");
            Impl.Reload();
            foreach (string script in Directory.GetFiles(folderPath))
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
                        var result = Script.InterpretAPI(JSApi, script);
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
            StaticCoroutine.Queue(StaticCoroutine.SyncRunner(TextManager.Refresh));
        }
        public static void BeginScript(bool sandbox = false)
        {
            Impl.IsFirstCall = true;
            Impl.ignoreSafety = false;
            if (sandbox)
            {
                CurrentExecutingScript = SandboxJSCode;
                CurrentExecutingScriptPath = "Sandbox.js";
            }
            CurrentExecutingScriptIsSafe = Impl_Verify.IsSafe(CurrentExecutingScript);
        }
        public static void EndScript()
        {
            IsFullySafe &= !Impl.ignoreSafety || CurrentExecutingScriptIsSafe;
            Logger.Log($"IsFullySafe:{IsFullySafe}");
            CurrentExecutingScript = null;
            CurrentExecutingScriptPath = null;
        }
    }
}
