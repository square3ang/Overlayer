using JSON;
using Overlayer.Core;
using Overlayer.Core.Translatior;
using Overlayer.Models;
using Overlayer.Utils;
using SA.GoogleDoc;
using SFB;
using System;
using System.IO;
using RapidGUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityModManagerNet.UnityModManager;
using Object = UnityEngine.Object;
using Overlayer.Core.Patches;
using Overlayer.Tags.Patches;
using static Overlayer.Patches.HitFixPatch;
using Newtonsoft.Json;

namespace Overlayer.Views
{
    public class SettingsDrawer : ModelDrawable<Settings>
    {
        public SettingsDrawer(Settings settings) : base(settings) { }

        public override void Draw()
        {
            if(Main.Logo != null) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Main.Logo, GUILayout.Width(Main.Logo.width), GUILayout.Height(Main.Logo.height));
                GUILayout.BeginVertical();
                GUILayout.Label("<size=62>Overlayer v3</size>");
                GUILayout.Label($"<size=26>{Main.Lang.Get("SLOGAN_TEXT", "Display everything as you wish.")}</size>");
                GUILayout.Label($"<size=16>{Main.ModVersion}, by <color=#{Tags.Effect.Rainbow()}>Square & Kkitut</color></size>");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            if(model != null)
            Main.Lang.CurrentLanguage = model.Lang;
            GUILayout.Label(Main.Lang.Get("SELECTLANGUAGE","Select Language"));
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
            if(Drawer.Button(Main.Lang.GetFail() ? Main.Lang.FailString() : (Main.Lang.GetLoading() ? Main.Lang.Get("RELOADING","Reloading...") : Main.Lang.Get("RELOADLANG","Reload Language Pack")),GUILayout.Width(320)))
            {
                _ = Main.Lang.LoadTranslationsAsync(Path.Combine(Main.Mod.Path,"lang"));
                Main.Lang.CurrentLanguage = model.Lang;
            }
            GUILayout.EndHorizontal();
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

            Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), Main.Lang.Get("AUTO_UPDATE","Auto Update")), ref model.useAutoUpdate);
            if (model.useAutoUpdate) {
                GUILayoutEx.BeginIndent();
                Drawer.DrawBool(string.Format(Main.Lang.Get("ALLOW_THIS", "Allow {0}"), Main.Lang.Get("BETA", "Beta")), ref model.useAutoUpdateBeta);
                GUILayoutEx.EndIndent();
            }
            if (Drawer.DrawBool(
                    string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), Main.Lang.Get("LEGACY_THEME", "Legacy Theme")),
                    ref model.useLegacyTheme))
            {
                RGUIStyle.CreateStyles();
            }
            if(Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("SHOW_TRUE_AUTO_JUDGMENT", "Show True Auto Judgment"))), ref model.useShowTrueAutoJudgment)) {
                LazyPatchManager.Unpatch(typeof(ChangeAddHit));
                LazyPatchManager.Patch(typeof(ChangeAddHit));
            }
            Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "MovingMan")), ref model.useMovingManEditor);
            Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "ColorRange")), ref model.useColorRangeEditor);
            Drawer.DrawSingle(Main.Lang.Get("FPS_UPDATE_RATE","Fps Update Rate"), ref model.FPSUpdateRate);
            Drawer.DrawSingle(Main.Lang.Get("FRAMETIME_UPDATE_RATE","FrameTime Update Rate"), ref model.FrameTimeUpdateRate);
            Drawer.DrawInt32(Main.Lang.Get("SYSTEMTAG_UPDATE_RATE","System Tag Update Rate"), ref model.SystemTagUpdateRate);
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
                    GUILayout.Label($"[{Main.Lang.Get("ERROR","Error")}] " + string.Format(Main.Lang.Get("ERROR_THIS_TEXT_INDEX","Unable to load text data at index {0}")), i.ToString());
                    continue;
                }
                GUILayout.BeginHorizontal();
                GUI.color = new Color(0.8f, 0.8f, 1f);
                if(Drawer.Button(Main.Lang.Get("EDIT", "Edit"))) {
                    Main.GUI.Push(new TextConfigDrawer(text.Config));
                }
                GUI.color = new Color(0.8f, 1f, 0.8f);
                if(Drawer.Button(Main.Lang.Get("CLONE", "Clone"))) {
                    TextManager.CreateText(text.Config.Copy());
                }
                GUI.color = new Color(1f, 0.8f, 0.8f);
                if(Drawer.Button(Main.Lang.Get("DESTROY", "Destroy"))) {
                    if(Object.FindAnyObjectByType<DeletePopup>() == null) {
                        if(Input.GetKey(KeyCode.LeftShift)) {
                            TextManager.DestroyText(text);
                        } else {
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
    }
}
