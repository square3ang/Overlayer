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

namespace Overlayer.Views
{
    public class SettingsDrawer : ModelDrawable<Settings>
    {
        public SettingsDrawer(Settings settings) : base(settings) { }

        private enum ExtraMenus {
            Closed,
            Extra,
            Tag,
        }

        private ExtraMenus extraMenu = ExtraMenus.Closed;
        private string[] languages;
        private string[] userLanguages;
        internal bool NeedLangInit = true;

        public static float preparinglastUpdateTime = 0f;
        public static string[] preparingsymbols = { "|", "/", "-", "\\" };
        public static int preparingsymbolIndex = 0;
        public static float helptime = 0f;

        private void languageInit() {
            helptime = 0f;
            preparingsymbolIndex = 0;
            languages = Main.Lang.GetLanguages();
            userLanguages = Main.Lang.GetLanguageNativeNames();
        }

        private void languageUpdate(int index) {
            Main.Lang.Language = languages[index];
            model.Lang = Main.Lang.Language;
        }

        public override void OnceCall() {
            NeoDrawer.StaticInstance.FieldResetDictById();
            languageInit();
        }
        public override void Draw()
        {
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
                GUILayout.Label($"<size=16>{Main.Mod.Version}, by <color=#{Tags.Effect.Rainbow()}>Square & Kkitut</color></size>");
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
                    languageUpdate(selectedIndex);
                }

