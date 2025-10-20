using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static UnityModManagerNet.UnityModManager;

namespace Overlayer.Scripting.Bootstrapper {
    public static class Main {
        private static readonly string[] requiredDlls = new[] { "JSNet", "Vostok" };
        public static void Load(ModEntry modEntry) {
            var domain = AppDomain.CurrentDomain;

            string libPath = Path.Combine(modEntry.Path, "lib");
            if(!Directory.Exists(libPath)) {
                modEntry.Logger.Log("/lib/ folder does not exist.");
                return;
            }

            var dllFiles = Directory.GetFiles(libPath, "*.dll", SearchOption.AllDirectories);
            modEntry.Logger.Log($"Found {dllFiles.Length} DLL(s) in /lib/");

            var loadedTitles = new HashSet<string>();

            foreach(var dllPath in dllFiles) {
                try {
                    var bytes = File.ReadAllBytes(dllPath);
                    var assembly = Assembly.Load(bytes);
                    modEntry.Logger.Log($"Loaded assembly {assembly.FullName} from {dllPath}");

                    var productAttr = assembly.GetCustomAttribute<AssemblyProductAttribute>();
                    if(productAttr != null) {
                        loadedTitles.Add(productAttr.Product);
                        modEntry.Logger.Log($"AssemblyTitle: {productAttr.Product}");
                    }
                } catch(Exception e) {
                    modEntry.Logger.Log($"Failed to load {dllPath}: {e}");
                }
            }

            foreach(var required in requiredDlls) {
                if(!loadedTitles.Contains(required)) {
                    modEntry.Logger.Log($"[ERROR] Required assembly with title '{required}' not loaded. Aborting scripting load.");
                    return;
                }
            }

            string scriptingPath = Path.Combine(modEntry.Path, "Overlayer.Scripting.dll");
            if(!File.Exists(scriptingPath)) {
                modEntry.Logger.Log("Overlayer.Scripting.dll not found");
                return;
            }

            try {
                var scriptingAss = Assembly.Load(File.ReadAllBytes(scriptingPath));
                modEntry.Logger.Log("Loaded Overlayer.Scripting.dll successfully");

                typeof(ModEntry)
                    .GetField("mAssembly", (BindingFlags)15420)
                    .SetValue(modEntry, scriptingAss);

                scriptingAss
                    .GetType("Overlayer.Scripting.Main")
                    .GetMethod("Load")
                    .Invoke(null, new object[] { modEntry });

                modEntry.Logger.Log("Overlayer.Scripting.Main.Load invoked successfully");
            } catch(Exception e) {
                modEntry.Logger.Log($"Failed to load or invoke Overlayer.Scripting.dll: {e}");
            }
        }
    }
}
