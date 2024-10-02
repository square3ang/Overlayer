using Overlayer.Controllers;
using Overlayer.Core;
using Overlayer.Core.Patches;
using Overlayer.Core.TextReplacing;
using Overlayer.Core.Translatior;
using Overlayer.Patches;
using Overlayer.Tags;
using Overlayer.Tags.Attributes;
using Overlayer.Unity;
using Overlayer.Utils;
using Overlayer.Views;
using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace Overlayer
{
    public static class Main
    {
        [Tag(NotPlaying = true)]
        public static string Developer => Lang.Get("MISC_DEVELOPER","Super Kawaii Suckyoubus Chan~♥");
        public static Assembly Ass { get; private set; }
        public static ModEntry Mod { get; private set; }
        [Tag(NotPlaying = true)]
        public static ModLogger Logger { get; private set; }
        [Tag(NotPlaying = true)]
        public static Settings Settings { get; private set; }
        public static GUIController GUI { get; private set; }
        [Tag(NotPlaying = true)]
        public static Scene ActiveScene { get; private set; }
        public static HttpClient HttpClient { get; private set; }
        public static Translator Lang { get; internal set; }
        [Tag(NotPlaying = true)]
        public static Version ModVersion { get; private set; }
        [Tag(NotPlaying = true)]
        public static Version LastestVersion { get; private set; }
        [Tag(NotPlaying = true)]
        public static string DownloadLink { get; private set; }
        public static long GGReqCnt, GetGGReqCnt, TUFReqCnt, GetTUFReqCnt, PlayCnt, HandshakeCnt;
        public static void Load(ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            Ass = Assembly.GetExecutingAssembly();
            Mod = modEntry;
            GUI = new GUIController();
            HttpClient = new HttpClient();
            ModVersion = modEntry.Version;
            Lang = new Translator();
            modEntry.OnToggle = OnToggle;
            modEntry.OnShowGUI = OnShowGUI;
            modEntry.OnGUI = OnGUI;
            modEntry.OnHideGUI = OnHideGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            Lang.OnInitialize += OnLanguageInitialize;
            SceneManager.activeSceneChanged += (f, t) => ActiveScene = t;
            MiscUtils.SetAttr(TMPro.TMP_Settings.instance, "m_warningsDisabled", true);
            
        }

        public static IEnumerator LoadCoroutine(ModEntry modEntry)
        {
            while (!RDString.initialized) yield return null;
            PatchGuard.Ignore(() =>
            {
                Settings = ModSettings.Load<Settings>(modEntry);
                _ = Lang.LoadTranslationsAsync(Path.Combine(Mod.Path, "lang"));
                LazyPatchManager.Load(Ass);
                LazyPatchManager.PatchInternal();
                Tag.InitializeWrapperAssembly();
                OverlayerTag.Initialize();
                TagManager.Initialize();
                TagManager.Load(Ass);
                FontManager.Initialize();
                TextManager.Initialize();
                TagResetter.Postfix();
            });
        }
        public static bool OnToggle(ModEntry modEntry, bool toggle)
        {
            if (toggle)
            {
                PatchGuard.Ignore(() =>
                {
                    StaticCoroutine.Run(null);
                    StaticCoroutine.Run(LoadCoroutine(modEntry));
                });
            }
            else
            {
                PatchGuard.Ignore(() =>
                {
                    TextManager.Release();
                    FontManager.Release();
                    TagManager.Release();
                    OverlayerTag.Release();
                    Tag.ReleaseWrapperAssembly();
                    LazyPatchManager.UnloadAll();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                    ModSettings.Save(Settings, modEntry);
                });
            }
            return true;
        }
        public static void OnShowGUI(ModEntry modEntry)
        {
            GUI.Flush();
        }
        public static void OnGUI(ModEntry modEntry)
        {
            if (Lang.GetLoading())
                GUILayout.Label("Preparing...");
            else GUI.Draw();
        }
        public static void OnHideGUI(ModEntry modEntry)
        {
            GUI.Flush();
        }
        public static void OnSaveGUI(ModEntry modEntry)
        {

            PatchGuard.Ignore(() =>
            {
                TextManager.Save();
                ModSettings.Save(Settings, modEntry);
            });
        }
        public static bool IsPlaying
        {
            get
            {
                var ctrl = scrController.instance;
                var cdt = scrConductor.instance;
                if (ctrl != null && cdt != null)
                    return !ctrl.paused && cdt.isGameWorld;
                return false;
            }
        }
        public static void OnLanguageInitialize()
        {
            GUI.Flush();
            GUI.Init(new SettingsDrawer(Settings));
        }
    }
}
