using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Overlayer.Core {
    public class DllImporter {
        private static bool nCalcInitialized = false;
        public static void NCalcInitialize() {
            if(!nCalcInitialized) {
                string dll = Path.Combine(Main.Mod.Path, "lib", "NCalc.dll");

                bool alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                    .Any(a => a.GetName().Name.Equals("NCalc", StringComparison.OrdinalIgnoreCase));

                if(alreadyLoaded) {
                    nCalcInitialized = true;
                    Console.WriteLine($"[Field] Already loaded: {dll}");
                    return;
                }

                if(File.Exists(dll)) {
                    try {
                        Assembly.LoadFrom(dll);
                        nCalcInitialized = true;
                        Console.WriteLine($"[Field] Loaded: {dll}");
                    } catch(Exception ex) {
                        Console.WriteLine($"[Field] Failed to load {dll}: {ex.Message}");
                    }
                } else {
                    Console.WriteLine($"[Field] DLL not found: {dll}");
                }
            }
        }
    }
}
