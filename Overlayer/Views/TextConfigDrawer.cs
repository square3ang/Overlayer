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
            Drawer.DrawBool(string.Format(Main.Lang.Get("EDIT_THIS","Edit {0}"),Main.Lang.Get("TEXT_COLOR","Text Color")), ref colorsExpanded[0]);
            if (colorsExpanded[0])
            {
                GUILayoutEx.BeginIndent();
                changed |= Drawer.DrawGColor(ref model.TextColor, true);
                GUILayoutEx.EndIndent();
            }
            Drawer.DrawBool(string.Format(Main.Lang.Get("EDIT_THIS","Edit {0}"),Main.Lang.Get("SHADOW_COLOR","Shadow Color")), ref colorsExpanded[1]);
            if (colorsExpanded[1])
            {
                GUILayoutEx.BeginIndent();
                changed |= Drawer.DrawGColor(ref model.ShadowColor, false);
                GUILayoutEx.EndIndent();
            }
            Drawer.DrawBool(string.Format(Main.Lang.Get("EDIT_THIS","Edit {0}"),Main.Lang.Get("OUTLINE_COLOR","Outline Color")), ref colorsExpanded[2]);
            if (colorsExpanded[2])
            {
                GUILayoutEx.BeginIndent();
                changed |= Drawer.DrawGColor(ref model.OutlineColor, false);
                GUILayoutEx.EndIndent();
            }
            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("OUTLINE_WIDTH","Outline Width"), ref model.OutlineWidth, 0, 1, 300f);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Lang.Get("ALIGNMENT","Alignment"));
            changed |= Drawer.DrawEnumPlus("Text Alignment",ref model.Alignment,TranslateTextAlignment);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            changed |= Drawer.DrawCodeEditor(Main.Lang.Get("PLAYING_TEXT","Playing Text"), model.Name + "PlayingText", ref model.PlayingText);
            changed |= Drawer.DrawCodeEditor(Main.Lang.Get("NOT_PLAYING_TEXT","Not Playing Text"), model.Name + "NotPlayingText", ref model.NotPlayingText);
            GUILayout.BeginHorizontal();
            if (Drawer.Button(Main.Lang.Get("EXPORT","Export")))
            {
                string target = StandaloneFileBrowser.SaveFilePanel(Main.Lang.Get("SELECT_TEXT","Select Text"), Persistence.GetLastUsedFolder(), $"{model.Name}.json", "json");
                if (!string.IsNullOrWhiteSpace(target))
                {
                    var node = model.Serialize();
                    node["References"] = TextConfigImporter.GetReferences(model);
                    File.WriteAllText(target, node.ToString(4));
                }
            }
            if (Drawer.Button(Main.Lang.Get("RESET","Reset")))
            {
                changed = true;
                text.Config = model = new TextConfig();
            }
            if (Drawer.Button(Main.Lang.Get("DESTROY","Destroy")))
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

        private string TranslateTextAlignment(string alignmentName)
        {
            return alignmentName switch
            {
                "TopLeft" => Main.Lang.Get("TOP_LEFT","Top Left"),
                "Top" => Main.Lang.Get("TOP","Top"),
                "TopRight" => Main.Lang.Get("TOP_RIGHT","Top Right"),
                "TopJustified" => Main.Lang.Get("TOP_JUSTIFIED","Top Justified"),
                "TopFlush" => Main.Lang.Get("TOP_FLUSH","Top Flush"),
                "TopGeoAligned" => Main.Lang.Get("TOP_GEO_ALIGNED","Top Geo Aligned"),
                "Left" => Main.Lang.Get("LEFT","Left"),
                "Center" => Main.Lang.Get("CENTER","Center"),
                "Right" => Main.Lang.Get("RIGHT","Right"),
                "Justified" => Main.Lang.Get("JUSTIFIED","Justified"),
                "Flush" => Main.Lang.Get("FLUSH","Flush"),
                "CenterGeoAligned" => Main.Lang.Get("CENTER_GEO_ALIGNED","Center Geo Aligned"),
                "BottomLeft" => Main.Lang.Get("BOTTOM_LEFT","Bottom Left"),
                "Bottom" => Main.Lang.Get("BOTTOM","Bottom"),
                "BottomRight" => Main.Lang.Get("BOTTOM_RIGHT","Bottom Right"),
                "BottomJustified" => Main.Lang.Get("BOTTOM_JUSTIFIED","Bottom Justified"),
                "BottomFlush" => Main.Lang.Get("BOTTOM_FLUSH","Bottom Flush"),
                "BottomGeoAligned" => Main.Lang.Get("BOTTOM_GEO_ALIGNED","Bottom Geo Aligned"),
                "BaselineLeft" => Main.Lang.Get("BASELINE_LEFT","Baseline Left"),
                "Baseline" => Main.Lang.Get("BASELINE","Baseline"),
                "BaselineRight" => Main.Lang.Get("BASELINE_RIGHT","Baseline Right"),
                "BaselineJustified" => Main.Lang.Get("BASELINE_JUSTIFIED","Baseline Justified"),
                "BaselineFlush" => Main.Lang.Get("BASELINE_FLUSH","Baseline Flush"),
                "BaselineGeoAligned" => Main.Lang.Get("BASELINE_GEO_ALIGNED","Baseline Geo Aligned"),
                "MidlineLeft" => Main.Lang.Get("MIDLINE_LEFT","Midline Left"),
                "Midline" => Main.Lang.Get("MIDLINE","Midline"),
                "MidlineRight" => Main.Lang.Get("MIDLINE_RIGHT","Midline Right"),
                "MidlineJustified" => Main.Lang.Get("MIDLINE_JUSTIFIED","Midline Justified"),
                "MidlineFlush" => Main.Lang.Get("MIDLINE_FLUSH","Midline Flush"),
                "MidlineGeoAligned" => Main.Lang.Get("MIDLINE_GEO_ALIGNED","Midline Geo Aligned"),
                "CaplineLeft" => Main.Lang.Get("CAPLINE_LEFT","Capline Left"),
                "Capline" => Main.Lang.Get("CAPLINE","Capline"),
                "CaplineRight" => Main.Lang.Get("CAPLINE_RIGHT","Capline Right"),
                "CaplineJustified" => Main.Lang.Get("CAPLINE_JUSTIFIED","Capline Justified"),
                "CaplineFlush" => Main.Lang.Get("CAPLINE_FLUSH","Capline Flush"),
                "CaplineGeoAligned" => Main.Lang.Get("CAPLINE_GEO_ALIGNED","Capline Geo Aligned"),
                "Converted" => Main.Lang.Get("CONVERTED","Converted"),
                _ => alignmentName
            };
        }
    }
}
