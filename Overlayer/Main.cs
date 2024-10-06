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
using Time = UnityEngine.Time;

namespace Overlayer
{
    public static class Main
    {
        [Tag(NotPlaying = true)]
        public static string Developer => Lang.Get("MISC_DEVELOPER", "Super Kawaii Suckyoubus Chan~♥");

        public static Assembly Ass { get; private set; }
        public static ModEntry Mod { get; private set; }
        [Tag(NotPlaying = true)] public static ModLogger Logger { get; private set; }
        [Tag(NotPlaying = true)] public static Settings Settings { get; private set; }
        public static GUIController GUI { get; private set; }
        [Tag(NotPlaying = true)] public static Scene ActiveScene { get; private set; }
        public static HttpClient HttpClient { get; private set; }
        public static Translator Lang { get; internal set; }
        [Tag(NotPlaying = true)] public static Version ModVersion { get; private set; }
        /*
        [Tag(NotPlaying = true)] public static Version LastestVersion { get; private set; }
        [Tag(NotPlaying = true)] public static string DownloadLink { get; private set; }
        public static long GGReqCnt, GetGGReqCnt, TUFReqCnt, GetTUFReqCnt, PlayCnt, HandshakeCnt;
        */
        private static UpdatePopup popup;

        private static bool showTooltip = false;
        private static string tooltip = "";

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
            yield return null;
            while (!RDString.initialized) yield return null;
            PatchGuard.Ignore(TextManager.Initialize);
        }

        public static bool OnToggle(ModEntry modEntry, bool toggle)
        {
            if (toggle)
            {
                PatchGuard.Ignore(() =>
                {
                    StaticCoroutine.Run(null);
                    StaticCoroutine.Run(LoadCoroutine(modEntry));
                    Settings = ModSettings.Load<Settings>(modEntry);
                    _ = Lang.LoadTranslationsAsync(Path.Combine(Mod.Path, "lang"));
                    LazyPatchManager.Load(Ass);
                    LazyPatchManager.PatchInternal();
                    Tag.InitializeWrapperAssembly();
                    OverlayerTag.Initialize();
                    TagManager.Initialize();
                    TagManager.Load(Ass);
                    FontManager.Initialize();
                    TagResetter.Postfix();
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
            popup = new GameObject().AddComponent<UpdatePopup>();
            UnityEngine.Object.DontDestroyOnLoad(popup);
            popup.Initialize();

            GUI.Flush();
        }

        public static float preparinglastUpdateTime = 0f;
        public static string[] preparingsymbols = { ".","..","..." };
        public static int preparingsymbolIndex = 0;
        public static float helptime = 0f;

        public static void OnGUI(ModEntry modEntry)
        {
            if(Lang.GetLoading())
            {
                float elapsedTime = Time.time - preparinglastUpdateTime;

                if(elapsedTime >= 0.05f)
                {
                    preparingsymbolIndex = (preparingsymbolIndex + 1) % preparingsymbols.Length;
                    preparinglastUpdateTime = Time.time;
                }

                GUILayout.Label("Preparing" + preparingsymbols[preparingsymbolIndex]);

                helptime += Time.deltaTime;
                if(helptime >= 4f)
                {
                    GUILayout.Label("Is the Preparing is taking too long??\nplease get in touch with the developer for assistance!!");
                }
                else
                {
                    GUILayout.Label("");
                }
            }
            else
            {
                showTooltip = false;
                helptime = 0f;
                GUI.Draw();
                GUILayout.Space(30);
                GUILayout.BeginHorizontal();
                if (Drawer.Button("Square Mod Server"))
                {
                    Application.OpenURL("https://square.lrl.kr/");
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (showTooltip)
                {
                    Drawer.Tooltip(tooltip);
                }
            }
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