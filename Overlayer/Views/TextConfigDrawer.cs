using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Overlayer.Core;
using Overlayer.Models;
using Overlayer.Unity;
using Overlayer.Utils;
using SFB;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Overlayer.Views;

public class TextConfigDrawer : ModelDrawable<TextConfig> {
    public OverlayerText text;

    public TextConfigDrawer(OverlayerText text) : base((TextConfig)text.Config) => this.text = text;

    bool IsAdvensedMode => Main.Settings.UiMode == Settings.EditorUIMode.Advanced;

    public override void OnceCall() => NeoDrawer.StaticInstance.FieldResetDictById();

    public override void Draw() {
        NeoDrawer.StaticInstance.FieldResetId();
        Color old = GUI.color;

        GUILayout.BeginHorizontal();
        var oldMode = Main.Settings.UiMode;
        GUI.color = Main.Settings.UiMode == Settings.EditorUIMode.Simple ? Color.cyan : old;
        if(Drawer.Button(Main.Lang.Get("UI_SIMPLE", "Simple"), GUILayout.Width(120f), GUILayout.Height(32f))) {
            Main.Settings.UiMode = Settings.EditorUIMode.Simple;
        }
        GUI.color = Main.Settings.UiMode == Settings.EditorUIMode.Advanced ? Color.cyan : old;
        if(Drawer.Button(Main.Lang.Get("UI_ADVANCED", "Advanced"), GUILayout.Width(120f), GUILayout.Height(32f))) {
            Main.Settings.UiMode = Settings.EditorUIMode.Advanced;
        }
        GUI.color = old;
        GUILayout.EndHorizontal();
        if(oldMode != Main.Settings.UiMode) {
            NeoDrawer.StaticInstance.FieldClear();
        }

        if(Drawer.DrawBool(Drawer.Icon_Power, Main.Lang.Get("ACTIVE", "Active"), ref model.Active)) {
            text.gameObject.SetActive(model.Active);
        }

        bool _drag = model.Drag;
        Drawer.DrawBool(Drawer.Icon_Drag, Main.Lang.Get("DRAG", "Drag"), ref _drag);
        if(model.Drag != _drag) {
            model.Drag = _drag;
        }
        bool changed = false;
        Drawer.DrawString(Drawer.Icon_Pencil, Main.Lang.Get("NAME", "Name"), ref model.Name);
        changed |= Drawer.DrawExpr(Main.Lang.Get("POSITION", "Position"), "T" + nameof(model.Position), ref model.Position, () => changed |= NeoDrawer.StaticInstance.DrawSize2(ref model.Position.Value, 0, 1), typeof(Vector2));
        if(IsAdvensedMode) {
            changed |= Drawer.DrawExpr(Main.Lang.Get("SCALE", "Scale"), "T" + nameof(model.Scale), ref model.Scale, () => changed |= NeoDrawer.StaticInstance.DrawSize2(ref model.Scale.Value, 0, 2), typeof(Vector2));
            changed |= Drawer.DrawExpr(Main.Lang.Get("PIVOT", "Pivot"), "T" + nameof(model.Pivot), ref model.Pivot, () => changed |= NeoDrawer.StaticInstance.DrawSize2(ref model.Pivot.Value, 0, 1), typeof(Vector2));
            changed |= Drawer.DrawExpr(Main.Lang.Get("ROTATION", "Rotation"), "T" + nameof(model.Rotation), ref model.Rotation, () => changed |= NeoDrawer.StaticInstance.DrawRotate3(ref model.Rotation.Value, -180, 180), typeof(Vector3));
            changed |= Drawer.DrawExpr(Main.Lang.Get("SHADOW_OFFSET", "Shadow Offset"), "T" + nameof(model.ShadowOffset), ref model.ShadowOffset, () => changed |= NeoDrawer.StaticInstance.DrawSize2(ref model.ShadowOffset.Value, -1, 1), typeof(Vector2));
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label(Drawer.Icon_Font);
        GUILayout.Space(4);
        GUILayout.Label(Main.Lang.Get("FONT", "Font"));
        Drawer.DrawSelectFont(font => {
            model.Font = font;
            text.ApplyConfig();
        });
        changed |= Drawer.DrawOnlyString(ref model.Font);
        GUILayout.EndHorizontal();
        if(IsAdvensedMode) {
            changed |= Drawer.DrawBool(Drawer.Icon_FontAlternate, Main.Lang.Get("FALLBACK_FONTS", "Enable Fallback Fonts"), ref model.EnableFallbackFonts);

            if(model.EnableFallbackFonts) {
                model.FallbackFonts ??= [];

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
                Drawer.BeginTab();
                for(int i = 0; i < model.FallbackFonts.Length; i++) {
                    GUILayout.BeginHorizontal();
                    Drawer.DrawSelectFont(font => {
                        model.FallbackFonts[i] = font;
                        text.ApplyConfig();
                    });
                    changed |= Drawer.DrawOnlyString(ref model.FallbackFonts[i]);
                    GUILayout.EndHorizontal();
                }
                Drawer.EndTab();
            }
        }
        changed |= Drawer.DrawExpr(Drawer.Icon_FontSize, Main.Lang.Get("FONT_SIZE", "Font Size"), "T" + nameof(model.FontSize), ref model.FontSize, () => {
            GUILayout.BeginHorizontal();
            changed |= NeoDrawer.StaticInstance.DrawSingleWithSlider(ref model.FontSize.Value, 0, 100, 300f);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        });
        if(IsAdvensedMode) {
            changed |= Drawer.DrawExpr(Drawer.Icon_LineSpacing, Main.Lang.Get("LINE_SPACING", "Line Spacing"), "T" + nameof(model.LineSpacing), ref model.LineSpacing, () => {
                GUILayout.BeginHorizontal();
                changed |= NeoDrawer.StaticInstance.DrawSingleWithSlider(ref model.LineSpacing.Value, -120f, 20f, 300f);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            });
            changed |= Drawer.DrawExpr(Drawer.Icon_ShadowDilate, Main.Lang.Get("SHADOW_DILATE", "Shadow Dilate"), "T" + nameof(model.ShadowDilate), ref model.ShadowDilate, () => {
                GUILayout.BeginHorizontal();
                changed |= NeoDrawer.StaticInstance.DrawSingleWithSlider(ref model.ShadowDilate.Value, 0, 1, 300f);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            });
            changed |= Drawer.DrawExpr(Drawer.Icon_ShadowSoftness, Main.Lang.Get("SHADOW_SOFTNESS", "Shadow Softness"), "T" + nameof(model.ShadowSoftness), ref model.ShadowSoftness, () => {
                GUILayout.BeginHorizontal();
                changed |= NeoDrawer.StaticInstance.DrawSingleWithSlider(ref model.ShadowSoftness.Value, 0, 1, 300f);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            });
            changed |= Drawer.DrawExpr(Drawer.Icon_OutlineWidth, Main.Lang.Get("OUTLINE_WIDTH", "Outline Width"), "T" + nameof(model.OutlineWidth), ref model.OutlineWidth, () => {
                GUILayout.BeginHorizontal();
                changed |= NeoDrawer.StaticInstance.DrawSingleWithSlider(ref model.OutlineWidth.Value, 0, 1, 300f);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            });
        }
        changed |= Drawer.DrawExpr(Drawer.Icon_Color, Main.Lang.Get("COLOR", "Color"), "T" + nameof(model.TextColor), ref model.TextColor, () => changed |= NeoDrawer.StaticInstance.DrawGColor(ref model.TextColor.Value), typeof(GColor));
        changed |= Drawer.DrawExpr(Drawer.Icon_Shadow, Main.Lang.Get("SHADOW_COLOR", "Shadow Color"), "T" + nameof(model.ShadowColor), ref model.ShadowColor, () => {
            GUILayout.BeginHorizontal();
            changed |= NeoDrawer.StaticInstance.DrawColor(ref model.ShadowColor.Value);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }, typeof(Color));
        changed |= Drawer.DrawExpr(Drawer.Icon_Outline, Main.Lang.Get("OUTLINE_COLOR", "Outline Color"), "T" + nameof(model.OutlineColor), ref model.OutlineColor, () => {
            GUILayout.BeginHorizontal();
            changed |= NeoDrawer.StaticInstance.DrawColor(ref model.OutlineColor.Value);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }, typeof(Color));

        GUILayout.BeginHorizontal();
        GUILayout.Label(Main.Lang.Get("ALIGNMENT", "Alignment"));
        if(Drawer.DrawEnumPlus(ref model.Alignment, TranslateTextAlignment)) {
            changed = true;
            if(Main.Settings.AutoPivot || !IsAdvensedMode) {
                model.Pivot.Value = MiscUtils.AlignmentToPivot(model.Alignment);
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(Drawer.DrawAlignment(ref model.Alignment)) {
            changed = true;
            if(Main.Settings.AutoPivot || !IsAdvensedMode) {
                model.Pivot.Value = MiscUtils.AlignmentToPivot(model.Alignment);
            }
        }

        changed |= Drawer.DrawCodeEditor(Drawer.Icon_Play, Main.Lang.Get("PLAYING_TEXT", "Playing Text"), model.Name + "PlayingText", ref model.PlayingText);
        changed |= Drawer.DrawCodeEditor(Drawer.Icon_Pause, Main.Lang.Get("NOT_PLAYING_TEXT", "Not Playing Text"), model.Name + "NotPlayingText", ref model.NotPlayingText);
        GUILayout.BeginHorizontal();
        GUI.color = new Color(1f, 0.8f, 1f);
        if(Drawer.Button(Drawer.Icon_Up, GUILayout.Width(46))) {
            Task.Run(() => {
                string target = StandaloneFileBrowser.SaveFilePanel(
                    Main.Lang.Get("EXPORT_TEXT_CONFIG", "Export Text Config"),
                    Persistence.GetLastUsedFolder(),
                    $"{model.Name}.json", "json"
                );
                if(!string.IsNullOrWhiteSpace(target)) {
                    JObject node = model.Serialize() as JObject;
                    node["Type"] = "Text";
                    if(Main.Settings.IncludeReferences) {
                        node["References"] = TextConfigImporter.GetReferences(model);
                    }
                    File.WriteAllText(target, JsonConvert.SerializeObject(node, Formatting.Indented));
                }
            });
        }
        GUI.color = new Color(1f, 0.8f, 0.8f);
        if(Drawer.Button(Drawer.Icon_X, GUILayout.Width(46))) {
            text.Parent.ObjectManager.Destroy(text);
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
