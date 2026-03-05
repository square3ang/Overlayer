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

    public bool disableLogo = false;
    public bool ChangeFont = false;
    public FontMeta AdofaiFont = new();
    public string Lang = "Default";
    public float FPSUpdateRate = 100;
    public float FrameTimeUpdateRate = 100;
    public int SystemTagUpdateRate = 100;
    public bool useLegacyTheme = false;
    public bool useShowTrueAutoJudgment = false;
    public bool useMovingManEditor = true;
    public bool useColorRangeEditor = true;
    public bool useEasedValueEditor = true;
    public bool useAutoUpdate = false;
    public bool useAutoUpdateBeta = false;
    public bool useTooltip = true;
    public bool autoPivot = true;
    public bool showTextNameAsDisplayText = false;
    public EditorUIMode uiMode = EditorUIMode.Simple;
    public bool isFirstEg = true;
    public JToken Serialize() {
        var node = new JObject {
            [nameof(disableLogo)] = disableLogo,
            [nameof(ChangeFont)] = ChangeFont,
            [nameof(AdofaiFont)] = AdofaiFont?.Serialize(),
            [nameof(Lang)] = Lang,
            [nameof(FPSUpdateRate)] = FPSUpdateRate,
            [nameof(FrameTimeUpdateRate)] = FrameTimeUpdateRate,
            [nameof(SystemTagUpdateRate)] = SystemTagUpdateRate,
            [nameof(useLegacyTheme)] = useLegacyTheme,
            [nameof(useShowTrueAutoJudgment)] = useShowTrueAutoJudgment,
            [nameof(useMovingManEditor)] = useMovingManEditor,
            [nameof(useColorRangeEditor)] = useColorRangeEditor,
            [nameof(useEasedValueEditor)] = useEasedValueEditor,
            [nameof(useAutoUpdate)] = useAutoUpdate,
            [nameof(useAutoUpdateBeta)] = useAutoUpdateBeta,
            [nameof(useTooltip)] = useTooltip,
            [nameof(autoPivot)] = autoPivot,
            [nameof(showTextNameAsDisplayText)] = showTextNameAsDisplayText,
            [nameof(uiMode)] = uiMode.ToString(),
            [nameof(isFirstEg)] = isFirstEg
        };
        return node;
    }
    public void Deserialize(JToken node) {
        var defaultSettings = new Settings();

        disableLogo = node[nameof(disableLogo)]?.Value<bool>() ?? defaultSettings.disableLogo;
        ChangeFont = node[nameof(ChangeFont)]?.Value<bool>() ?? defaultSettings.ChangeFont;
        AdofaiFont = node[nameof(AdofaiFont)] != null
            ? ModelUtils.Unbox<FontMeta>(node[nameof(AdofaiFont)])
            : defaultSettings.AdofaiFont;
        Lang = node[nameof(Lang)]?.Value<string>() ?? defaultSettings.Lang;
        FPSUpdateRate = node[nameof(FPSUpdateRate)]?.Value<float>() ?? defaultSettings.FPSUpdateRate;
        FrameTimeUpdateRate = node[nameof(FrameTimeUpdateRate)]?.Value<float>() ?? defaultSettings.FrameTimeUpdateRate;
        SystemTagUpdateRate = node[nameof(SystemTagUpdateRate)]?.Value<int>() ?? defaultSettings.SystemTagUpdateRate;
        useLegacyTheme = node[nameof(useLegacyTheme)]?.Value<bool>() ?? defaultSettings.useLegacyTheme;
        useShowTrueAutoJudgment = node[nameof(useShowTrueAutoJudgment)]?.Value<bool>() ?? defaultSettings.useShowTrueAutoJudgment;
        useMovingManEditor = node[nameof(useMovingManEditor)]?.Value<bool>() ?? defaultSettings.useMovingManEditor;
        useColorRangeEditor = node[nameof(useColorRangeEditor)]?.Value<bool>() ?? defaultSettings.useColorRangeEditor;
        useEasedValueEditor = node[nameof(useEasedValueEditor)]?.Value<bool>() ?? defaultSettings.useEasedValueEditor;
        useAutoUpdate = node[nameof(useAutoUpdate)]?.Value<bool>() ?? defaultSettings.useAutoUpdate;
        useAutoUpdateBeta = node[nameof(useAutoUpdateBeta)]?.Value<bool>() ?? defaultSettings.useAutoUpdateBeta;
        useTooltip = node[nameof(useTooltip)]?.Value<bool>() ?? defaultSettings.useTooltip;
        autoPivot = node[nameof(autoPivot)]?.Value<bool>() ?? defaultSettings.autoPivot;
        showTextNameAsDisplayText = node[nameof(showTextNameAsDisplayText)]?.Value<bool>() ?? defaultSettings.showTextNameAsDisplayText;
        uiMode = EnumHelper<EditorUIMode>.Parse(node[nameof(uiMode)]?.Value<string>() ?? defaultSettings.uiMode.ToString());

        isFirstEg = node[nameof(isFirstEg)]?.Value<bool>() ?? defaultSettings.isFirstEg;
    }
    public Settings Copy() {
        var newSettings = new Settings {
            disableLogo = disableLogo,
            ChangeFont = ChangeFont,
            AdofaiFont = AdofaiFont.Copy(),
            Lang = Lang,
            FPSUpdateRate = FPSUpdateRate,
            FrameTimeUpdateRate = FrameTimeUpdateRate,
            SystemTagUpdateRate = SystemTagUpdateRate,
            useLegacyTheme = useLegacyTheme,
            useShowTrueAutoJudgment = useShowTrueAutoJudgment,
            useMovingManEditor = useMovingManEditor,
            useColorRangeEditor = useColorRangeEditor,
            useEasedValueEditor = useEasedValueEditor,
            useAutoUpdate = useAutoUpdate,
            useAutoUpdateBeta = useAutoUpdateBeta,
            autoPivot = autoPivot,
            showTextNameAsDisplayText = showTextNameAsDisplayText,
            isFirstEg = isFirstEg
        };
        return newSettings;
    }
}