                if(Drawer.SelectionPopup(ref selectedIndex, userLanguages, "", GUILayout.Width(400))) {
                    languageUpdate(selectedIndex);
                }
                if(Drawer.Button("▶", GUILayout.Width(40))) {
                    selectedIndex = (selectedIndex + 1) % languages.Length;
                    languageUpdate(selectedIndex);
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
            if(Drawer.Button(Main.Lang.Get("EXTRA_MENU","Extra Menu") + " " + (extraMenu == ExtraMenus.Extra ? "▼" : "▲"))) {
                if(extraMenu == ExtraMenus.Extra) {
                    extraMenu = ExtraMenus.Closed;
                } else {
                    extraMenu = ExtraMenus.Extra;
                }
            }
            if(Drawer.Button(Main.Lang.Get("TAG_MENU", "Tag Menu") + " " + (extraMenu == ExtraMenus.Tag ? "▼" : "▲"))) {
                if(extraMenu == ExtraMenus.Tag) {
                    extraMenu = ExtraMenus.Closed;
                } else {
                    extraMenu = ExtraMenus.Tag;
                }
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
                            foreach(var font in FontManager.OSFonts)
                                Main.Logger.Log(font);
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
                    Drawer.DrawBool(
                        string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), Main.Lang.Get("LEGACY_NUMBER_FIELD", "Legacy Number Field")),
                        ref model.useLegacyNumberField);
                    if(Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("SHOW_TRUE_AUTO_JUDGMENT", "Show True Auto Judgment"))), ref model.useShowTrueAutoJudgment)) {
                        LazyPatchManager.Unpatch(typeof(ChangeAddHit), true);
                        LazyPatchManager.Patch(typeof(ChangeAddHit));
                    }
                    Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "MovingMan")), ref model.useMovingManEditor);
                    Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "ColorRange")), ref model.useColorRangeEditor);
                    Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "EasedValue")), ref model.useEasedValueEditor);
                    Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("AUTO_THIS", "Auto {0}"), Main.Lang.Get("PIVOT", "Pivot"))), ref model.autoPivot);
                    if(Main.Settings.useLegacyNumberField) {
                        Drawer.DrawSingle(Main.Lang.Get("FPS_UPDATE_RATE", "Fps Update Rate"), ref model.FPSUpdateRate);
                        Drawer.DrawSingle(Main.Lang.Get("FRAMETIME_UPDATE_RATE", "FrameTime Update Rate"), ref model.FrameTimeUpdateRate);
                        Drawer.DrawInt32(Main.Lang.Get("SYSTEMTAG_UPDATE_RATE", "System Tag Update Rate"), ref model.SystemTagUpdateRate);
                    } else {
                        NeoDrawer.StaticInstance.DrawSingle(Main.Lang.Get("FPS_UPDATE_RATE", "Fps Update Rate"), ref model.FPSUpdateRate);
                        NeoDrawer.StaticInstance.DrawSingle(Main.Lang.Get("FRAMETIME_UPDATE_RATE", "FrameTime Update Rate"), ref model.FrameTimeUpdateRate);
                        NeoDrawer.StaticInstance.DrawInt32(Main.Lang.Get("SYSTEMTAG_UPDATE_RATE", "System Tag Update Rate"), ref model.SystemTagUpdateRate);
                    }
                    break;
                case ExtraMenus.Tag:
                    Drawer.DrawBool(Main.Lang.Get("LEVEL_NAME_TEXT_UPDATE_ALWAYS", "Always update {LevelNameText} and {LevelNameTextRaw} (for ADOFAI Tweaks 'Hide song artist and title' setting)"), ref model.tagLevelNameTextUpdateAlways);
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
                for(int i = 0; i < TextManager.Count; i++) {
                    var text = TextManager.Get(i);

                    GUILayout.BeginHorizontal();
                    if(Drawer.DrawOnlyBool(ref text.Config.Active)) {
                        text.gameObject.SetActive(text.Config.Active);
                    }
                    Color old = GUI.color;
                    GUI.color = (i <= 0) ? Color.gray : Color.white;
                    string upSymbol = (i <= 0) ? "△" : "▲";
                    if(Drawer.Button(upSymbol, GUILayout.Width(38))) {
                        if(Event.current.shift) {
                            TextManager.MoveTextToTop(i);
                        } else if(i > 0) {
                            TextManager.MoveTextUp(i);
                        }
                    }
                    GUI.color = (i >= TextManager.Count - 1) ? Color.gray : Color.white;
                    string downSymbol = (i >= TextManager.Count - 1) ? "▽" : "▼";
                    if(Drawer.Button(downSymbol, GUILayout.Width(38))) {
                        if(Event.current.shift) {
                            TextManager.MoveTextToBottom(i);
                        } else if(i < TextManager.Count - 1) {
                            TextManager.MoveTextDown(i);
                        }
                    }
                    GUI.color = Color.white;
                    if(text == null) {
                        GUILayout.Label($"[{Main.Lang.Get("ERROR", "Error")}] " + string.Format(Main.Lang.Get("ERROR_THIS_TEXT_INDEX", "Unable to load text data at index {0}"), i.ToString()));
                        continue;
                    }
                    GUILayout.Space(6);
                    GUI.color = new Color(0.8f, 0.8f, 1f);
                    if(Drawer.Button(Main.Lang.Get("EDIT", "Edit"))) {
                        TextConfigDrawer config = new TextConfigDrawer(text.Config);
                        Main.GUI.Push(config);
                    }
                    GUI.color = new Color(0.8f, 1f, 0.8f);
                    if(Drawer.Button(Main.Lang.Get("CLONE", "Clone"))) {
                        TextManager.CreateText(text.Config.Copy());
                    }
                    GUI.color = new Color(1f, 0.8f, 0.8f);
                    if(Drawer.Button(Main.Lang.Get("DESTROY", "Destroy"))) {
                        if(Event.current.shift) {
                            TextManager.DestroyText(text);
                        } else {
                            if(Object.FindAnyObjectByType<DeletePopup>() == null) {
                                var popup = new GameObject().AddComponent<DeletePopup>();
                                UnityEngine.Object.DontDestroyOnLoad(popup);
                                popup.Initialize(text);
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
                            } else if(textName?.Length > 62) {
                                textName = textName.Substring(0, 62) + $"<color=#808080>..({textName.Length - 62})</color>";
                            }
                        } else {
                            textName = Main.Lang.Get("TEXT_INACTIVE", "<i><color=#808080>[ inactive ]</color></i>");
                        }
                    } else {
                        if(text.Config.Active) {
                            textName = text.Config.Name;
                        } else {
                            textName = $"<color=#808080>{text.Config.Name}</color>";
                        }
                    }
                    GUILayout.Label(textName);

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }

                if(NeedLangInit) {
                    NeedLangInit = false;
                    languages = null;
                    userLanguages = null;
                    languageInit();
                }
                if(needCreateNewText) {
                    TextManager.CreateText(new TextConfig());
                    TextManager.Refresh();
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

            if(egClickCount >= 7) {
                Main.EgEnabled = true;
                egClickCount = 0;
            }
        }
    }
}
