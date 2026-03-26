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
    public FontMeta AdofaiFont = new();
    public string Lang = "en-US";
    public float FPSUpdateRate = 100;
    public float FrameTimeUpdateRate = 100;
    public int SystemTagUpdateRate = 100;
    public bool LegacyTheme = false;
    public bool MovingManEditor = true;
    public bool ColorRangeEditor = true;
    public bool EasedValueEditor = true;
    public bool AutoUpdate = false;
    public bool AutoUpdateBeta = false;
    public bool Tooltip = true;
    public bool AutoPivot = true;
    public bool IncludeReferences = true;
    public bool ShowTextNameAsDisplayText = false;
    public EditorUIMode UiMode = EditorUIMode.Simple;

    public bool ChangeFont = false;
    public bool UseShowTrueAutoJudgment = false;

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
            [nameof(LegacyTheme)] = LegacyTheme,
            [nameof(UseShowTrueAutoJudgment)] = UseShowTrueAutoJudgment,
            [nameof(MovingManEditor)] = MovingManEditor,
            [nameof(ColorRangeEditor)] = ColorRangeEditor,
            [nameof(EasedValueEditor)] = EasedValueEditor,
            [nameof(AutoUpdate)] = AutoUpdate,
            [nameof(AutoUpdateBeta)] = AutoUpdateBeta,
            [nameof(Tooltip)] = Tooltip,
            [nameof(AutoPivot)] = AutoPivot,
            [nameof(ShowTextNameAsDisplayText)] = ShowTextNameAsDisplayText,
            [nameof(IncludeReferences)] = IncludeReferences,
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
        LegacyTheme = LegacyUseGet(node, nameof(LegacyTheme))?.Value<bool>() ?? defaultSettings.LegacyTheme;
        UseShowTrueAutoJudgment = LegacyUseGet(node, nameof(UseShowTrueAutoJudgment))?.Value<bool>() ?? defaultSettings.UseShowTrueAutoJudgment;
        MovingManEditor = LegacyUseGet(node, nameof(MovingManEditor))?.Value<bool>() ?? defaultSettings.MovingManEditor;
        ColorRangeEditor = LegacyUseGet(node, nameof(ColorRangeEditor))?.Value<bool>() ?? defaultSettings.ColorRangeEditor;
        EasedValueEditor = LegacyUseGet(node, nameof(EasedValueEditor))?.Value<bool>() ?? defaultSettings.EasedValueEditor;
        AutoUpdate = LegacyUseGet(node, nameof(AutoUpdate))?.Value<bool>() ?? defaultSettings.AutoUpdate;
        AutoUpdateBeta = LegacyUseGet(node, nameof(AutoUpdateBeta))?.Value<bool>() ?? defaultSettings.AutoUpdateBeta;
        Tooltip = LegacyUseGet(node, nameof(Tooltip))?.Value<bool>() ?? defaultSettings.Tooltip;
        AutoPivot = LegacyGet(node, nameof(AutoPivot))?.Value<bool>() ?? defaultSettings.AutoPivot;
        IncludeReferences = node[nameof(IncludeReferences)]?.Value<bool>() ?? defaultSettings.IncludeReferences;
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
            LegacyTheme = LegacyTheme,
            UseShowTrueAutoJudgment = UseShowTrueAutoJudgment,
            MovingManEditor = MovingManEditor,
            ColorRangeEditor = ColorRangeEditor,
            EasedValueEditor = EasedValueEditor,
            AutoUpdate = AutoUpdate,
            AutoUpdateBeta = AutoUpdateBeta,
            AutoPivot = AutoPivot,
            IncludeReferences = IncludeReferences,
            ShowTextNameAsDisplayText = ShowTextNameAsDisplayText,
            IsFirstEg = IsFirstEg
        };
        return newSettings;
    }

    public static JToken LegacyGet(JToken node, string name) {
        return node[name] ?? node[char.ToLower(name[0]) + name.Substring(1)];
    }

    public static JToken LegacyUseGet(JToken node, string name) {
        return node[name] ?? node["use"+name];
    }
}
