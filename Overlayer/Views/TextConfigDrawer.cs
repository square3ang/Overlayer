using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Overlayer.Core;
using Overlayer.Models;
using Overlayer.Unity;
using Overlayer.Utils;
using SFB;
using System;
using System.IO;
using UnityEngine;

namespace Overlayer.Views;

public class TextConfigDrawer : ModelDrawable<TextConfig> {
    public OverlayerText text;
    public TextConfigDrawer(TextConfig config) : base(config) => text = TextManager.Find(config);

    bool IsAdvensedMode => Main.Settings.uiMode == Settings.EditorUIMode.Advanced;

    public override void OnceCall() => NeoDrawer.StaticInstance.FieldResetDictById();

    public override void Draw() {
        NeoDrawer.StaticInstance.FieldResetId();

        GUILayout.BeginHorizontal();
        var oldMode = Main.Settings.uiMode;
        Color old = GUI.color;
        GUI.color = Main.Settings.uiMode == Settings.EditorUIMode.Simple ? Color.cyan : old;
        if(Drawer.Button(Main.Lang.Get("UI_SIMPLE", "Simple"), GUILayout.Width(120f), GUILayout.Height(32f))) {
            Main.Settings.uiMode = Settings.EditorUIMode.Simple;
        }
        GUI.color = Main.Settings.uiMode == Settings.EditorUIMode.Advanced ? Color.cyan : old;
        if(Drawer.Button(Main.Lang.Get("UI_ADVANCED", "Advanced"), GUILayout.Width(120f), GUILayout.Height(32f))) {
            Main.Settings.uiMode = Settings.EditorUIMode.Advanced;
        }
        GUI.color = old;
        GUILayout.EndHorizontal();
        if(oldMode != Main.Settings.uiMode) {
            NeoDrawer.StaticInstance.FieldClear();
        }

        if(Drawer.DrawBool(Drawer.icon_Active, Main.Lang.Get("ACTIVE", "Active"), ref model.Active)) {
            text.gameObject.SetActive(model.Active);
        }

        bool _drag = model.Drag;
        Drawer.DrawBool(Drawer.icon_Drag, Main.Lang.Get("DRAG", "Drag"), ref _drag);
        if(model.Drag != _drag) {
            model.Drag = _drag;
        }
        bool changed = false;
        Drawer.DrawString(Drawer.icon_Pencil, Main.Lang.Get("NAME", "Name"), ref model.Name);
        changed |= NeoDrawer.StaticInstance.DrawSize2(Main.Lang.Get("POSITION", "Position"), ref model.Position, 0, 1);
        if(IsAdvensedMode) {
            changed |= NeoDrawer.StaticInstance.DrawSize2(Main.Lang.Get("SCALE", "Scale"), ref model.Scale, 0, 2);
            changed |= NeoDrawer.StaticInstance.DrawSize2(Main.Lang.Get("PIVOT", "Pivot"), ref model.Pivot, 0, 1);
            changed |= NeoDrawer.StaticInstance.DrawRotate3(Main.Lang.Get("ROTATION", "Rotation"), ref model.Rotation, -180, 180);
            changed |= NeoDrawer.StaticInstance.DrawSize2(Main.Lang.Get("SHADOW_OFFSET", "Shadow Offset"), ref model.ShadowOffset, -1, 1);
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label(Drawer.icon_Font);
        GUILayout.Space(4);
        GUILayout.Label(Main.Lang.Get("FONT", "Font"));
        changed |= Drawer.DrawSelectFont(ref model.Font);
        GUILayout.EndHorizontal();
        if(IsAdvensedMode) {
            changed |= Drawer.DrawBool(Drawer.icon_FontAlternate, Main.Lang.Get("FALLBACK_FONTS", "Enable Fallback Fonts"), ref model.EnableFallbackFonts);

            if(model.EnableFallbackFonts) {
                model.FallbackFonts ??= new string[0];

                GUILayout.BeginHorizontal();

                if(Drawer.Button("+", GUILayout.Width(50))) {
                    Array.Resize(ref model.FallbackFonts, model.FallbackFonts.Length + 1);
                    changed = true;
                }

                if(Drawer.Button("-", GUILayout.Width(50)) && model.FallbackFonts.Length > 0) {
                    Array.Resize(ref model.FallbackFonts, model.FallbackFonts.Length - 1);
                    changed = true;
                }

                GUILayout.EndHorizontal();
                for(int i = 0; i < model.FallbackFonts.Length; i++) {
                    changed |= Drawer.DrawSelectFont(ref model.FallbackFonts[i]);
                }
            }
        }
        changed |= NeoDrawer.StaticInstance.DrawSingleWithSlider(Drawer.icon_FontSize, Main.Lang.Get("FONT_SIZE", "Font Size"), ref model.FontSize, 0, 100, 300f);
        if(IsAdvensedMode) {
            changed |= NeoDrawer.StaticInstance.DrawSingleWithSlider(Drawer.icon_LineSpacing, Main.Lang.Get("LINE_SPACING", "Line Spacing"), ref model.LineSpacing, -120f, 20f, 300f);
            changed |= NeoDrawer.StaticInstance.DrawSingleWithSlider(Drawer.icon_ShadowDilate, Main.Lang.Get("SHADOW_DILATE", "Shadow Dilate"), ref model.ShadowDilate, 0, 1, 300f);
            changed |= NeoDrawer.StaticInstance.DrawSingleWithSlider(Drawer.icon_ShadowSoftness, Main.Lang.Get("SHADOW_SOFTNESS", "Shadow Softness"), ref model.ShadowSoftness, 0, 1, 300f);
            changed |= NeoDrawer.StaticInstance.DrawSingleWithSlider(Drawer.icon_OutlineWidth, Main.Lang.Get("OUTLINE_WIDTH", "Outline Width"), ref model.OutlineWidth, 0, 1, 300f);
        }
        Drawer.DrawBool(Drawer.icon_Color, string.Format(Main.Lang.Get("EDIT_THIS", "Edit {0}"), Main.Lang.Get("TEXT_COLOR", "Text Color")), ref model.TextColor.status.Enabled);
        if(model.TextColor.status.Enabled) {
            changed |= NeoDrawer.StaticInstance.DrawGColor(ref model.TextColor, true);
        } else {
            NeoDrawer.StaticInstance.FieldSetId(NeoDrawer.StaticInstance.FieldGetId() + 4);
        }
        Drawer.DrawBool(Drawer.icon_Shadow, string.Format(Main.Lang.Get("EDIT_THIS", "Edit {0}"), Main.Lang.Get("SHADOW_COLOR", "Shadow Color")), ref model.ShadowColor.status.Enabled);
        if(model.ShadowColor.status.Enabled) {
            changed |= NeoDrawer.StaticInstance.DrawGColor(ref model.ShadowColor, false);
        } else {
            NeoDrawer.StaticInstance.FieldSetId(NeoDrawer.StaticInstance.FieldGetId() + 4);
        }
        Drawer.DrawBool(Drawer.icon_Outline, string.Format(Main.Lang.Get("EDIT_THIS", "Edit {0}"), Main.Lang.Get("OUTLINE_COLOR", "Outline Color")), ref model.OutlineColor.status.Enabled);
        if(model.OutlineColor.status.Enabled) {
            changed |= NeoDrawer.StaticInstance.DrawGColor(ref model.OutlineColor, false);
        } else {
            NeoDrawer.StaticInstance.FieldSetId(NeoDrawer.StaticInstance.FieldGetId() + 4);
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label(Main.Lang.Get("ALIGNMENT", "Alignment"));
        if(Drawer.DrawEnumPlus(ref model.Alignment, TranslateTextAlignment)) {
            changed = true;
            if(Main.Settings.autoPivot || !IsAdvensedMode) {
                model.Pivot = MiscUtils.AlignmentToPivot(model.Alignment);
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if(Drawer.DrawAlignment(ref model.Alignment)) {
            changed = true;
            if(Main.Settings.autoPivot || !IsAdvensedMode) {
                model.Pivot = MiscUtils.AlignmentToPivot(model.Alignment);
            }
        }

        changed |= Drawer.DrawCodeEditor(Drawer.icon_Play, Main.Lang.Get("PLAYING_TEXT", "Playing Text"), model.Name + "PlayingText", ref model.PlayingText);
        changed |= Drawer.DrawCodeEditor(Drawer.icon_Pause, Main.Lang.Get("NOT_PLAYING_TEXT", "Not Playing Text"), model.Name + "NotPlayingText", ref model.NotPlayingText);
        GUILayout.BeginHorizontal();
        GUI.color = new Color(1f, 0.8f, 1f);
        if(Drawer.Button(Main.Lang.Get("EXPORT", "Export"))) {
            string target = StandaloneFileBrowser.SaveFilePanel(Main.Lang.Get("SELECT_TEXT", "Select Text"), Persistence.GetLastUsedFolder(), $"{model.Name}.json", "json");
            if(!string.IsNullOrWhiteSpace(target)) {
                JObject node = model.Serialize() as JObject;
                node["References"] = TextConfigImporter.GetReferences(model);
                File.WriteAllText(
                    target,
                    JsonConvert.SerializeObject(node, Formatting.Indented)
                );
            }
        }
        GUI.color = Color.white;
        GUI.color = new Color(1f, 1f, 0.8f);
        if(Drawer.Button(Main.Lang.Get("RESET", "Reset"))) {
            changed = true;
            text.Config = model = new TextConfig();
        }
        GUI.color = new Color(1f, 0.8f, 0.8f);
        if(Drawer.Button(Main.Lang.Get("DESTROY", "Destroy"))) {
            TextManager.DestroyText(text);
            Main.GUI.Skip(frames: 2);
            Main.GUI.Pop();
            return;
        }
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(changed) {
            text.ApplyConfig();
        }

        NeoDrawer.StaticInstance.UpdateFocused();
    }

    private string TranslateTextAlignment(string alignmentName) {
        return alignmentName switch {
            "TopLeft" => Main.Lang.Get("TOP_LEFT", "Top Left"),
            "Top" => Main.Lang.Get("TOP", "Top"),
            "TopRight" => Main.Lang.Get("TOP_RIGHT", "Top Right"),
            "TopJustified" => Main.Lang.Get("TOP_JUSTIFIED", "Top Justified"),
            "TopFlush" => Main.Lang.Get("TOP_FLUSH", "Top Flush"),
            "TopGeoAligned" => Main.Lang.Get("TOP_GEO_ALIGNED", "Top Geo Aligned"),
            "Left" => Main.Lang.Get("LEFT", "Left"),
            "Center" => Main.Lang.Get("CENTER", "Center"),
            "Right" => Main.Lang.Get("RIGHT", "Right"),
            "Justified" => Main.Lang.Get("JUSTIFIED", "Justified"),
            "Flush" => Main.Lang.Get("FLUSH", "Flush"),
            "CenterGeoAligned" => Main.Lang.Get("CENTER_GEO_ALIGNED", "Center Geo Aligned"),
            "BottomLeft" => Main.Lang.Get("BOTTOM_LEFT", "Bottom Left"),
            "Bottom" => Main.Lang.Get("BOTTOM", "Bottom"),
            "BottomRight" => Main.Lang.Get("BOTTOM_RIGHT", "Bottom Right"),
            "BottomJustified" => Main.Lang.Get("BOTTOM_JUSTIFIED", "Bottom Justified"),
            "BottomFlush" => Main.Lang.Get("BOTTOM_FLUSH", "Bottom Flush"),
            "BottomGeoAligned" => Main.Lang.Get("BOTTOM_GEO_ALIGNED", "Bottom Geo Aligned"),
            "BaselineLeft" => Main.Lang.Get("BASELINE_LEFT", "Baseline Left"),
            "Baseline" => Main.Lang.Get("BASELINE", "Baseline"),
            "BaselineRight" => Main.Lang.Get("BASELINE_RIGHT", "Baseline Right"),
            "BaselineJustified" => Main.Lang.Get("BASELINE_JUSTIFIED", "Baseline Justified"),
            "BaselineFlush" => Main.Lang.Get("BASELINE_FLUSH", "Baseline Flush"),
            "BaselineGeoAligned" => Main.Lang.Get("BASELINE_GEO_ALIGNED", "Baseline Geo Aligned"),
            "MidlineLeft" => Main.Lang.Get("MIDLINE_LEFT", "Midline Left"),
            "Midline" => Main.Lang.Get("MIDLINE", "Midline"),
            "MidlineRight" => Main.Lang.Get("MIDLINE_RIGHT", "Midline Right"),
            "MidlineJustified" => Main.Lang.Get("MIDLINE_JUSTIFIED", "Midline Justified"),
            "MidlineFlush" => Main.Lang.Get("MIDLINE_FLUSH", "Midline Flush"),
            "MidlineGeoAligned" => Main.Lang.Get("MIDLINE_GEO_ALIGNED", "Midline Geo Aligned"),
            "CaplineLeft" => Main.Lang.Get("CAPLINE_LEFT", "Capline Left"),
            "Capline" => Main.Lang.Get("CAPLINE", "Capline"),
            "CaplineRight" => Main.Lang.Get("CAPLINE_RIGHT", "Capline Right"),
            "CaplineJustified" => Main.Lang.Get("CAPLINE_JUSTIFIED", "Capline Justified"),
            "CaplineFlush" => Main.Lang.Get("CAPLINE_FLUSH", "Capline Flush"),
            "CaplineGeoAligned" => Main.Lang.Get("CAPLINE_GEO_ALIGNED", "Capline Geo Aligned"),
            "Converted" => Main.Lang.Get("CONVERTED", "Converted"),
            _ => alignmentName
        };
    }
}
