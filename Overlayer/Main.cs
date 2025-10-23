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
using RapidGUI;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;
using Time = UnityEngine.Time;

namespace Overlayer
{
#if DEBUG
[UnityModManagerNet.EnableReloading]
#endif
    public static class Main
    {
        [Tag(NotPlaying = true)]
        public static string Developer => Lang.Get("MISC_DEVELOPER","Square & Kkitut. Display everything as you wish. Thank you for being with Overlayer.");
        [Tag(NotPlaying = true)]
        public static string MipaNyang => "MipaNyang is God";

        public static Assembly Ass { get; private set; }
        public static ModEntry Mod { get; private set; }
        [Tag(NotPlaying = true)] public static ModLogger Logger { get; private set; }
        [Tag(NotPlaying = true)] public static Settings Settings { get; private set; }
        public static GUIController GUI { get; private set; }
        public static Scene ActiveScene { get; private set; }
        [Tag(NotPlaying = true)] public static Translator Lang { get; internal set; }
        [Tag(NotPlaying = true)] public static Version ModVersion() => Mod.Version;
        public static bool IsShowGUI { get; private set; } = false;
        private static UpdatePopup popup;

        public static string tooltip = "";
        public static string UpdateInfo = "";

        public static Texture2D Logo;

        internal static Wiki.Wiki Wiki;

        internal static Olly.Olly Eg;
        private static bool _egEnabled = false;
        internal static bool EgEnabled {
            get => _egEnabled;
            set {
                if(_egEnabled != value) {
                    if(value) {
                        if(Olly.OllyResources.LoadAll(Mod)) {
                            Eg = new GameObject().AddComponent<Olly.Olly>();
                            UnityEngine.Object.DontDestroyOnLoad(Eg);
                            Eg.Init();
                            _egEnabled = value;
                        }
                    } else {
                        UnityEngine.Object.Destroy(Eg.gameObject);
                        Eg.Release();
                        Eg = null;
                        Olly.OllyResources.UnloadAll();
                        _egEnabled = value;
                    }
                }
            }
        }

        public static void Load(ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            Ass = Assembly.GetExecutingAssembly();
            Mod = modEntry;

            Version needReload = AutoUpdater.UpdateBeforeLoad(modEntry);
            if(needReload != null) {
                FieldInfo versionField = typeof(ModEntry).GetField("Version", BindingFlags.Instance | BindingFlags.Public);
                versionField?.SetValue(modEntry, needReload);
                modEntry.Info.Version = needReload.ToString();

                AutoUpdater.Reload(modEntry);
            }

            GUI = new GUIController();
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
            TextManager.Initialize();
            yield return null;
        }

        public static bool OnToggle(ModEntry modEntry, bool toggle)
        {
            if (toggle)
            {
                StaticCoroutine.Run(null);
                StaticCoroutine.Run(LoadCoroutine(modEntry));
                Settings = ModSettings.Load<Settings>(modEntry);
                Lang.CurrentLanguage = Settings.Lang;
                _ = Lang.LoadTranslationsAsync(Path.Combine(Mod.Path, "lang"));
                LazyPatchManager.Load(Ass);
                LazyPatchManager.PatchInternal();
                Tag.InitializeWrapperAssembly();
                OverlayerTag.Initialize();
                TagManager.Initialize();
                TagManager.Load(Ass);
                FontManager.Initialize();
                TagResetter.Postfix();
                Tags.System.Init();
                DllImporter.NCalcInitialize();
                if(!Settings.disableLogo) {
                    LogoInit(modEntry.Path);
                }
            }
            else
            {
                if(EgEnabled) {
                    EgEnabled = false;
                }
                if(Logo != null) {
                    Logo = null;
                }
                Tags.System.Free();
                TextManager.Release();
                FontManager.Release();
                TagManager.Release();
                OverlayerTag.Release();
                Tag.ReleaseWrapperAssembly();
                LazyPatchManager.UnloadAll();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                ModSettings.Save(Settings, modEntry);
            }

            return true;
        }

        public static void OnShowGUI(ModEntry modEntry)
        {
            IsShowGUI = true;
            popup = new GameObject().AddComponent<UpdatePopup>();
            UnityEngine.Object.DontDestroyOnLoad(popup);
            popup.Initialize();

            //CodeEditor.CodeEditor.ignoreTextAreaNext.Clear();

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
                    preparingsymbolIndex++;
                    if(preparingsymbolIndex >= preparingsymbols.Length) {
                        preparingsymbolIndex = 0;
                    }
                    preparinglastUpdateTime = Time.time;
                }

                GUILayout.Label(Lang.Get("PREPARING", "Preparing") + preparingsymbols[preparingsymbolIndex]);

