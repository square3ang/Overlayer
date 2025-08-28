using JSON;
using Newtonsoft.Json;
using Overlayer.Core;
using Overlayer.Core.Patches;
using Overlayer.Core.Translation;
using Overlayer.Core.Translatior;
using Overlayer.Models;
using Overlayer.Tags.Patches;
using Overlayer.Utils;
using RapidGUI;
using SA.GoogleDoc;
using SFB;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using static Overlayer.Patches.HitFixPatch;
using static UnityModManagerNet.UnityModManager;
using Object = UnityEngine.Object;

namespace Overlayer.Views
{
    public class SettingsDrawer : ModelDrawable<Settings>
    {
        public SettingsDrawer(Settings settings) : base(settings) { }

        private bool isOpenedExtraMenu = false;
        public override void Draw()
        {
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
                GUILayout.Label($"<size=16>{Main.ModVersion}, by <color=#{Tags.Effect.Rainbow()}>Square & Kkitut</color></size>");
                if(Main.EgEnabled) {
                    Main.Eg.DrawChoices();
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            if(model != null)
                Main.Lang.CurrentLanguage = model.Lang;
            GUILayout.Label($"{Main.Lang.Get("SELECTLANGUAGE","Select Language")} | {Main.Lang.CurrentLanguage} by {Main.Lang.Get("0TRANSLATOR", "[UNKNOWN]")}");
            GUILayout.BeginHorizontal();
            string[] languageNames = Main.Lang.GetLanguages();
            int selectedIndex = Array.IndexOf(languageNames,Main.Lang.CurrentLanguage);

            if(Drawer.Button("◀",GUILayout.Width(40)))
            {
                selectedIndex = (selectedIndex - 1 + languageNames.Length) % languageNames.Length;
                UpdateLanguageSetting(selectedIndex);
            }

            if(Drawer.SelectionPopup(ref selectedIndex,languageNames, "", GUILayout.Width(400)))
            {
                UpdateLanguageSetting(selectedIndex);
            }
            if(Drawer.Button("▶",GUILayout.Width(40)))
            {
                selectedIndex = (selectedIndex + 1) % languageNames.Length;
                UpdateLanguageSetting(selectedIndex);
            }

            void UpdateLanguageSetting(int index)
            {
                Main.Lang.CurrentLanguage = languageNames[index];
                model.Lang = Main.Lang.CurrentLanguage;
            }
            if(Drawer.Button(Main.Lang.GetFail() ? TranslatorHelper.FailString(Main.Lang) : (Main.Lang.GetLoading() ? Main.Lang.Get("RELOADING","Reloading...") : Main.Lang.Get("RELOADLANG","Reload Language Pack")),GUILayout.Width(320)))
            {
                _ = Main.Lang.LoadTranslationsAsync(Path.Combine(Main.Mod.Path,"lang"));
                Main.Lang.CurrentLanguage = model.Lang;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(Drawer.Button(Main.Lang.Get("EXTRA_MENU","Extra Menu") + " " + (isOpenedExtraMenu ? "▼" : "▲"))) {
                isOpenedExtraMenu = !isOpenedExtraMenu;
            }
            /*
            if(Drawer.Button(Main.Lang.Get("OPEN_WIKI_MENU","Open Wiki Menu"))) {
                if(Main.Wiki == null) {
                    Main.Wiki = new GameObject().AddComponent<Wiki.Wiki>();
                }
            }
            */
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if(isOpenedExtraMenu) {
                if(Drawer.DrawBool(string.Format(Main.Lang.Get("DISABLE_THIS","Disable {0}"), Main.Lang.Get("LOGO","Logo")), ref model.disableLogo)) {
                    if (model.disableLogo) {
                        Main.LogoRelease();
                    } else {
                        Main.LogoInit(Main.Mod.Path);
                    }
                }
                if (Drawer.DrawBool(Main.Lang.Get("CHANGE_FONT","Change Font"), ref model.ChangeFont))
                {
                    if (!model.ChangeFont)
                    {
                        model.AdofaiFont.name = "Default";
                        if (model.AdofaiFont.Apply(out var font))
                        {
                            FontManager.SetFont(model.AdofaiFont.name, font);
                            RDString.initialized = false;
                            RDString.Setup();
                            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        }
                    }
                }
                if (model.ChangeFont)
                {
                    GUILayoutEx.BeginIndent();
                    Drawer.DrawString(Main.Lang.Get("FONT","Font"), ref model.AdofaiFont.name);
                    Drawer.DrawSingle(Main.Lang.Get("FONT_SCALE","Font Scale"), ref model.AdofaiFont.fontScale);
                    Drawer.DrawSingle(Main.Lang.Get("LINE_SPACING","Font Line Spacing"), ref model.AdofaiFont.lineSpacing);
                    GUILayout.BeginHorizontal();
                    if (Drawer.Button(Main.Lang.Get("APPLY","Apply")))
                    {
                        if (model.AdofaiFont.Apply(out var font))
                        {
                            FontManager.SetFont(model.AdofaiFont.name, font);
                            RDString.initialized = false;
                            RDString.Setup();
                            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        }
                    }
                    if (Drawer.Button(Main.Lang.Get("LOG_FONT_LIST","Log Font List")))
                    {
                        foreach (var font in FontManager.OSFonts)
                            Main.Logger.Log(font);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayoutEx.EndIndent();
                }

                Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("AUTO_THIS", "Auto {0}"), Main.Lang.Get("UPDATE", "Update"))), ref model.useAutoUpdate);
                if (model.useAutoUpdate) {
                    GUILayoutEx.BeginIndent();
                    Drawer.DrawBool(string.Format(Main.Lang.Get("ALLOW_THIS", "Allow {0}"), Main.Lang.Get("BETA_TEXT", "Beta version")), ref model.useAutoUpdateBeta);
                    GUILayoutEx.EndIndent();
                }
                if (Drawer.DrawBool(
                        string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), Main.Lang.Get("LEGACY_THEME", "Legacy Theme")),
                        ref model.useLegacyTheme))
                {
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
                Drawer.DrawSingle(Main.Lang.Get("FPS_UPDATE_RATE","Fps Update Rate"), ref model.FPSUpdateRate);
                Drawer.DrawSingle(Main.Lang.Get("FRAMETIME_UPDATE_RATE","FrameTime Update Rate"), ref model.FrameTimeUpdateRate);
                Drawer.DrawInt32(Main.Lang.Get("SYSTEMTAG_UPDATE_RATE","System Tag Update Rate"), ref model.SystemTagUpdateRate);
            }
            GUILayout.BeginHorizontal();
            if (Drawer.Button(Main.Lang.Get("NEW_TEXT","Create New Text")))
            {
                TextManager.CreateText(new TextConfig());
                TextManager.Refresh();
            }
            if (Drawer.Button(Main.Lang.Get("IMPORT_TEXT","Import Text")))
            {
                var texts = StandaloneFileBrowser.OpenFilePanel(Main.Lang.Get("SELECT_TEXT","Select Text"), Main.Mod.Path, new[] { new ExtensionFilter("Text", "json") }, true);
                foreach (var text in texts)
                {
                    var json = JsonNode.Parse(File.ReadAllText(text));
                    if (json is JsonArray arr)
                        ModelUtils.UnwrapList<TextConfig>(arr).ForEach(t => TextManager.CreateText(t));
                    else if (json is JsonObject obj)
                        TextManager.CreateText(TextConfigImporter.Import(obj));
                }
                TextManager.Refresh();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            for (int i = 0; i < TextManager.Count; i++)
            {
                var text = TextManager.Get(i);
                if(text == null) {
                    GUILayout.Label($"[{Main.Lang.Get("ERROR","Error")}] " + string.Format(Main.Lang.Get("ERROR_THIS_TEXT_INDEX","Unable to load text data at index {0}"), i.ToString()));
                    continue;
                }
                GUILayout.BeginHorizontal();
                GUI.color = new Color(0.8f, 0.8f, 1f);
                if(Drawer.Button(Main.Lang.Get("EDIT", "Edit"))) {
                    TextConfigDrawer config = new TextConfigDrawer(text.Config);
                    config.strInit();
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
                GUI.color = Color.white;
                GUILayout.Label(text.Config.Name);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        private int egClickCount = 0;
        private DateTime egFirstClickTime = DateTime.MinValue;

        private void egHandle() {
            DateTime now = DateTime.UtcNow;

            if(egClickCount == 0) {
                egFirstClickTime = now;
            }

            egClickCount++;

            if((now - egFirstClickTime).TotalSeconds > 1.0) {
                egClickCount = 1;
                egFirstClickTime = now;
            }

            if(egClickCount >= 7 && !Main.EgEnabled) {
                Main.EgEnabled = true;
                egClickCount = 0;
            }
        }
    }
}
