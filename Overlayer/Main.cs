using HarmonyLib;
using JSNet.API;
using JSNet.Utils;
using Newtonsoft.Json.Linq;
using Overlayer.Controllers;
using Overlayer.Core;
using Overlayer.Core.Scripting;
using Overlayer.Core.Patches;
using Overlayer.Core.TextReplacing;
using Overlayer.Core.Translation;
using Overlayer.Patches;
using Overlayer.Tags;
using Overlayer.Tags.Attributes;
using Overlayer.Unity;
using Overlayer.Utils;
using Overlayer.Views;
using RapidGUI;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;
using Time = UnityEngine.Time;

namespace Overlayer;
#if DEBUG
[UnityModManagerNet.EnableReloading]
#endif
public static class Main {
    [Tag(NotPlaying = true)]
    public static string Developer => Lang.Get("MISC_DEVELOPER", "Square3ang & Kkitut. Display everything as you wish. Thank you for being with Overlayer.");
    [Tag(NotPlaying = true)]
    public static string MipaNyang => "MipaNyang is God";
    [Tag(NotPlaying = true)]
    public static string Kyulio => "Kyulio is Sexy";

    public static Assembly Ass { get; private set; }
    public static ModEntry Mod { get; private set; }
    public static string ProfilePath => Path.Combine(Mod.Path, "profiles");
    [Tag(NotPlaying = true)] public static ModLogger Logger { get; private set; }
    [Tag(NotPlaying = true)] public static Settings Settings { get; private set; }
    public static GUIController GUI { get; private set; }
    public static Scene ActiveScene { get; private set; }
    [Tag(NotPlaying = true)] public static Translator Lang { get; internal set; }
    [Tag(NotPlaying = true)] public static Version ModVersion => Mod.Version;
    public static bool IsShowGUI { get; private set; } = false;
    private static UpdatePopup popup;

    public static string tooltip = "";
    public static Texture2D tooltipImage = null;

    public static string UpdateInfo = "";

    private static bool updateOnce = true;

    public static Texture2D Logo;

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

    public static void Load(ModEntry modEntry) {
        Mod = modEntry;
        Logger = modEntry.Logger;
        Ass = Assembly.GetExecutingAssembly();

        Version needReload = AutoUpdater.UpdateBeforeLoad(modEntry);
        if(needReload != null) {
            FieldInfo versionField = typeof(ModEntry).GetField("Version", BindingFlags.Instance | BindingFlags.Public);
            versionField?.SetValue(modEntry, needReload);
            modEntry.Info.Version = needReload.ToString();

            AutoUpdater.Reload(modEntry);
        }

        GUI = new GUIController();
        Lang = new Translator();
        Settings = new Settings();
        modEntry.OnToggle = OnToggle;
        modEntry.OnShowGUI = OnShowGUI;
        modEntry.OnGUI = OnGUI;
        modEntry.OnHideGUI = OnHideGUI;
        modEntry.OnSaveGUI = OnSaveGUI;
        modEntry.OnUpdate = OnUpdate;
        SceneManager.activeSceneChanged += (f, t) => ActiveScene = t;
        MiscUtils.SetAttr(TMPro.TMP_Settings.instance, "m_warningsDisabled", true);
    }

    public static IEnumerator LoadCoroutine(ModEntry modEntry) {
        yield return null;
        while(!RDString.initialized) {
            yield return null;
        }

        // ...
    }

    public static bool OnToggle(ModEntry modEntry, bool toggle) {
        if(toggle) {
            Settings.Load();
            Lang.Language = Settings.Lang;
            Lang.OnInitialize += OnLanguageInitialize;
            var settingsDrawer = new SettingsDrawer(Settings);
            Lang.OnInitialize += () => settingsDrawer.NeedLangInit = true;
            _ = Lang.Load(Path.Combine(Mod.Path, "lang"));
            LazyPatchManager.Load(Ass);
            LazyPatchManager.PatchInternal();
            Tag.InitializeWrapperAssembly();
            OverlayerTag.Initialize();
            TagManager.Initialize();
            TagManager.Load(Ass);
            FontManager.Initialize();
            TagResetter.Postfix();
            Tags.System.Init();
            ImageManager.Initialize();
            if(!Settings.DisableLogo) {
                LogoInit(modEntry.Path);
            }
            StaticCoroutine.Run(null);
            StaticCoroutine.Run(LoadCoroutine(modEntry));
            Scripting.Initalize();
            ProfileManager.Initialize();

            GUI.Init(settingsDrawer);
            GUI.Flush();

            if(updateOnce) {
                updateOnce = false;
                _ = AutoUpdater.InitAndUpdate(
                    Mod,
                    Settings.AutoUpdate,
                    Settings.AutoUpdateBeta,
                    () => UpdateInfo = Lang.Get("UPDATE_SUCESS", "Update Success!") + " " + Lang.Get("UPDATE_NEED_TO_RESTART", "Need to Restart!"),
                    (err) => {
                        Logger.Error("Update Fail: " + err);
                        UpdateInfo = Lang.Get("UPDATE_FAIL", "Update Fail") + ": " + err;
                    }
                );
            }
        } else {
            if(EgEnabled) {
                EgEnabled = false;
            }
            if(Logo != null) {
                Logo = null;
            }
            ProfileManager.Release();
            Scripting.Release();
            ImageManager.Release();
            Tags.System.Free();
            FontManager.Release();
            TagManager.Release();
            OverlayerTag.Release();
            Tag.ReleaseWrapperAssembly();
            LazyPatchManager.UnloadAll();
            Lang.Release();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            Settings.Save();
        }

        return true;
    }

