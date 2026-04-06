using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Overlayer.Core;
using Overlayer.Core.Patches;
using Overlayer.Core.Scripting;
using Overlayer.Core.Translation;
using Overlayer.Models;
using Overlayer.Patches;
using Overlayer.Tags;
using Overlayer.Unity;
using Overlayer.Utils;
using RapidGUI;
using SFB;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Time = UnityEngine.Time;

namespace Overlayer.Views;

public class SettingsDrawer : ModelDrawable<Settings> {
    public SettingsDrawer(Settings settings) : base(settings) { }

    private enum ExtraMenus {
        Closed,
        Overlayer,
        Scripting,
        Adofai
    }

    private ExtraMenus extraMenu = ExtraMenus.Closed;
    private string[] languages;
    private string[] userLanguages;
    internal bool NeedLangInit = true;

    public static readonly string[] preparingsymbols = { "|", "/", "-", "\\" };
    public float preparinglastUpdateTime;
    public int preparingsymbolIndex;
    public float helptime;

    private void LanguageInit() {
        helptime = 0f;
        preparingsymbolIndex = 0;
        languages = Main.Lang.GetLanguages();
        userLanguages = Main.Lang.GetLanguageNativeNames();
    }

    private void LanguageUpdate(int index) {
        Main.Lang.Language = languages[index];
        model.Lang = Main.Lang.Language;
    }

    public override void OnceCall() {
        NeoDrawer.StaticInstance.FieldResetDictById();
        LanguageInit();
    }

    private bool needCreateNewProfile;

    private int[] dragSoltRange;
    private bool dragSoltNeedInit = true;
    private int dragSoltDragging = -1;
    private int dragSoltInsert = -1;

    public override void Draw() {
        NeoDrawer.StaticInstance.FieldResetId();

        if (Main.Logo != null && !model.DisableLogo) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Logo, GUILayout.Width(Main.Logo.width), GUILayout.Height(Main.Logo.height));
            GUILayout.BeginVertical();
            var v3labelRect =
                GUILayoutUtility.GetRect(new GUIContent("Overlayer v3"), GUI.skin.label, GUILayout.Height(62));
            GUI.Label(v3labelRect, "<size=62>Overlayer v3</size>");
            if (Event.current.type == EventType.MouseDown && v3labelRect.Contains(Event.current.mousePosition) &&
                !Main.EgEnabled) {
                egHandle();
                Event.current.Use();
            }

