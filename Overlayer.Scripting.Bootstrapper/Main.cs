using System;
using System.IO;
using System.Reflection;
using static UnityModManagerNet.UnityModManager;

namespace Overlayer.Scripting.Bootstrapper
{
    public static class Main
    {
        public static void Load(ModEntry modEntry)
        {
            var domain = AppDomain.CurrentDomain;
            var jsNet = File.ReadAllBytes(Path.Combine(modEntry.Path, "JSNet.dll"));
            var scripting = File.ReadAllBytes(Path.Combine(modEntry.Path, "Overlayer.Scripting.dll"));
            domain.Load(jsNet);
            var scriptingAss = domain.Load(scripting);
            typeof(ModEntry).GetField("mAssembly", (BindingFlags)15420).SetValue(modEntry, scriptingAss);
            scriptingAss.GetType("Overlayer.Scripting.Main").GetMethod("Load").Invoke(null, new object[] { modEntry });
        }
    }
}