    public static void OnShowGUI(ModEntry modEntry) {
        IsShowGUI = true;
        popup = new GameObject().AddComponent<UpdatePopup>();
        UnityEngine.Object.DontDestroyOnLoad(popup);
        popup.Initialize();

        //CodeEditor.CodeEditor.ignoreTextAreaNext.Clear();

        GUI.Flush();
    }

    public static void OnGUI(ModEntry modEntry) {
        if(!AutoUpdater.IsLatest) {
            GUILayout.Label($"<size=50><color=red>{Lang.Get("OUTDATED_DESCRIPTION", "Outdated Version Detected!")}</color></size>");
            GUILayout.BeginHorizontal();
            if(AutoUpdater.IsUpdating) {
                UpdateInfo = Lang.Get("UPDATING", "Updating...");
            } else {
                if(!AutoUpdater.RequireRestart) {
                    if(Drawer.Button($"<size=30>{Lang.Get("UPDATE", "Update")}</size>")) {
                        _ = AutoUpdater.CheckAndPrepareUpdate(modEntry, false,
                            () => UpdateInfo = Lang.Get("UPDATE_SUCESS", "Update Sucess!") + " " + Lang.Get("UPDATE_NEED_TO_RESTART", "Need to Restart!"),
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
                                () => UpdateInfo = Lang.Get("UPDATE_SUCESS", "Update Sucess!") + " " + Lang.Get("UPDATE_NEED_TO_RESTART", "Need to Restart!"),
                                (err) => {
                                    Logger.Error("Update Fail: " + err);
                                    UpdateInfo = Lang.Get("UPDATE_FAIL", "Update Fail") + ": " + err;
                                },
                                true
                            );
                        }
                    }
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label(UpdateInfo);

            GUILayout.Space(30);
        }

        if(AutoUpdater.IsBeta) {
            GUILayout.Label($"<size=30><color=lime>{Lang.Get("BETA_TEXT", "Beta Version")}</color></size>");
            GUILayout.Label($"{Lang.Get("BETA_DESCRIPTION", "Beta version may be unstable")}");
            GUILayout.Space(30);
        }

        tooltip = null;
        tooltipImage = null;

        GUI.Draw();
        GUILayout.Space(30);
        GUILayout.BeginHorizontal();
        if(Drawer.Button(Drawer.Icon_Discord, " Discord")) {
            Application.OpenURL("https://discord.modlist.org/");
        }
        Drawer.HoverTooltip("modlist.org");
        if(Drawer.Button(Drawer.Icon_Github, " GitHub")) {
            Application.OpenURL("https://github.com/modlist-org/Overlayer");
        }
        if(Drawer.Button(Drawer.Icon_Wiki, " Wiki")) {
            Application.OpenURL("https://github.com/modlist-org/Overlayer/wiki");
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
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
                GUILayout.Label($"You are using an <color=#{Tags.Effect.Rainbow(12)}>SPECIAL BETA!</color>");
            }
        } else if(AutoUpdater.IsRateLimited) {
            GUILayout.Label($"<color=yellow>{Lang.Get("UPDATE_RATE_LIMITED", "Update or Version check Rate Limited!")}</color>");
        }

        if(!RGUI.PopupWindow.isOpen) {
            if(Settings.Tooltip) {
                if(!Drawer.Tooltip(tooltip)) {
                    Drawer.Tooltip(tooltipImage);
                }
            }
        }
    }

    public static void OnHideGUI(ModEntry modEntry) {
        IsShowGUI = false;
        //CodeEditor.CodeEditor.ignoreTextAreaNext.Clear();
        Drawer.codeEditor.undoRedoManagers.Clear();
        GUI.Flush();
    }

    public static void OnSaveGUI(ModEntry modEntry) {
        ProfileManager.Save();
        Settings.Save();
    }

    public static void OnUpdate(ModEntry modEntry, float delta) => MainThreadDispatcher.Update();

    public static bool IsPlaying {
        get {
            var ctrl = scrController.instance;
            var cdt = scrConductor.instance;
            return ctrl != null && cdt != null && !ctrl.paused && cdt.isGameWorld;
        }
    }

    public static void OnLanguageInitialize() {
        string[] translatorLogs = Lang.Logs;
        if(translatorLogs != null && translatorLogs.Length > 0) {
            foreach(var log in translatorLogs) {
                Logger.Log(log);
            }
        }
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

    public static class MainThreadDispatcher {
        private static readonly ConcurrentQueue<Action> queue = new();
        public static void Enqueue(Action action) => queue.Enqueue(action);
        public static void Update() {
            while(queue.TryDequeue(out var action)) {
                action.Invoke();
            }
        }
    }
}