            GUILayout.Label($"<size=26>{Main.Lang.Get("SLOGAN_TEXT", "Display everything as you wish.")}</size>");
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<size=16>{Main.Mod.Version}, by </size>");
            GUILayout.Label($"<size=16><color=#{Effect.Rainbow(11.0)}>Square3ang</color></size>");
            GUILayout.Label("<size=16> & </size>");
            GUILayout.Label($"<size=16><color=#{Effect.Rainbow(9.0)}>Kkitut</color></size>");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (Main.EgEnabled) Main.Eg.DrawChoices();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        if (Main.Lang.IsLoading) {
            var elapsedTime = Time.time - preparinglastUpdateTime;

            if (elapsedTime >= 0.05f) {
                preparingsymbolIndex++;
                if (preparingsymbolIndex >= preparingsymbols.Length) preparingsymbolIndex = 0;
                preparinglastUpdateTime = Time.time;
            }

            helptime += Time.deltaTime;
            if (helptime >= 8f)
                GUILayout.Label(
                    "Is the Preparing is taking too long?? please get in touch with the developer for assistance!!");
            else
                GUILayout.Label("");
            GUILayout.BeginHorizontal();
            Drawer.Button("Loading translations for you, hang tight...", GUILayout.Width(480));
            GUILayout.Space(10);
            GUILayout.Label(preparingsymbols[preparingsymbolIndex]);
            GUILayout.EndHorizontal();
        }
        else {
            string languageDesc;
            if (Main.Lang.IsDefault) {
                languageDesc = $"! {Translator.FALLBACK_LANGUAGE} by OVERLAYER";
            }
            else {
                var translatorsCount = Main.Lang.GetArrCount("0TRANSLATORS");

                if (translatorsCount > 0) {
                    var names = new List<string>();
                    for (var i = 0; i < translatorsCount; i++)
                        names.Add(Main.Lang.GetArr("0TRANSLATORS", i, "[UNKNOWN]"));
                    var translatorsText = string.Join(" & ", names);
                    languageDesc = $"| {Main.Lang.Get("0NATIVELANG", Main.Lang.Language)} by {translatorsText}";
                }
                else {
                    languageDesc = $"| {Main.Lang.Language}";
                }
            }

            GUILayout.Label($"{Main.Lang.Get("SELECTLANGUAGE", "Select Language")} {languageDesc}");
            if (Main.Lang.IsSomeFail)
                GUILayout.Label(
                    "<color=#FFFF00>Some translations are failed to load, See the log for details.</color>");
            else if (Main.Lang.IsFail)
                GUILayout.Label($"<color=#FF0000>All translations failed to load: {Main.Lang.FailState}</color>");
            GUILayout.BeginHorizontal();
            var selectedIndex = Array.IndexOf(languages, Main.Lang.Language);

            if (Drawer.Button("◀", GUILayout.Width(40))) {
                selectedIndex = (selectedIndex - 1 + languages.Length) % languages.Length;
                LanguageUpdate(selectedIndex);
            }

            if (Drawer.SelectionPopup(ref selectedIndex, userLanguages, "", GUILayout.Width(400)))
                LanguageUpdate(selectedIndex);
            if (Drawer.Button("▶", GUILayout.Width(40))) {
                selectedIndex = (selectedIndex + 1) % languages.Length;
                LanguageUpdate(selectedIndex);
            }

            var reloadLang = false;
            // I LOVE UNITY SO MUCH WTF
            try {
                // F###! WHY 'System.ArgumentException'?????? WHY??????????????
                reloadLang = Drawer.Button(Main.Lang.Get("RELOADLANG", "Reload Language Pack"), GUILayout.Width(320));
            }
            catch { }
            finally {
                GUILayout.EndHorizontal();
            }

            if (reloadLang)
                _ = Task.Run(async () => {
                    await Main.Lang.Load(Path.Combine(Main.Mod.Path, "lang"));
                    NeedLangInit = true;
                });
        }

