using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using Overlayer.Models;
using Overlayer.Utils;
using UnityModManagerNet;

namespace Overlayer;

public class Settings : UnityModManager.ModSettings, IModel, ICopyable<Settings> {
    public enum EditorUIMode {
        Simple,
        Advanced
    }

    public bool DisableLogo = false;
    public bool ChangeFont = false;
    public FontMeta AdofaiFont = new();
    public string Lang = "Default";
    public float FPSUpdateRate = 100;
    public float FrameTimeUpdateRate = 100;
    public int SystemTagUpdateRate = 100;
    public bool UseLegacyTheme = false;
    public bool UseShowTrueAutoJudgment = false;
    public bool UseMovingManEditor = true;
    public bool UseColorRangeEditor = true;
    public bool UseEasedValueEditor = true;
    public bool UseAutoUpdate = false;
    public bool UseAutoUpdateBeta = false;
    public bool UseTooltip = true;
    public bool AutoPivot = true;
    public bool ShowTextNameAsDisplayText = false;
    public EditorUIMode UiMode = EditorUIMode.Simple;
    public bool IsFirstEg = true;
    public JToken Serialize() {
        var node = new JObject {
            [nameof(DisableLogo)] = DisableLogo,
            [nameof(ChangeFont)] = ChangeFont,
            [nameof(AdofaiFont)] = AdofaiFont?.Serialize(),
            [nameof(Lang)] = Lang,
            [nameof(FPSUpdateRate)] = FPSUpdateRate,
            [nameof(FrameTimeUpdateRate)] = FrameTimeUpdateRate,
            [nameof(SystemTagUpdateRate)] = SystemTagUpdateRate,
            [nameof(UseLegacyTheme)] = UseLegacyTheme,
            [nameof(UseShowTrueAutoJudgment)] = UseShowTrueAutoJudgment,
            [nameof(UseMovingManEditor)] = UseMovingManEditor,
            [nameof(UseColorRangeEditor)] = UseColorRangeEditor,
            [nameof(UseEasedValueEditor)] = UseEasedValueEditor,
            [nameof(UseAutoUpdate)] = UseAutoUpdate,
            [nameof(UseAutoUpdateBeta)] = UseAutoUpdateBeta,
            [nameof(UseTooltip)] = UseTooltip,
            [nameof(AutoPivot)] = AutoPivot,
            [nameof(ShowTextNameAsDisplayText)] = ShowTextNameAsDisplayText,
            [nameof(UiMode)] = UiMode.ToString(),
            [nameof(IsFirstEg)] = IsFirstEg
        };
        return node;
    }
    public void Deserialize(JToken node) {
        var defaultSettings = new Settings();

        DisableLogo = LegacyGet(node, nameof(DisableLogo))?.Value<bool>() ?? defaultSettings.DisableLogo;
        ChangeFont = node[nameof(ChangeFont)]?.Value<bool>() ?? defaultSettings.ChangeFont;
        AdofaiFont = node[nameof(AdofaiFont)] != null
            ? ModelUtils.Unbox<FontMeta>(node[nameof(AdofaiFont)])
            : defaultSettings.AdofaiFont;
        Lang = node[nameof(Lang)]?.Value<string>() ?? defaultSettings.Lang;
        FPSUpdateRate = node[nameof(FPSUpdateRate)]?.Value<float>() ?? defaultSettings.FPSUpdateRate;
        FrameTimeUpdateRate = node[nameof(FrameTimeUpdateRate)]?.Value<float>() ?? defaultSettings.FrameTimeUpdateRate;
        SystemTagUpdateRate = node[nameof(SystemTagUpdateRate)]?.Value<int>() ?? defaultSettings.SystemTagUpdateRate;
        UseLegacyTheme = LegacyGet(node, nameof(UseLegacyTheme))?.Value<bool>() ?? defaultSettings.UseLegacyTheme;
        UseShowTrueAutoJudgment = LegacyGet(node, nameof(UseShowTrueAutoJudgment))?.Value<bool>() ?? defaultSettings.UseShowTrueAutoJudgment;
        UseMovingManEditor = LegacyGet(node, nameof(UseMovingManEditor))?.Value<bool>() ?? defaultSettings.UseMovingManEditor;
        UseColorRangeEditor = LegacyGet(node, nameof(UseColorRangeEditor))?.Value<bool>() ?? defaultSettings.UseColorRangeEditor;
        UseEasedValueEditor = LegacyGet(node, nameof(UseEasedValueEditor))?.Value<bool>() ?? defaultSettings.UseEasedValueEditor;
        UseAutoUpdate = LegacyGet(node, nameof(UseAutoUpdate))?.Value<bool>() ?? defaultSettings.UseAutoUpdate;
        UseAutoUpdateBeta = LegacyGet(node, nameof(UseAutoUpdateBeta))?.Value<bool>() ?? defaultSettings.UseAutoUpdateBeta;
        UseTooltip = LegacyGet(node, nameof(UseTooltip))?.Value<bool>() ?? defaultSettings.UseTooltip;
        AutoPivot = LegacyGet(node, nameof(AutoPivot))?.Value<bool>() ?? defaultSettings.AutoPivot;
        ShowTextNameAsDisplayText = LegacyGet(node, nameof(ShowTextNameAsDisplayText))?.Value<bool>() ?? defaultSettings.ShowTextNameAsDisplayText;
        UiMode = EnumHelper<EditorUIMode>.Parse(LegacyGet(node, nameof(UiMode))?.Value<string>() ?? defaultSettings.UiMode.ToString());

        IsFirstEg = LegacyGet(node, nameof(IsFirstEg))?.Value<bool>() ?? defaultSettings.IsFirstEg;
    }
    public Settings Copy() {
        var newSettings = new Settings {
            DisableLogo = DisableLogo,
            ChangeFont = ChangeFont,
            AdofaiFont = AdofaiFont.Copy(),
            Lang = Lang,
            FPSUpdateRate = FPSUpdateRate,
            FrameTimeUpdateRate = FrameTimeUpdateRate,
            SystemTagUpdateRate = SystemTagUpdateRate,
            UseLegacyTheme = UseLegacyTheme,
            UseShowTrueAutoJudgment = UseShowTrueAutoJudgment,
            UseMovingManEditor = UseMovingManEditor,
            UseColorRangeEditor = UseColorRangeEditor,
            UseEasedValueEditor = UseEasedValueEditor,
            UseAutoUpdate = UseAutoUpdate,
            UseAutoUpdateBeta = UseAutoUpdateBeta,
            AutoPivot = AutoPivot,
            ShowTextNameAsDisplayText = ShowTextNameAsDisplayText,
            IsFirstEg = IsFirstEg
        };
        return newSettings;
    }

    public static JToken LegacyGet(JToken node, string name) {
        return node[name] ?? node[char.ToLower(name[0]) + name.Substring(1)];
    }
}
