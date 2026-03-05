using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Overlayer.Core;

public class DllImporter {
    private static bool nCalcInitialized = false;
    public static void NCalcInitialize() {
        if(!nCalcInitialized) {
            string dll = Path.Combine(Main.Mod.Path, "lib", "NCalc.dll");

            bool alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name.Equals("NCalc", StringComparison.OrdinalIgnoreCase));

            if(alreadyLoaded) {
                nCalcInitialized = true;
                Main.Logger.Log($"Already loaded: {dll}");
                return;
            }

            if(File.Exists(dll)) {
                try {
                    Assembly.LoadFrom(dll);
                    nCalcInitialized = true;
                    Main.Logger.Log($"Loaded: {dll}");
                } catch(Exception ex) {
                    Main.Logger.Error($"Failed to load {dll}: {ex.Message}");
                }
            } else {
                Main.Logger.Error($"DLL not found: {dll}");
            }
        }
    }
}