        GUILayout.BeginHorizontal();
        DrawMenuButton("Overlayer", ExtraMenus.Overlayer);
        DrawMenuButton("Scripting", ExtraMenus.Scripting);
        DrawMenuButton("ADOFAI", ExtraMenus.Adofai);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        switch (extraMenu) {
            case ExtraMenus.Overlayer:
                if (Drawer.DrawBool(
                        string.Format(Main.Lang.Get("DISABLE_THIS", "Disable {0}"), Main.Lang.Get("LOGO", "Logo")),
                        ref model.DisableLogo)) {
                    if (model.DisableLogo)
                        Main.LogoRelease();
                    else
                        Main.LogoInit(Main.Mod.Path);
                }

                Drawer.DrawBool(
                    string.Format(Main.Lang.Get("AUTO_THIS", "Auto {0}"), Main.Lang.Get("UPDATE", "Update")),
                    ref model.AutoUpdate);
                if (model.AutoUpdate) {
                    Drawer.BeginTab();
                    Drawer.DrawBool(
                        string.Format(Main.Lang.Get("ALLOW_THIS", "Allow {0}"),
                            Main.Lang.Get("BETA_TEXT", "Beta version")), ref model.AutoUpdateBeta);
                    Drawer.HoverTooltip(Main.Lang.Get("AUTO_UPDATE_BETA_DESC",
                        "<color=#ff0000>Not recommended.</color>\nUse this only if you are willing to risk <color=#ff0000>file corruption</color>\nor <color=#ff0000>malfunction</color> in order to access the beta version fast"));
                    Drawer.EndTab();
                }

                Drawer.DrawBool(
                    string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), Main.Lang.Get("TOOLTIP", "Tooltip")),
                    ref model.Tooltip);
                Drawer.HoverTooltip(Main.Lang.Get("TOOLTIP_DESC", "You are currently viewing this"));
                if (Drawer.DrawBool(Main.Lang.Get("LEGACY_THEME", "Legacy Theme"), ref model.LegacyTheme)) {
                    Drawer.SetStyle(model.LegacyTheme);
                    RGUIStyle.CreateStyles();
                }

                Drawer.HoverTooltip(Main.Lang.Get("LEGACY_THEME_DESC",
                    "Reverts Overlayer’s custom clean theme back to Unity’s default OnGUI theme"));
                Drawer.DrawBool(string.Format(Main.Lang.Get("AUTO_THIS", "Auto {0}"), Main.Lang.Get("PIVOT", "Pivot")),
                    ref model.AutoPivot);
                Drawer.HoverTooltip(Main.Lang.Get("AUTO_PIVOT_DESC",
                    "Automatically adjusts the pivot to match the text alignment.\nWhen the editor is in Simple mode, this is always enabled regardless of settings"));
                Drawer.DrawBool(Main.Lang.Get("INCLUDE_REFERENCES", "Include References"), ref model.IncludeReferences);
                Drawer.HoverTooltip(Main.Lang.Get("INCLUDE_REFERENCES_DESC",
                    "When this setting is enabled,\nelements that rely on external files(such as fonts and images)\nwill be included directly in the JSON file export.\n\nThis may increase the file size"));
                Drawer.DrawBool(
                    string.Format(Main.Lang.Get("USE_THIS", "Use {0}"),
                        string.Format(Main.Lang.Get("SAFE_COMMAND_PARSE", "Safe Command Parse"))),
                    ref model.SafeCommandParse);
                Drawer.HoverTooltip(Main.Lang.Get("SAFE_COMMAND_PARSE_DESC",
                    "Parses commands safely by catching errors.\nReturns a default value instead of throwing exceptions on failure.\n\nMay hide underlying errors"));
                if (Drawer.DrawBool(
                        string.Format(Main.Lang.Get("USE_THIS", "Use {0}"),
                            string.Format(Main.Lang.Get("FILE_ATTEMPT", "File Attempt"))), ref model.FileAttempt)) {
                    if (model.FileAttempt)
                        SafePatchManager.ApplyPatch(typeof(FileAttempt));
                    else
                        SafePatchManager.RemovePatch(typeof(FileAttempt));
                }

                Drawer.HoverTooltip(Main.Lang.Get("FILE_ATTEMPT_DESC",
                    "You can use FileAttempts & FileTileAttempts Tags when enabled"));
                Drawer.DrawBool(string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), nameof(Effect.MovingMan)),
                    ref model.MovingManEditor);
                Drawer.DrawBool(string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), nameof(Effect.ColorRange)),
                    ref model.ColorRangeEditor);
                Drawer.DrawBool(string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), nameof(Effect.EasedValue)),
                    ref model.EasedValueEditor);
                NeoDrawer.StaticInstance.DrawSingle(Main.Lang.Get("FPS_UPDATE_RATE", "Fps Update Rate"),
                    ref model.FPSUpdateRate);
                NeoDrawer.StaticInstance.DrawSingle(Main.Lang.Get("FRAMETIME_UPDATE_RATE", "FrameTime Update Rate"),
                    ref model.FrameTimeUpdateRate);
                NeoDrawer.StaticInstance.DrawInt32(Main.Lang.Get("SYSTEMTAG_UPDATE_RATE", "System Tag Update Rate"),
                    ref model.SystemTagUpdateRate);
                Drawer.HoverTooltip(Main.Lang.Get("SYSTEMTAG_UPDATE_RATE_DESC", "Such as GC usage Tags"));
                break;
            case ExtraMenus.Scripting:
                Scripting.DrawUI();
                break;
            case ExtraMenus.Adofai:
                if (Drawer.DrawBool(Main.Lang.Get("CHANGE_FONT", "Change Font"), ref model.ChangeFont))
                    if (!model.ChangeFont) {
                        model.AdofaiFont.name = "Default";
                        if (model.AdofaiFont.Apply(out var font)) {
                            FontManager.SetFont(model.AdofaiFont.name, font);
                            RDString.initialized = false;
                            RDString.Setup();
                            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        }
                    }

                if (model.ChangeFont) {
                    Drawer.BeginTab();
                    Drawer.DrawString(Main.Lang.Get("FONT", "Font"), ref model.AdofaiFont.name);
                    NeoDrawer.StaticInstance.DrawSingle(Main.Lang.Get("FONT_SIZE", "Font Scale"),
                        ref model.AdofaiFont.fontScale);
                    NeoDrawer.StaticInstance.DrawSingle(Main.Lang.Get("LINE_SPACING", "Font Line Spacing"),
                        ref model.AdofaiFont.lineSpacing);
                    GUILayout.BeginHorizontal();
                    if (Drawer.Button(Main.Lang.Get("APPLY", "Apply")))
                        if (model.AdofaiFont.Apply(out var font)) {
                            FontManager.SetFont(model.AdofaiFont.name, font);
                            RDString.initialized = false;
                            RDString.Setup();
                            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        }

                    if (Drawer.Button(Main.Lang.Get("LOG_FONT_LIST", "Log Font List")))
                        foreach (var font in FontManager.OSFonts)
                            Main.Logger.Log(font);

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    Drawer.EndTab();
                }

                if (Drawer.DrawBool(
                        string.Format(Main.Lang.Get("USE_THIS", "Use {0}"),
                            string.Format(Main.Lang.Get("SHOW_TRUE_AUTO_JUDGMENT", "Show True Auto Judgment"))),
                        ref model.ShowTrueAutoJudgment)) {
                    if (model.ShowTrueAutoJudgment)
                        SafePatchManager.ApplyPatch(typeof(HitFixPatch));
                    else
                        SafePatchManager.RemovePatch(typeof(HitFixPatch));
                }

                Drawer.HoverTooltip(Main.Lang.Get("SHOW_TRUE_AUTO_JUDGEMENT_DESC",
                    "Patches the in game judgement line code\nso that judgements are always displayed on the Hit Error Meter even during Autoplay"));
                break;
        }

        if (extraMenu != ExtraMenus.Closed) GUILayout.Space(12);

        var old = GUI.color;
        GUILayout.BeginHorizontal();
        if (Drawer.Button(Drawer.Icon_Plus, GUILayout.Width(100))) needCreateNewProfile = true;
        GUI.color = new Color(1f, 1f, 0.8f);
        if (Drawer.Button(Drawer.Icon_Down, GUILayout.Width(60)))
            Task.Run(() => {
                var pfs = StandaloneFileBrowser.OpenFilePanel(
                    Main.Lang.Get("SELECT_PROFILE", "Select Profile"),
                    Main.ProfilePath,
                    [new ExtensionFilter(Main.Lang.Get("OVERLAYER_PROFILE_JSON", "Overlayer Profile JSON"), "json")],
                    true
                );
                foreach (var pf in pfs)
                    try {
                        if (Path.GetExtension(pf) != ".json") continue;
                        var name = Path.GetFileNameWithoutExtension(pf);
                        if (ProfileManager.Exists(name)) continue;
                        var targetPath = Path.Combine(Main.ProfilePath, name + ".json");
                        if (!string.Equals(
                                Path.GetFullPath(pf),
                                Path.GetFullPath(targetPath),
                                StringComparison.OrdinalIgnoreCase
                            ))
                            File.Copy(pf, targetPath, true);
                        var content = File.ReadAllText(targetPath);
                        if (string.IsNullOrWhiteSpace(content)) continue;
                        var token = JToken.Parse(content);
                        var cfg = new ProfileConfig();
                        cfg.Deserialize(token);
                        cfg.Path = targetPath;
                        cfg.Name = name;
                        Main.MainThreadDispatcher.Enqueue(() => {
                            try {
                                var profileGO = new GameObject(cfg.Name ?? "Profile");
                                var profile = profileGO.AddComponent<OverlayerProfile>();
                                profile.Config = cfg;
                                profile.Init(cfg.Name);
                                foreach (var obj in cfg.Objects)
                                    if (obj is TextConfig t)
                                        TextConfigImporter.ImportRef(t, token);
                                    else if (obj is ImageConfig i) ImageConfigImporter.ImportRef(i, token);

                                profile.ObjectManager.Import(cfg.Objects);
                                ProfileManager.Profiles.Add(profile);
                                profile.ObjectManager.Refresh();
                                dragSoltNeedInit = true;
                            }
                            catch (Exception ex) {
                                Debug.LogError($"Failed to create profile '{pf}' on main thread: {ex}");
                            }
                        });
                    }
                    catch (Exception e) {
                        Debug.LogError($"Failed to load profile '{pf}': {e}");
                    }
            });
        GUI.color = old;
        if (Drawer.Button(Drawer.Icon_OpenFolder, GUILayout.Width(80)))
            Application.OpenURL(Path.GetFullPath(Main.Mod.Path));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        var isRepaint = Event.current.type == EventType.Repaint;
        if (dragSoltNeedInit) {
            dragSoltNeedInit = false;
            dragSoltRange = new int[ProfileManager.Profiles.Count];
        }

        for (var i = 0; i < ProfileManager.Profiles.Count; i++) {
            var profile = ProfileManager.Get(i);
            if (profile == null) {
                GUILayout.Label($"[{Main.Lang.Get("ERROR", "Error")}] " + string.Format(
                    Main.Lang.Get("ERROR_THIS_PROFILE_INDEX", "Unable to load profile data at index {0}"),
                    i.ToString()));
                continue;
            }

            if (i != dragSoltDragging) {
                if (i == dragSoltInsert) {
                    GUILayout.BeginHorizontal();
                    var dummy = false;
                    var oldd = GUI.color;
                    GUI.color = Color.black;
                    Drawer.DrawOnlyBool(ref dummy);
                    GUI.color = oldd;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                if (Drawer.DrawOnlyBool(ref profile.Config.Active)) profile.gameObject.SetActive(profile.Config.Active);
                GUILayout.Label("-==-", GUI.skin.label);
                if (dragSoltDragging < 0 && Event.current.type == EventType.MouseDown &&
                    GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                    dragSoltDragging = i;
                    dragSoltInsert = i;
                }

                GUILayout.Space(6);
                GUI.color = profile.Config.Active ? new Color(0.8f, 0.8f, 1f) : Color.gray;
                GUI.enabled = profile.Config.Active;
                if (Drawer.Button(Drawer.Icon_Pencil, GUILayout.Width(80))) Main.GUI.Push(new ProfileDrawer(profile));
                GUI.enabled = true;
                GUI.color = new Color(1f, 0.8f, 1f);
                if (Drawer.Button(Drawer.Icon_Up, GUILayout.Width(46)))
                    Task.Run(() => {
                        var target = StandaloneFileBrowser.SaveFilePanel(
                            Main.Lang.Get("EXPORT_PROFILE", "Export Profile"),
                            Persistence.GetLastUsedFolder(),
                            $"{profile.Config.Name}.json",
                            "json"
                        );
                        if (!string.IsNullOrWhiteSpace(target)) {
                            var node = profile.Config.Serialize();
                            if (model.IncludeReferences) node["References"] = ProfileReferences.GetReferences(profile);
                            File.WriteAllText(target, node.ToString());
                        }
                    });
                GUI.color = new Color(1f, 0.8f, 0.8f);
                if (Drawer.Button(Drawer.Icon_X, GUILayout.Width(46))) {
                    if (Event.current.shift) {
                        ProfileManager.Destroy(profile);
                    }
                    else {
                        if (Object.FindAnyObjectByType<DeletePopup>() == null) {
                            var popup = new GameObject().AddComponent<DeletePopup>();
                            Object.DontDestroyOnLoad(popup);
                            popup.Initialize(profile, () => {
                                ProfileManager.Destroy(profile);
                                dragSoltNeedInit = true;
                            });
                        }
                    }

                    return;
                }

                GUI.color = old;
                GUILayout.Label(profile.Config.Name);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            if (Event.current.type == EventType.Repaint)
                dragSoltRange[i] = Mathf.RoundToInt(GUILayoutUtility.GetLastRect().y);
        }

        if (dragSoltInsert == ProfileManager.Count) {
            GUILayout.BeginHorizontal();
            var dummy = false;
            var oldd = GUI.color;
            GUI.color = Color.black;
            Drawer.DrawOnlyBool(ref dummy);
            GUI.color = oldd;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        if (dragSoltDragging >= 0) {
            if (Event.current.type == EventType.MouseUp) {
                ProfileManager.OrderByDrag(dragSoltDragging, dragSoltInsert);

                dragSoltDragging = -1;
                dragSoltInsert = -1;

                GUILayout.BeginArea(Rect.zero);
                GUILayout.BeginHorizontal();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
            else {
                if (isRepaint) {
                    var insertIndex = -1;
                    for (var i = 0; i < dragSoltRange.Length; i++) {
                        if (Event.current.mousePosition.y - 14 <= dragSoltRange[i]) {
                            insertIndex = i;
                            break;
                        }

                        if (i == dragSoltRange.Length - 1) {
                            insertIndex = dragSoltRange.Length;
                            break;
                        }
                    }

                    dragSoltInsert = insertIndex;
                }

                float dragWidth = Screen.width;
                float dragHeight = 24;
                Rect dragRect = new(
                    GUILayoutUtility.GetLastRect().x,
                    Event.current.mousePosition.y - dragHeight * 3,
                    dragWidth,
                    dragHeight
                );

                var dpf = ProfileManager.Get(dragSoltDragging);

                GUILayout.BeginArea(dragRect);
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                var dmyActive = dpf.Config.Active;
                Drawer.DrawOnlyBool(ref dmyActive);
                GUILayout.Label("-==-", GUI.skin.label);
                GUILayout.Space(6);
                GUI.color = new Color(0.8f, 0.8f, 1f);
                Drawer.ButtonDummy(Drawer.Icon_Pencil, GUILayout.Width(80));
                GUI.color = new Color(1f, 0.8f, 1f);
                Drawer.ButtonDummy(Drawer.Icon_Up, GUILayout.Width(46));
                GUI.color = new Color(1f, 0.8f, 0.8f);
                Drawer.ButtonDummy(Drawer.Icon_X, GUILayout.Width(46));
                GUI.color = old;
                GUILayout.Label(dpf.name);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }
        else {
            GUILayout.BeginArea(Rect.zero);
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        if (NeedLangInit) {
            NeedLangInit = false;
            languages = null;
            userLanguages = null;
            LanguageInit();
        }

        if (needCreateNewProfile) {
            needCreateNewProfile = false;

            Directory.CreateDirectory(Main.ProfilePath);

            var i = 1;
            string name;
            do {
                name = $"Profile {i}";
                i++;
            } while (ProfileManager.Profiles.Any(p => string.Equals(p.Config.Name, name, StringComparison.Ordinal)));

            var path = Path.Combine(Main.ProfilePath, name + ".json");

            var config = new ProfileConfig {
                Name = name,
                Path = name + ".json"
            };

            File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));

            var profile = ProfileManager.Create(config);

            dragSoltNeedInit = true;
        }

        NeoDrawer.StaticInstance.UpdateFocused();
    }

    private void DrawMenuButton(string label, ExtraMenus menu) {
        var prev = extraMenu;
        if (Drawer.Button(label + (extraMenu == menu ? "▼" : "▲"))) {
            extraMenu = extraMenu == menu ? ExtraMenus.Closed : menu;
            if (extraMenu != prev) NeoDrawer.StaticInstance.FieldClear();
        }
    }

    private int egClickCount;
    private DateTime egFirstClickTime = DateTime.MinValue;

    private void egHandle() {
        if (Main.EgEnabled) return;
        var now = DateTime.UtcNow;

        if (egClickCount == 0) egFirstClickTime = now;

        egClickCount++;

        if ((now - egFirstClickTime).TotalSeconds > 1.0) {
            egClickCount = 1;
            egFirstClickTime = now;
        }

        if (egClickCount >= 10) {
            Main.EgEnabled = true;
            egClickCount = 0;
        }
    }
}