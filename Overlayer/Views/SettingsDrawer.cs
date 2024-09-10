using JSON;
using Overlayer.Core;
using Overlayer.Core.Translation;
using Overlayer.Models;
using Overlayer.Utils;
using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TKS = Overlayer.Core.Translation.TranslationKeys.Settings;
using TKTC = Overlayer.Core.Translation.TranslationKeys.TextConfig;

namespace Overlayer.Views
{
    public class SettingsDrawer : ModelDrawable<Settings>
    {
        public SettingsDrawer(Settings settings) : base(settings, L(TKS.Prefix)) { }
        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            Drawer.ButtonLabel(L(TKS.Langauge), () => Application.OpenURL(Main.DiscordLink));
            if (Drawer.DrawEnum(L(TKS.Langauge), ref model.Lang, model.GetHashCode()))
                Main.Lang = Language.GetLangauge(model.Lang);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (Drawer.DrawBool(L(TKS.ChangeFont), ref model.ChangeFont))
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
                Drawer.DrawString(L(TKS.AdofaiFont_Font), ref model.AdofaiFont.name);
                Drawer.DrawSingle(L(TKS.AdofaiFont_FontScale), ref model.AdofaiFont.fontScale);
                Drawer.DrawSingle(L(TKS.AdofaiFont_LineSpacing), ref model.AdofaiFont.lineSpacing);
                GUILayout.BeginHorizontal();
                if (Drawer.Button(L(TKS.AdofaiFont_Apply)))
                {
                    if (model.AdofaiFont.Apply(out var font))
                    {
                        FontManager.SetFont(model.AdofaiFont.name, font);
                        RDString.initialized = false;
                        RDString.Setup();
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }
                }
                if (Drawer.Button(L(TKS.LogFontList)))
                {
                    foreach (var font in FontManager.OSFonts)
                        Main.Logger.Log(font);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayoutEx.EndIndent();
            }
            Drawer.DrawSingle(L(TKS.FpsUpdateRate), ref model.FPSUpdateRate);
            Drawer.DrawSingle(L(TKS.FrameTimeUpdateRate), ref model.FrameTimeUpdateRate);
            GUILayout.BeginHorizontal();
            if (Drawer.Button(L(TKS.NewText)))
            {
                TextManager.CreateText(new TextConfig());
                TextManager.Refresh();
            }
            if (Drawer.Button(L(TKS.ImportText)))
            {
                var texts = StandaloneFileBrowser.OpenFilePanel(L(TKTC.SelectText), Main.Mod.Path, new[] { new ExtensionFilter("Text", "json") }, true);
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
                Drawer.TitleButton(L(TKS.EditThisText, text.Config.Name), L(TKS.Edit), () => Main.GUI.Push(new TextConfigDrawer(text.Config)), () =>
                {
                    if (Drawer.Button(L(TKTC.Destroy)))
                    {
                        TextManager.DestroyText(text);
                        Main.GUI.Skip(frames: 2);
                        Main.GUI.Pop();
                        return;
                    }
                });
            }
        }
    }
}
