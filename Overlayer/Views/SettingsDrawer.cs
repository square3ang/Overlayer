using Overlayer.Core;
using Overlayer.Core.Translation;
using Overlayer.Utils;
using UnityEngine;
using Overlayer.Models;
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
            Drawer.ButtonLabel(L(TKS.Langauge), Main.OpenDiscordLink);
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
                if (GUILayout.Button(L(TKS.AdofaiFont_Apply)))
                {
                    if (model.AdofaiFont.Apply(out var font))
                    {
                        FontManager.SetFont(model.AdofaiFont.name, font);
                        RDString.initialized = false;
                        RDString.Setup();
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayoutEx.EndIndent();
            }
            Drawer.DrawSingle(L(TKS.FpsUpdateRate), ref model.FPSUpdateRate);
            Drawer.DrawSingle(L(TKS.FrameTimeUpdateRate), ref model.FrameTimeUpdateRate);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(L(TKS.NewText))) TextManager.CreateText(new TextConfig());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            for (int i = 0; i < TextManager.Count; i++)
            {
                var text = TextManager.Get(i);
                Drawer.TitleButton(L(TKS.EditThisText, text.Config.Name), L(TKS.Edit), () => Main.GUI.Push(new TextConfigDrawer(text.Config)), () =>
                {
                    if (GUILayout.Button(L(TKTC.Destroy)))
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
