using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static UnityModManagerNet.UnityModManager;

namespace Overlayer.Bootstrapper;

public static class Main {
    private static readonly string[] requiredDlls = [
        "System.Memory",
        "NCalc",
        "JSNet",
        "Vostok.Sys.Metrics.PerfCounters"
    ];
    private static readonly string FailName = "Overlayer [FAIL]";
    public static void Load(ModEntry modEntry) {
        void SetFail() {
            SetFail();
        }

        string libPath = Path.Combine(modEntry.Path, "lib");
        if(!Directory.Exists(libPath)) {
            modEntry.Logger.Log("/lib/ folder does not exist.");
            SetFail();
            return;
        }

        var dllFiles = Directory.GetFiles(libPath, "*.dll", SearchOption.AllDirectories);
        modEntry.Logger.Log($"Found {dllFiles.Length} DLL(s) in /lib/");

        var loadedTitles = new HashSet<string>();

        foreach(var dllPath in dllFiles) {
            var name = Path.GetFileNameWithoutExtension(dllPath);

            bool alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if(alreadyLoaded) {
                modEntry.Logger.Log($"Already loaded: {name}");
                continue;
            }

            try {
                var assembly = Assembly.Load(File.ReadAllBytes(dllPath));
                modEntry.Logger.Log($"Loaded: {assembly.GetName().Name}");
                loadedTitles.Add(assembly.GetName().Name);
            } catch(Exception e) {
                modEntry.Logger.Log($"Failed to load {dllPath}: {e}");
                SetFail();
            }
        }

        foreach(var required in requiredDlls) {
            if(!loadedTitles.Contains(required) &&
               !AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name.Equals(required, StringComparison.OrdinalIgnoreCase))) {

                modEntry.Logger.Log($"[ERROR] Required assembly '{required}' not loaded.");
                SetFail();
                return;
            }
        }

        string mainPath = Path.Combine(modEntry.Path, "Overlayer.dll");
        if(!File.Exists(mainPath)) {
            modEntry.Logger.Log("Overlayer.dll not found");
            SetFail();
            return;
        }

        try {
            var mainAss = Assembly.Load(File.ReadAllBytes(mainPath));
            modEntry.Logger.Log("Loaded Overlayer.dll successfully");
            typeof(ModEntry).GetField("mAssembly", (BindingFlags)15420).SetValue(modEntry, mainAss);
            mainAss.GetType("Overlayer.Main").GetMethod("Load").Invoke(null, [modEntry]);
            modEntry.Logger.Log("Overlayer.Main.Load invoked successfully");
        } catch(Exception e) {
            modEntry.Logger.Log($"Failed to load or invoke Overlayer.dll: {e}");
            SetFail();
        }
    }
}