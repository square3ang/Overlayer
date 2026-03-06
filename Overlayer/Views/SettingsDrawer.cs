using Newtonsoft.Json.Linq;
using Overlayer.Core;
using Overlayer.Core.Patches;
using Overlayer.Core.Translation;
using Overlayer.Models;
using Overlayer.Utils;
using RapidGUI;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Overlayer.Patches.HitFixPatch;
using Object = UnityEngine.Object;

namespace Overlayer.Views;

public class SettingsDrawer : ModelDrawable<Settings> {
    public SettingsDrawer(Settings settings) : base(settings) { }

    private enum ExtraMenus {
        Closed,
        Extra,
    }

    private ExtraMenus extraMenu = ExtraMenus.Closed;
    private string[] languages;
    private string[] userLanguages;
    internal bool NeedLangInit = true;

    public readonly static string[] preparingsymbols = { "|", "/", "-", "\\" };
    public float preparinglastUpdateTime = 0f;
    public int preparingsymbolIndex = 0;
    public float helptime = 0f;

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

    private int[] dragSoltRange;
    private bool dragSoltNeedInit = true;
    private int dragSoltDragging = -1;
    private int dragSoltInsert = -1;

    public override void OnceCall() {
        NeoDrawer.StaticInstance.FieldResetDictById();
        LanguageInit();
    }
    public override void Draw() {
        NeoDrawer.StaticInstance.FieldResetId();

        if(Main.Logo != null && !model.disableLogo) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Logo, GUILayout.Width(Main.Logo.width), GUILayout.Height(Main.Logo.height));
            GUILayout.BeginVertical();
            Rect v3labelRect = GUILayoutUtility.GetRect(new GUIContent("Overlayer v3"), GUI.skin.label, GUILayout.Height(62));
            GUI.Label(v3labelRect, "<size=62>Overlayer v3</size>");
            if(Event.current.type == EventType.MouseDown && v3labelRect.Contains(Event.current.mousePosition) && !Main.EgEnabled) {
                egHandle();
                Event.current.Use();
            }
            GUILayout.Label($"<size=26>{Main.Lang.Get("SLOGAN_TEXT", "Display everything as you wish.")}</size>");
            GUILayout.Label($"<size=16>{Main.Mod.Version}, by <color=#{Tags.Effect.Rainbow()}>Square3ang & Kkitut</color></size>");
            if(Main.EgEnabled) {
                Main.Eg.DrawChoices();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        if(Main.Lang.IsLoading) {
            float elapsedTime = Time.time - preparinglastUpdateTime;

            if(elapsedTime >= 0.05f) {
                preparingsymbolIndex++;
                if(preparingsymbolIndex >= preparingsymbols.Length) {
                    preparingsymbolIndex = 0;
                }
                preparinglastUpdateTime = Time.time;
            }

            helptime += Time.deltaTime;
            if(helptime >= 8f) {
                GUILayout.Label("Is the Preparing is taking too long?? please get in touch with the developer for assistance!!");
            } else {
                GUILayout.Label("");
            }
            GUILayout.BeginHorizontal();
            Drawer.Button("Loading translations for you, hang tight...", GUILayout.Width(480));
            GUILayout.Space(10);
            GUILayout.Label(preparingsymbols[preparingsymbolIndex]);
            GUILayout.EndHorizontal();
        } else {
            string languageDesc;
            if(Main.Lang.IsDefault) {
                languageDesc = $"! {Translator.FALLBACK_LANGUAGE} by OVERLAYER";
            } else {
                int translatorsCount = Main.Lang.GetArrCount("0TRANSLATORS");

                if(translatorsCount > 0) {
                    var names = new List<string>();
                    for(int i = 0; i < translatorsCount; i++) {
                        names.Add(Main.Lang.GetArr("0TRANSLATORS", i, "[UNKNOWN]"));
                    }
                    string translatorsText = string.Join(" & ", names);
                    languageDesc = $"| {Main.Lang.Get("0NATIVELANG", Main.Lang.Language)} by {translatorsText}";
                } else {
                    languageDesc = $"| {Main.Lang.Language}";
                }
            }

            GUILayout.Label($"{Main.Lang.Get("SELECTLANGUAGE", "Select Language")} {languageDesc}");
            if(Main.Lang.IsSomeFail) {
                GUILayout.Label("<color=#FFFF00>Some translations are failed to load, See the log for details.</color>");
            } else if(Main.Lang.IsFail) {
                GUILayout.Label($"<color=#FF0000>All translations failed to load: {Main.Lang.FailState}</color>");
            }
            GUILayout.BeginHorizontal();
            int selectedIndex = Array.IndexOf(languages, Main.Lang.Language);

            if(Drawer.Button("◀", GUILayout.Width(40))) {
                selectedIndex = (selectedIndex - 1 + languages.Length) % languages.Length;
                LanguageUpdate(selectedIndex);
            }

            if(Drawer.SelectionPopup(ref selectedIndex, userLanguages, "", GUILayout.Width(400))) {
                LanguageUpdate(selectedIndex);
            }
            if(Drawer.Button("▶", GUILayout.Width(40))) {
                selectedIndex = (selectedIndex + 1) % languages.Length;
                LanguageUpdate(selectedIndex);
            }

            bool reloadLang = false;
            // I LOVE UNITY SO MUCH WTF
            try {
                // F###! WHY 'System.ArgumentException'?????? WHY??????????????
                reloadLang = Drawer.Button(Main.Lang.Get("RELOADLANG", "Reload Language Pack"), GUILayout.Width(320));
            } catch {
            } finally {
                GUILayout.EndHorizontal();
            }

            if(reloadLang) {
                _ = Task.Run(async () => {
                    await Main.Lang.Load(Path.Combine(Main.Mod.Path, "lang"));
                    NeedLangInit = true;
                });
            }
        }
        GUILayout.BeginHorizontal();
        if(Drawer.Button(Main.Lang.Get("EXTRA_MENU", "Extra Menu") + " " + (extraMenu == ExtraMenus.Extra ? "▼" : "▲"))) {
            extraMenu = extraMenu == ExtraMenus.Extra ? ExtraMenus.Closed : ExtraMenus.Extra;
        }
        /*
        if(Drawer.Button(Main.Lang.Get("OPEN_WIKI_MENU","Open Wiki Menu"))) {
            if(Main.Wiki == null) {
                Main.Wiki = new GameObject().AddComponent<Wiki.Wiki>();
                UnityEngine.Object.DontDestroyOnLoad(Main.Wiki);
            } else {
                Main.Wiki.BringToFrontOnce();
            }
        }
        */
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        switch(extraMenu) {
            case ExtraMenus.Extra:
                if(Drawer.DrawBool(string.Format(Main.Lang.Get("DISABLE_THIS", "Disable {0}"), Main.Lang.Get("LOGO", "Logo")), ref model.disableLogo)) {
                    if(model.disableLogo) {
                        Main.LogoRelease();
                    } else {
                        Main.LogoInit(Main.Mod.Path);
                    }
                }
                if(Drawer.DrawBool(Main.Lang.Get("CHANGE_FONT", "Change Font"), ref model.ChangeFont)) {
                    if(!model.ChangeFont) {
                        model.AdofaiFont.name = "Default";
                        if(model.AdofaiFont.Apply(out var font)) {
                            FontManager.SetFont(model.AdofaiFont.name, font);
                            RDString.initialized = false;
                            RDString.Setup();
                            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        }
                    }
                }
                if(model.ChangeFont) {
                    GUILayoutEx.BeginIndent();
                    Drawer.DrawString(Main.Lang.Get("FONT", "Font"), ref model.AdofaiFont.name);
                    Drawer.DrawSingle(Main.Lang.Get("FONT_SCALE", "Font Scale"), ref model.AdofaiFont.fontScale);
                    Drawer.DrawSingle(Main.Lang.Get("LINE_SPACING", "Font Line Spacing"), ref model.AdofaiFont.lineSpacing);
                    GUILayout.BeginHorizontal();
                    if(Drawer.Button(Main.Lang.Get("APPLY", "Apply"))) {
                        if(model.AdofaiFont.Apply(out var font)) {
                            FontManager.SetFont(model.AdofaiFont.name, font);
                            RDString.initialized = false;
                            RDString.Setup();
                            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        }
                    }
                    if(Drawer.Button(Main.Lang.Get("LOG_FONT_LIST", "Log Font List"))) {
                        foreach(var font in FontManager.OSFonts) {
                            Main.Logger.Log(font);
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayoutEx.EndIndent();
                }

                Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("AUTO_THIS", "Auto {0}"), Main.Lang.Get("UPDATE", "Update"))), ref model.useAutoUpdate);
                if(model.useAutoUpdate) {
                    GUILayoutEx.BeginIndent();
                    Drawer.DrawBool(string.Format(Main.Lang.Get("ALLOW_THIS", "Allow {0}"), Main.Lang.Get("BETA_TEXT", "Beta version")), ref model.useAutoUpdateBeta);
                    GUILayoutEx.EndIndent();
                }
                Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), Main.Lang.Get("TOOLTIP", "Tooltip")), ref model.useTooltip);
                if(Drawer.DrawBool(
                        string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), Main.Lang.Get("LEGACY_THEME", "Legacy Theme")),
                        ref model.useLegacyTheme)) {
                    Drawer.SetStyle(model.useLegacyTheme);
                    RGUIStyle.CreateStyles();
                }
                if(Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("SHOW_TRUE_AUTO_JUDGMENT", "Show True Auto Judgment"))), ref model.useShowTrueAutoJudgment)) {
                    LazyPatchManager.Unpatch(typeof(ChangeAddHit), true);
                    LazyPatchManager.Patch(typeof(ChangeAddHit));
                }
                Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("AUTO_THIS", "Auto {0}"), Main.Lang.Get("PIVOT", "Pivot"))), ref model.autoPivot);
                Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "MovingMan")), ref model.useMovingManEditor);
                Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "ColorRange")), ref model.useColorRangeEditor);
                Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "EasedValue")), ref model.useEasedValueEditor);
                NeoDrawer.StaticInstance.DrawSingle(Main.Lang.Get("FPS_UPDATE_RATE", "Fps Update Rate"), ref model.FPSUpdateRate);
                NeoDrawer.StaticInstance.DrawSingle(Main.Lang.Get("FRAMETIME_UPDATE_RATE", "FrameTime Update Rate"), ref model.FrameTimeUpdateRate);
                NeoDrawer.StaticInstance.DrawInt32(Main.Lang.Get("SYSTEMTAG_UPDATE_RATE", "System Tag Update Rate"), ref model.SystemTagUpdateRate);
                break;
        }
        GUILayout.BeginHorizontal();
        bool needCreateNewText = Drawer.Button("+ " + Main.Lang.Get("NEW_TEXT", "Create New Text"));
        if(Drawer.Button(Main.Lang.Get("IMPORT_TEXT", "Import Text"))) {
            var texts = StandaloneFileBrowser.OpenFilePanel(
                Main.Lang.Get("SELECT_TEXT", "Select Text"),
                Main.Mod.Path,
                new[] { new ExtensionFilter("Text", "json") },
                true
            );

            foreach(var text in texts) {
                var json = JToken.Parse(File.ReadAllText(text));
                if(json is JArray arr) {
                    ModelUtils.UnwrapList<TextConfig>(arr).ForEach(t => TextManager.CreateText(t));
                } else if(json is JObject obj) {
                    TextManager.CreateText(TextConfigImporter.Import(obj));
                    dragSoltNeedInit = true;
                }
            }
            TextManager.Refresh();
        }
        string showAs = model.showTextNameAsDisplayText
            ? Main.Lang.Get("TEXT_SHOW_AS_DISPLAY", "Show As <color=#808080>Name</color> / Display Text")
            : Main.Lang.Get("TEXT_SHOW_AS_NAME", "Show As Name / <color=#808080>Display Text</color>");
        if(Drawer.Button(showAs)) {
            model.showTextNameAsDisplayText = !model.showTextNameAsDisplayText;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(TextManager.Initialized) {
            bool isRepaint = Event.current.type == EventType.Repaint;
            if(dragSoltNeedInit) {
                dragSoltNeedInit = false;
                dragSoltRange = new int[TextManager.Count];
            }

            for(int i = 0; i < TextManager.Count; i++) {
                var text = TextManager.Get(i);
                if(text == null) {
                    GUILayout.Label($"[{Main.Lang.Get("ERROR", "Error")}] " + string.Format(Main.Lang.Get("ERROR_THIS_TEXT_INDEX", "Unable to load text data at index {0}"), i.ToString()));
                    continue;
                }

                if(i != dragSoltDragging) {
                    if(i == dragSoltInsert) {
                        GUILayout.BeginHorizontal();
                        bool dummy = false;
                        Color oldd = GUI.color;
                        GUI.color = Color.black;
                        Drawer.DrawOnlyBool(ref dummy);
                        GUI.color = oldd;
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.BeginHorizontal();
                    if(Drawer.DrawOnlyBool(ref text.Config.Active)) {
                        text.gameObject.SetActive(text.Config.Active);
                    }
                    GUILayout.Label("-==-", GUI.skin.label);
                    if(dragSoltDragging < 0 && Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                        dragSoltDragging = i;
                        dragSoltInsert = i;
                    }
                    Color old = GUI.color;
                    GUILayout.Space(6);
                    GUI.color = new Color(0.8f, 0.8f, 1f);
                    if(Drawer.Button(Drawer.icon_Pencil, GUILayout.Width(46))) {
                        Main.GUI.Push(new TextConfigDrawer(text.Config));
                    }
                    GUI.color = new Color(0.8f, 1f, 0.8f);
                    if(Drawer.Button(Drawer.icon_Copy, GUILayout.Width(46))) {
                        TextManager.CreateText(text.Config.Copy());
                        dragSoltNeedInit = true;
                    }
                    GUI.color = new Color(1f, 0.8f, 0.8f);
                    if(Drawer.Button(Drawer.icon_X, GUILayout.Width(46))) {
                        if(Event.current.shift) {
                            TextManager.DestroyText(text);
                        } else {
                            if(Object.FindAnyObjectByType<DeletePopup>() == null) {
                                var popup = new GameObject().AddComponent<DeletePopup>();
                                UnityEngine.Object.DontDestroyOnLoad(popup);
                                popup.Initialize(text, () => dragSoltNeedInit = true);
                            }
                        }
                        return;
                    }
                    GUI.color = old;
                    string textName;
                    if(model.showTextNameAsDisplayText) {
                        if(text.Config.Active) {
                            string current = text.GetCurrentText();
                            textName = current?.BreakRichTag();
                            if(string.IsNullOrEmpty(textName)) {
                                textName = Main.Lang.Get("TEXT_EMPTY", "<color=#808080>[ empty ]</color>");
                            }
                            textName = textName.Replace('\n', ' ');
                            if(textName?.Length > 62) {
                                textName = textName.Substring(0, 62) + $"<color=#808080>..({textName.Length - 62})</color>"; //TODO: Scroling Name
                            }
                        } else {
                            textName = Main.Lang.Get("TEXT_INACTIVE", "<i><color=#808080>[ inactive ]</color></i>");
                        }
                    } else {
                        textName = text.Config.Active ? text.Config.Name : $"<color=#808080>{text.Config.Name}</color>";
                    }
                    GUILayout.Label(textName);

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                if(Event.current.type == EventType.Repaint) {
                    dragSoltRange[i] = Mathf.RoundToInt(GUILayoutUtility.GetLastRect().y);
                }
            }

            if(dragSoltInsert == TextManager.Count) {
                GUILayout.BeginHorizontal();
                bool dummy = false;
                Color oldd = GUI.color;
                GUI.color = Color.black;
                Drawer.DrawOnlyBool(ref dummy);
                GUI.color = oldd;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            if(dragSoltDragging >= 0) {
                if(Event.current.type == EventType.MouseUp) {
                    TextManager.MoveTextByDrag(dragSoltDragging, dragSoltInsert);

                    dragSoltDragging = -1;
                    dragSoltInsert = -1;

                    GUILayout.BeginArea(Rect.zero);
                    GUILayout.BeginHorizontal();
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                } else {
                    if(isRepaint) {
                        int insertIndex = -1;
                        for(int i = 0; i < dragSoltRange.Length; i++) {
                            if(Event.current.mousePosition.y - 14 <= dragSoltRange[i]) {
                                insertIndex = i;
                                break;
                            } else if(i == dragSoltRange.Length - 1) {
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
                        Event.current.mousePosition.y - (dragHeight * 3),
                        dragWidth,
                        dragHeight
                    );

                    var dtxt = TextManager.Get(dragSoltDragging);

                    GUILayout.BeginArea(dragRect);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    bool dmyActive = dtxt.Config.Active;
                    Drawer.DrawOnlyBool(ref dmyActive);
                    GUILayout.Label("-==-", GUI.skin.label);
                    Color old = GUI.color;
                    GUILayout.Space(6);
                    GUI.color = new Color(0.8f, 0.8f, 1f);
                    Drawer.ButtonDummy(Drawer.icon_Pencil, GUILayout.Width(46));
                    GUI.color = new Color(0.8f, 1f, 0.8f);
                    Drawer.ButtonDummy(Drawer.icon_Copy, GUILayout.Width(46));
                    GUI.color = new Color(1f, 0.8f, 0.8f);
                    Drawer.ButtonDummy(Drawer.icon_X, GUILayout.Width(46));
                    GUI.color = old;
                    string textName;
                    if(model.showTextNameAsDisplayText) {
                        if(dtxt.Config.Active) {
                            string current = dtxt.GetCurrentText();
                            textName = current?.BreakRichTag();
                            if(string.IsNullOrEmpty(textName)) {
                                textName = Main.Lang.Get("TEXT_EMPTY", "<color=#808080>[ empty ]</color>");
                            }
                            textName = textName.Replace('\n', ' ');
                            if(textName?.Length > 62) {
                                textName = textName.Substring(0, 62) + $"<color=#808080>..({textName.Length - 62})</color>";
                            }
                        } else {
                            textName = Main.Lang.Get("TEXT_INACTIVE", "<i><color=#808080>[ inactive ]</color></i>");
                        }
                    } else {
                        textName = dtxt.Config.Active ? dtxt.Config.Name : $"<color=#808080>{dtxt.Config.Name}</color>";
                    }
                    GUILayout.Label(textName);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
            } else {
                GUILayout.BeginArea(Rect.zero);
                GUILayout.BeginHorizontal();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            if(NeedLangInit) {
                NeedLangInit = false;
                languages = null;
                userLanguages = null;
                LanguageInit();
            }
            if(needCreateNewText) {
                TextManager.CreateText(new TextConfig());
                TextManager.Refresh();
                dragSoltNeedInit = true;
            }
        }
        NeoDrawer.StaticInstance.UpdateFocused();
    }

    private int egClickCount = 0;
    private DateTime egFirstClickTime = DateTime.MinValue;

    private void egHandle() {
        if(Main.EgEnabled) {
            return;
        }
        DateTime now = DateTime.UtcNow;

        if(egClickCount == 0) {
            egFirstClickTime = now;
        }

        egClickCount++;

        if((now - egFirstClickTime).TotalSeconds > 1.0) {
            egClickCount = 1;
            egFirstClickTime = now;
        }

        if(egClickCount >= 10) {
            Main.EgEnabled = true;
            egClickCount = 0;
        }
    }
}
