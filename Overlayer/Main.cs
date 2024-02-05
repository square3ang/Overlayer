using Overlayer.Core.Patches;
using Overlayer.Tags;
using System;
using System.Reflection;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace Overlayer
{
    public static class Main
    {
        public static Assembly Ass { get; private set; }
        public static ModEntry Mod { get; private set; }
        public static ModLogger Logger { get; private set; }
        public static void Load(ModEntry modEntry)
        {
            Ass = Assembly.GetExecutingAssembly();
            Mod = modEntry;
            Logger = modEntry.Logger;
        }
        public static bool OnToggle(ModEntry modEntry, bool toggle)
        {
            if (toggle)
            {
                LazyPatchManager.Load(Ass);
                LazyPatchManager.PatchInternal();
                TagManager.Initialize();
            }
            else
            {
                TagManager.Release();
                LazyPatchManager.UnloadAll();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            }
            return true;
        }
    }
}
