using Overlayer.Core;
using Overlayer.Models;
using Overlayer.Tags;
using Overlayer.Unity;
using SFB;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TKTC = Overlayer.Core.Translation.TranslationKeys.TextConfig;

namespace Overlayer.Views
{
    public class TextConfigDrawer : ModelDrawable<TextConfig>
    {
        public OverlayerText text;
        private bool[] colorsExpanded = new bool[4];
        public TextConfigDrawer(TextConfig config) : base(config, L(TKTC.Prefix, config.Name)) => text = TextManager.Find(config);
        public override void Draw()
        {
            if (Drawer.DrawBool(L(TKTC.Active), ref model.Active))
                text.gameObject.SetActive(model.Active);
            bool changed = false;
            Drawer.ButtonLabel($"Available Tags: {TagManager.Count}", () => Application.OpenURL(Main.DiscordLink));
            Drawer.DrawString(L(TKTC.Name), ref model.Name);
            changed |= Drawer.DrawVector2(L(TKTC.Position), ref model.Position, 0, 1);
            changed |= Drawer.DrawVector2(L(TKTC.Scale), ref model.Scale, 0, 2);
            changed |= Drawer.DrawVector2(L(TKTC.Pivot), ref model.Pivot, 0, 1);
            changed |= Drawer.DrawVector3(L(TKTC.Rotation), ref model.Rotation, -180, 180);
            changed |= Drawer.DrawVector2(L(TKTC.ShadowOffset), ref model.ShadowOffset, -1, 1);
            changed |= Drawer.DrawString(L(TKTC.Font), ref model.Font);
            changed |= Drawer.DrawBool(L(TKTC.EnableFallbackFonts), ref model.EnableFallbackFonts);
            if (model.EnableFallbackFonts)
            {
                if (model.FallbackFonts == null) model.FallbackFonts = new string[0];
                changed |= Drawer.DrawStringArray(ref model.FallbackFonts);
            }
            changed |= Drawer.DrawString(L(TKTC.LexOption), ref model.LexOption);
            changed |= Drawer.DrawSingleWithSlider(L(TKTC.FontSize), ref model.FontSize, 0, 100, 300f);
            changed |= Drawer.DrawSingleWithSlider(L(TKTC.ShadowDilate), ref model.ShadowDilate, 0, 1, 300f);
            changed |= Drawer.DrawSingleWithSlider(L(TKTC.ShadowSoftness), ref model.ShadowSoftness, 0, 1, 300f);
            Drawer.DrawBool(L(TKTC.EditThis, L(TKTC.TextColor)), ref colorsExpanded[0]);
            if (colorsExpanded[0])
            {
                GUILayoutEx.BeginIndent();
                changed |= Drawer.DrawGColor(ref model.TextColor, true);
                GUILayoutEx.EndIndent();
            }
            Drawer.DrawBool(L(TKTC.EditThis, L(TKTC.ShadowColor)), ref colorsExpanded[1]);
            if (colorsExpanded[1])
            {
                GUILayoutEx.BeginIndent();
                changed |= Drawer.DrawGColor(ref model.ShadowColor, false);
                GUILayoutEx.EndIndent();
            }
            Drawer.DrawBool(L(TKTC.EditThis, L(TKTC.OutlineColor)), ref colorsExpanded[2]);
            if (colorsExpanded[2])
            {
                GUILayoutEx.BeginIndent();
                changed |= Drawer.DrawGColor(ref model.OutlineColor, false);
                GUILayoutEx.EndIndent();
            }
            changed |= Drawer.DrawSingleWithSlider(L(TKTC.OutlineWidth), ref model.OutlineWidth, 0, 1, 300f);

            GUILayout.BeginHorizontal();
            Drawer.ButtonLabel(L(TKTC.Alignment), () => Application.OpenURL(Main.DiscordLink));
            changed |= Drawer.DrawEnum(L(TKTC.Alignment), ref model.Alignment);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            changed |= Drawer.DrawString(L(TKTC.PlayingText), ref model.PlayingText, true);
            changed |= Drawer.DrawString(L(TKTC.NotPlayingText), ref model.NotPlayingText, true);
            GUILayout.BeginHorizontal();
            if (Drawer.Button(L(TKTC.Export)))
            {
                string target = StandaloneFileBrowser.SaveFilePanel(L(TKTC.SelectText), Persistence.GetLastUsedFolder(), $"{model.Name}.json", "json");
                if (!string.IsNullOrWhiteSpace(target))
                {
                    var node = model.Serialize();
                    node["References"] = TextConfigImporter.GetReferences(model);
                    File.WriteAllText(target, node.ToString(4));
                }
            }
            if (Drawer.Button(L(TKTC.Reset)))
            {
                changed = true;
                text.Config = model = new TextConfig();
            }
            if (Drawer.Button(L(TKTC.Destroy)))
            {
                TextManager.DestroyText(text);
                Main.GUI.Skip(frames: 2);
                Main.GUI.Pop();
                return;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (changed) text.ApplyConfig();
        }
    }
}
