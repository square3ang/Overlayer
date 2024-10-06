using JSON;
using Overlayer.Core;
using Overlayer.Core.Translatior;
using Overlayer.Models;
using Overlayer.Utils;
using SA.GoogleDoc;
using SFB;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityModManagerNet.UnityModManager;

namespace Overlayer.Views
{
    public class SettingsDrawer : ModelDrawable<Settings>
    {
        public SettingsDrawer(Settings settings) : base(settings) { }

        private static string FailString()
        {
            switch(Main.Lang.GetFailAdvence())
            {
                case 0:
                    return "Load Language Pack";
                case -1:
                    return "Fail: Unknown Cause";
                case 1:
                    return "Fail: No valid translation was found";
                case 2:
                    return "Fail: Error Reading Directory";
                case 3:
                    return "Fail: Error loading file";
                case 4:
                    return "Fail: The file does not exist";
                default:
                    return "Load Language Pack (Unknown error)";
            }
        }

        public override void Draw()
        {
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
            
            if(Drawer.RawSelectionPopup(ref selectedIndex,languageNames, GUILayout.Width(400)))
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
            if(Drawer.Button(Main.Lang.GetFail() ? FailString() : (Main.Lang.GetLoading() ? Main.Lang.Get("RELOADING","Reloading...") : Main.Lang.Get("RELOADLANG","Reload Language Pack")),GUILayout.Width(320)))
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
            Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), Main.Lang.Get("LEGACY_THEME","Legacy Theme")), ref model.useLegacyTheme);
            Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "MovingMan")), ref model.useMovingManEditor);
            Drawer.DrawBool(string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "ColorRange")), ref model.useColorRangeEditor);
            Drawer.DrawSingle(Main.Lang.Get("FPS_UPDATE_RATE","Fps Update Rate"), ref model.FPSUpdateRate);
            Drawer.DrawSingle(Main.Lang.Get("FRAMETIME_UPDATE_RATE","FrameTime Update Rate"), ref model.FrameTimeUpdateRate);
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
                Drawer.TitleButton(
                    string.Format(Main.Lang.Get("EDIT_THIS_TEXT","Edit {0} Text"),text.Config.Name),
                    Main.Lang.Get("EDIT","Edit"),
                    () => Main.GUI.Push(new TextConfigDrawer(text.Config)),
                    () =>
                    {
                    if (Drawer.Button(Main.Lang.Get("DESTROY","Destroy")))
                    {
                        var popup = new GameObject().AddComponent<DeletePopup>();
                        UnityEngine.Object.DontDestroyOnLoad(popup);
                        popup.Initialize(text);
                        
                        return;
                    }
                });
            }
        }
    }
}
