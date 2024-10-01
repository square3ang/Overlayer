using Overlayer.Core;
using Overlayer.Models;
using Overlayer.Tags;
using Overlayer.Unity;
using SFB;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Overlayer.Views
{
    public class TextConfigDrawer : ModelDrawable<TextConfig>
    {
        public OverlayerText text;
        private bool[] colorsExpanded = new bool[4];
        public TextConfigDrawer(TextConfig config) : base(config) => text = TextManager.Find(config);
        public override void Draw()
        {
            if (Drawer.DrawBool(Main.Lang.Get("ACTIVE","Active"), ref model.Active))
                text.gameObject.SetActive(model.Active);
            bool changed = false;
            GUILayout.Label($"{Main.Lang.Get("AVAILABLE_TAGS","Available Tags")}: {TagManager.Count}");
            Drawer.DrawString(Main.Lang.Get("NAME","Name"), ref model.Name);
            changed |= Drawer.DrawVector2(Main.Lang.Get("POSITION","Position"), ref model.Position, 0, 1);
            changed |= Drawer.DrawVector2(Main.Lang.Get("SCALE","Scale"), ref model.Scale, 0, 2);
            changed |= Drawer.DrawVector2(Main.Lang.Get("PIVOT","Pivot"), ref model.Pivot, 0, 1);
            changed |= Drawer.DrawVector3(Main.Lang.Get("ROTATION","Rotation"), ref model.Rotation, -180, 180);
            changed |= Drawer.DrawVector2(Main.Lang.Get("SHADOW_OFFSET","Shadow Offset"), ref model.ShadowOffset, -1, 1);
            changed |= Drawer.DrawString(Main.Lang.Get("FONT","Font"), ref model.Font);
            changed |= Drawer.DrawBool(Main.Lang.Get("FALLBACK_FONTS","Enable Fallback Fonts"), ref model.EnableFallbackFonts);
            if (model.EnableFallbackFonts)
            {
                if (model.FallbackFonts == null) model.FallbackFonts = new string[0];
                changed |= Drawer.DrawStringArray(ref model.FallbackFonts);
            }
            changed |= Drawer.DrawString(Main.Lang.Get("LEX_OPTION","Text interpreter settings"), ref model.LexOption);
            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("FONT_SIZE","Font Size"), ref model.FontSize, 0, 100, 300f);
            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("SHADOW_DILATE","Shadow Dilate"), ref model.ShadowDilate, 0, 1, 300f);
            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("SHADOW_SOFTNESS","Shadow Softness"), ref model.ShadowSoftness, 0, 1, 300f);
            Drawer.DrawBool(string.Format(Main.Lang.Get("EDIT_THIS_TEXT","Edit {0} Text"),Main.Lang.Get("TEXT_COLOR","Text Color")), ref colorsExpanded[0]);
            if (colorsExpanded[0])
            {
                GUILayoutEx.BeginIndent();
                changed |= Drawer.DrawGColor(ref model.TextColor, true);
                GUILayoutEx.EndIndent();
            }
            Drawer.DrawBool(string.Format(Main.Lang.Get("EDIT_THIS_TEXT","Edit {0} Text"),Main.Lang.Get("SHADOW_COLOR","Shadow Color")), ref colorsExpanded[1]);
            if (colorsExpanded[1])
            {
                GUILayoutEx.BeginIndent();
                changed |= Drawer.DrawGColor(ref model.ShadowColor, false);
                GUILayoutEx.EndIndent();
            }
            Drawer.DrawBool(string.Format(Main.Lang.Get("EDIT_THIS_TEXT","Edit {0} Text"),Main.Lang.Get("SHADOW_COLOR","Shadow Color")), ref colorsExpanded[2]);
            if (colorsExpanded[2])
            {
                GUILayoutEx.BeginIndent();
                changed |= Drawer.DrawGColor(ref model.OutlineColor, false);
                GUILayoutEx.EndIndent();
            }
            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("OUTLINE_WIDTH","Outline Width"), ref model.OutlineWidth, 0, 1, 300f);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Lang.Get("ALIGNMENT","Alignment"));
            changed |= Drawer.DrawEnum(Main.Lang.Get("ALIGNMENT","Alignment"), ref model.Alignment);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            changed |= Drawer.DrawString(Main.Lang.Get("PLAYING_TEXT","Playing Text"), ref model.PlayingText, true);
            changed |= Drawer.DrawString(Main.Lang.Get("NOT_PLAYING_TEXT","Not Playing Text"), ref model.NotPlayingText, true);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Main.Lang.Get("EXPORT","Export")))
            {
                string target = StandaloneFileBrowser.SaveFilePanel(Main.Lang.Get("SELECT_TEXT","Select Text"), Persistence.GetLastUsedFolder(), $"{model.Name}.json", "json");
                if (!string.IsNullOrWhiteSpace(target))
                {
                    var node = model.Serialize();
                    node["References"] = TextConfigImporter.GetReferences(model);
                    File.WriteAllText(target, node.ToString(4));
                }
            }
            if (GUILayout.Button(Main.Lang.Get("RESET","Reset")))
            {
                changed = true;
                text.Config = model = new TextConfig();
            }
            if (GUILayout.Button(Main.Lang.Get("DESTROY","Destroy")))
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