                helptime += Time.deltaTime;
                if(helptime >= 4f)
                {
                    GUILayout.Label(Lang.Get("LONG_PREPARING","Is the Preparing is taking too long??\nplease get in touch with the developer for assistance!!"));
                }
                else
                {
                    GUILayout.Label("");
                }
            }
            else
            {
                if (!AutoUpdater.isLatest)
                {
                    GUILayout.Label($"<size=50><color=red>{Lang.Get("OUTDATED_DESCRIPTION", "Outdated Version Detected!")}</color></size>");
                    GUILayout.BeginHorizontal();
                    if(AutoUpdater.IsUpdating) {
                        UpdateInfo = Lang.Get("UPDATING", "Updating...");
                    } else {
                        if(!AutoUpdater.RequireRestart) {
                            if(Drawer.Button($"<size=30>{Lang.Get("UPDATE", "Update")}</size>")) {
                                _ = AutoUpdater.CheckAndPrepareUpdate(modEntry, false,
                                    () => {
                                        UpdateInfo = Lang.Get("UPDATE_SUCESS", "Update Sucess!") + " " + Lang.Get("UPDATE_NEED_TO_RESTART", "Need to Restart!");
                                    },
                                    (err) => {
                                        Logger.Error("Update Fail: " + err);
                                        UpdateInfo = Lang.Get("UPDATE_FAIL", "Update Fail") + ": " + err;
                                    },
                                    true
                                );
                            }
                            if(AutoUpdater.BetaUrl != null) {
                                if(Drawer.Button($"<size=30>{Lang.Get("BETA", "Beta")}</size>")) {
                                    _ = AutoUpdater.CheckAndPrepareUpdate(modEntry, true,
                                        () => {
                                            UpdateInfo = Lang.Get("UPDATE_SUCESS", "Update Sucess!") + " " + Lang.Get("UPDATE_NEED_TO_RESTART", "Need to Restart!");
                                        },
                                        (err) => {
                                            Logger.Error("Update Fail: " + err);
                                            UpdateInfo = Lang.Get("UPDATE_FAIL", "Update Fail") + ": " + err;
                                        },
                                        true
                                    );
                                }
                            }
                        }
                        if(Drawer.Button("<size=30>Square Mod Server</size>")) {
                            Application.OpenURL("https://square.lrl.kr/");
                        }
                        if(Drawer.Button("<size=30>GitHub</size>")) {
                            Application.OpenURL("https://github.com/modlist-org/Overlayer");
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Label(UpdateInfo);
                    
                    GUILayout.Space(30);
                }

                if (AutoUpdater.isBeta)
                {
                    GUILayout.Label($"<size=30><color=lime>{Lang.Get("BETA_TEXT", "Beta Version")}</color></size>");
                    GUILayout.Label($"{Lang.Get("BETA_DESCRIPTION", "Beta version may be unstable")}");
                    GUILayout.Space(30);
                }

                helptime = 0f;
                tooltip = "";
                GUI.Draw();
                GUILayout.Space(30);
                if(AutoUpdater.isLatest || AutoUpdater.isBeta) {
                    GUILayout.BeginHorizontal();
                    if(Drawer.Button("Square Mod Server")) {
                        Application.OpenURL("https://square.lrl.kr/");
                    }
                    if(Drawer.Button("GitHub")) {
                        Application.OpenURL("https://github.com/modlist-org/Overlayer");
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                if(AutoUpdater.CurrentVersionType != AutoUpdater.VersionType.Unknown) {
                    if(AutoUpdater.LatestVersion != null) {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("STABLE", GUILayout.Width(60));
                        GUILayout.Label($":  {AutoUpdater.LatestVersion}");
                        if(AutoUpdater.CurrentVersionType == AutoUpdater.VersionType.Stable) {
                            GUILayout.Label(" <<");
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    if(AutoUpdater.BetaVersion != null) {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("BETA", GUILayout.Width(60));
                        GUILayout.Label($":  {AutoUpdater.BetaVersion}");
                        if(AutoUpdater.CurrentVersionType == AutoUpdater.VersionType.Beta) {
                            GUILayout.Label(" <<");
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    if(AutoUpdater.CurrentVersionType == AutoUpdater.VersionType.UnknownBeta) {
                        GUILayout.Label($"You are using an <color=#{Tags.Effect.Rainbow(12)}>SPESIAL BETA!</color>");
                    }
                }

                if(!RGUI.PopupWindow.isOpen) {
                    if(Settings.useTooltip) {
                        Drawer.Tooltip(tooltip);
                    }
                }
            }
        }

        public static void OnHideGUI(ModEntry modEntry)
        {
            IsShowGUI = false;
            //CodeEditor.CodeEditor.ignoreTextAreaNext.Clear();
            Drawer.codeEditor.undoRedoManagers.Clear();
            GUI.Flush();
        }

        public static void OnSaveGUI(ModEntry modEntry)
        {
            TextManager.Save();
            ModSettings.Save(Settings, modEntry);
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
            _ = AutoUpdater.InitAndUpdate(
                Mod,
                Settings.useAutoUpdate,
                Settings.useAutoUpdateBeta,
                () => {
                    UpdateInfo = Lang.Get("UPDATE_SUCESS", "Update Success!") + " " + Lang.Get("UPDATE_NEED_TO_RESTART", "Need to Restart!");
                },
                (err) => {
                    Logger.Error("Update Fail: " + err);
                    UpdateInfo = Lang.Get("UPDATE_FAIL", "Update Fail") + ": " + err;
                }
            );

            GUI.Flush();
            GUI.Init(new SettingsDrawer(Settings));
        }

        public static void LogoInit(string path) {
            string logopath = Path.Combine(path, "ov3_logo.png");
            if(System.IO.File.Exists(logopath)) {
                byte[] fileData = System.IO.File.ReadAllBytes(logopath);
                Logo = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                Logo.LoadImage(fileData);
            } else {
                Logger.Log("Logo image not found!");
            }
        }
        public static void LogoRelease() {
            if(Logo != null) {
                UnityEngine.Object.Destroy(Logo);
                Logo = null;
            }
        }
    }
}